using System;
using System.Collections;
using System.IO;
using System.Net;
using NUnit.Framework;
using OpenMetaverse;
using OpenMetaverse.Assets;
using UnityEngine;
using UnityEngine.TestTools;
using Disk;
using OpenMetaverse.Http;
using Plugins.ObjectPool;
using Tests;

namespace Raindrop.Tests.LMV_ExtendedTests
{
    [TestFixture]
    public class LMVLibraryImageFetchingTests
    {
        private static RaindropInstance instance;

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            instance.CleanUp();
        }
        
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {    
            //make the main thread dispatcher
            GameObject mainThreadDispatcher = 
                new GameObject("mainThreadDispatcher");
            mainThreadDispatcher.AddComponent<UnityMainThreadDispatcher>();
            
            instance = new RaindropInstance(new GridClient());
        }
        

        /* DownloadManager variant of 'wtf' WebRequest.
         * (Used to) Fail in unity editor, android target, net standard 2.0
         * (Used to) unity completely hang at call to
         * HttpWebRequest.BeginGetResponse() 
         * 
         * For full information:
         * https://github.com/cinderblocks/libremetaverse/issues/62
         */
        [UnityTest]
        public IEnumerator wtf1_WebRequest_BeginGetResponse_PASS()
        {
            DownloadManager dl = new DownloadManager();
            dl.QueueDownload(new DownloadRequest(
                new Uri(string.Format(
                    "http://map.secondlife.com/map-{0}-{1}-{2}-objects.jpg", 
                    1, 1000, 1000)),
                5000,
                null,
                null,
                (request, response, responseData, error) =>
                {
                    Debug.Log(("there was a reply"));
                }
            ));
            
            yield break;
        }
        
        
        /* simple variant of 'wtf' WebRequest.
        * Passes in unity editor, android target, net standard 2.0
        * this call to HttpWebRequest.BeginGetResponse() is ok, does not hang 
        * note that some sort of exception was raised somewhere and the
        * debugger break for it... but not relevant. this passes.
        *
        * For full information:
        * https://github.com/cinderblocks/libremetaverse/issues/62
        */
        [UnityTest]
        public IEnumerator wtf2_WebRequest_BeginGetResponse_PASS()
        {
            var request = HttpWebRequest.Create(
                new Uri(string.Format(
                    "http://map.secondlife.com/map-{0}-{1}-{2}-objects.jpg",
                    1, 1000, 1000))
            );

            request.BeginGetResponse(
                new AsyncCallback(FinishWebRequest), 
                request);

            yield return new WaitForSeconds(10);
            yield break;
            
        
            void FinishWebRequest(IAsyncResult ar)
            {
                Debug.Log("net done.");

            }
        }

        // Download a Maptile, via SL's External API.
        // Implementation: using DownloadManager class.
        [UnityTest]
        public IEnumerator Test_DownloadMaptile_DownloadManager()
        {
            DownloadManager dlm = new DownloadManager();
            
            // var request = (HttpWebRequest)HttpWebRequest.Create(
            //     new Uri(string.Format(
            // "http://map.secondlife.com/map-{0}-{1}-{2}-objects.jpg",
            // 1, 1000, 1000))
            // );
            DownloadRequest req = new DownloadRequest(
                new Uri(string.Format(
                    "http://map.secondlife.com/map-{0}-{1}-{2}-objects.jpg", 
                    1, 1000, 1000)),
                20 * 1000,
                null,
                null,
                (request, response, responseData, error) =>
                {
                    if (error == null && responseData != null) // success
                    {
                        Debug.Log("Download Success.");
                        Assert.Pass();
                    }
                    else // download failed
                    {
                        Debug.Log("Download failed.");
                        Assert.Fail();
                    }
                }
            );
            
            dlm.QueueDownload(req);
            
            yield return new WaitForSeconds(10);
        }

        
        // Download a Maptile, via SL's External API.
        // Implementation: BeginGetResponse, HttpWebRequest
        [UnityTest]
        public IEnumerator Test_DownloadMapTile_SimpleImplementation()
        {

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(
                new Uri(string.Format(
                    "http://map.secondlife.com/map-{0}-{1}-{2}-objects.jpg", 
                    1, 1000, 1000))
            );
            
            RequestState state = new RequestState(request,
                null, 
                20*1000, 
                null,
                null,
                null);

            // Start the request for the remote server response
            IAsyncResult result = request.BeginGetResponse(GetResponse, state);
            
            yield return new WaitForSeconds(10);
            yield break;
            // hack function - implementation is in CapsBase
            void GetResponse(IAsyncResult ar)
            {
                var state = (RequestState)ar.AsyncState;
                HttpWebResponse response = null;
                byte[] responseData = null;
                Exception error = null;

                try
                {
                    using (response = 
                               (HttpWebResponse)state.Request.EndGetResponse(ar))
                    {
                        // Get the stream for downloading the response
                        using (var responseStream = response.GetResponseStream())
                        {
                            #region Read the response

                            // If Content-Length is set,
                            // we create a buffer of the exact size,
                            // otherwise,
                            // a MemoryStream is used to receive the response
                            bool nolength = (response.ContentLength <= 0) || 
                                            (Type.GetType("Mono.Runtime") != null);
                            int size = (nolength) 
                                ? 8192 
                                : (int)response.ContentLength;
                            MemoryStream ms = (nolength)
                                ? new MemoryStream()
                                : null;
                            byte[] buffer = new byte[size];

                            int bytesRead = 0;
                            int offset = 0;
                            int totalBytesRead = 0;
                            int totalSize = nolength ? 0 : size;

                            while (responseStream != null && 
                                   (bytesRead = responseStream.Read(buffer, offset, size)) != 0)
                            {
                                totalBytesRead += bytesRead;

                                if (nolength)
                                {
                                    totalSize += (size - bytesRead);
                                    ms.Write(buffer, 0, bytesRead);
                                }
                                else
                                {
                                    offset += bytesRead;
                                    size -= bytesRead;
                                }

                                // Fire the download progress callback for
                                // each chunk of received data
                                if (state.DownloadProgressCallback != null)
                                    state.DownloadProgressCallback(
                                        state.Request,
                                        response,
                                        totalBytesRead,
                                        totalSize);
                            }

                            if (nolength)
                            {
                                responseData = ms.ToArray();
                                ms.Close();
                                ms.Dispose();
                            }
                            else
                            {
                                responseData = buffer;
                            }

                            #endregion Read the response
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Logger.DebugLog("CapsBase.GetResponse(): " + ex.Message);
                    error = ex;
                }
                state.CompletedCallback?.Invoke(state.Request, response, responseData, error);
            }
        }
        

        /* After much confusion, it seems that the line
         * request.ServicePoint.MaxIdleTime = 0;
         * is causing the issue.
         *
         * For full information:
         * https://github.com/cinderblocks/libremetaverse/issues/62
         * 
         */
        /*
        //to run this test, make the SetupRequest public.
        // WARN: important to use SetupRequest to reproduce deadlock; 
        // if i construct the HttpWebRequest directly, its all ok.
        [UnityTest]
        public IEnumerator SetupRequestMethod_IsCausingMainThreadToLockup()
        {
            var dlm = new DownloadManager();
            HttpWebRequest req;
            bool passTheTest_YesPlease = false;
            if (passTheTest_YesPlease == true)
            {
                req = (HttpWebRequest) HttpWebRequest.Create( 
                    new Uri(string.Format(
                    "http://map.secondlife.com/map-{0}-{1}-{2}-objects.jpg", 
                    1, 1000, 1000))
                );
                req.Accept = null;

            }else{
                req = dlm.SetupRequest( // <-- setuprequest is required to
                                        // replicate deadlock
                    new Uri(string.Format(
                    "http://map.secondlife.com/map-{0}-{1}-{2}-objects.jpg",
                     1, 1000, 1000))
                    , null);
            }

            CapsBase.DownloadDataAsync(
                req,
                20*1000,
                (request, response, bytesReceived, totalBytesToReceive) =>
                {
                    Debug.Log("ok no blocking - progression callback");
                },
                (request, response, responseData, error) =>
                {
                    Debug.Log("ok no blocking - completed callback.");
                }
            );

            yield break;
        }
        */

        // able to login and grab a user's profile pic and save to disk
        [Test]
        public IEnumerator LoginAndDownloadJ2P()
        {
            Helpers.LoginHeadless(instance, 0, "Hooper");

            yield return new WaitForSeconds(15);
            
            //request that image (using DownloadManager)
            instance.Client.Assets.RequestImage(UUID.Parse(
                    "ed891aaa-b031-cd08-77d7-4d0a15a2b8c5"), //nuki face. SL.
                ImageType.Normal,
                Callback_DecodeAndSaveFileAsPNG,
                false);

            yield return new WaitForSeconds(10);
            instance.Client.Network.Logout();
            yield return new WaitForSeconds(10);

            yield break;
        }

        // Callback for DownloadManager.
        // It will fetch and save the desired image as .png. 
        private void Callback_DecodeAndSaveFileAsPNG(
            TextureRequestState state, 
            AssetTexture assettexture)
        {
            //assert download is finished
            Debug.Log("downloaded bytes: " + assettexture.AssetData.Length);
            Assert.True(state == TextureRequestState.Finished);
            
            //save image as jp2
            var basepath = instance.ClientDir + "/test_cache/";
            var relativepath1 = assettexture.AssetID.ToString() +  ".jp2";
            DirectoryHelpers.WriteToFile(
                assettexture.AssetData, 
                Path.Combine(basepath, relativepath1));
            
            // image encoding conversion:
            // j2p -> t2d -> png 
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                // j2p -> t2d -> MI (RGB bitmap) 
                assettexture.Decode(); 
                
                var t2d = 
                    TexturePoolSelfImpl.GetInstance().
                        GetFromPool(TextureFormat.ARGB32);
                
                // MI (RGB bitmap) -> Texture2D
                assettexture.Image.ExportTex2D(t2d);
                
                var bytes = t2d.EncodeToPNG(); 
            
                // save that image as png
                // WARN: this is a image with BGR channels.
                var relativepath2 = 
                    assettexture.AssetID.ToString() +  ".png";
                    
                DirectoryHelpers.WriteToFile(
                    bytes,  
                    Path.Combine(basepath, relativepath2));
                
            });

        }
    }
    
    public class RequestState
    {
        public HttpWebRequest Request;
        public byte[] UploadData;
        public int MillisecondsTimeout;
        public CapsBase.OpenWriteEventHandler OpenWriteCallback;
        public CapsBase.DownloadProgressEventHandler DownloadProgressCallback;
        public CapsBase.RequestCompletedEventHandler CompletedCallback;

        public RequestState(HttpWebRequest request, byte[] uploadData, int millisecondsTimeout, CapsBase.OpenWriteEventHandler openWriteCallback,
            CapsBase.DownloadProgressEventHandler downloadProgressCallback, CapsBase.RequestCompletedEventHandler completedCallback)
        {
            Request = request;
            UploadData = uploadData;
            MillisecondsTimeout = millisecondsTimeout;
            OpenWriteCallback = openWriteCallback;
            DownloadProgressCallback = downloadProgressCallback;
            CompletedCallback = completedCallback;
        }
        
    }
    
}