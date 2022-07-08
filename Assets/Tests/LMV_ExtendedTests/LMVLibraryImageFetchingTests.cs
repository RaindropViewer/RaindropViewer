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

namespace Raindrop.Tests
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
        
        // Do login using the LMV library only.
        void LoginHeadless(int userIdx = 0, string startLocation = "Hooper")
        {
            /* hack: I added this,
             * to prevent the library from throwing the runtime exception :
             *
             * Unhandled log message: '[Error] 22:34:22 [ERROR] - <TanukiDEV
             * Resident>: Setting server side baking failed'. Use
             * UnityEngine.TestTools.LogAssert.Expect   */
             
            instance.Client.Settings.SEND_AGENT_APPEARANCE = false;

            var fullUsername = Secrets.GridUsers[userIdx];
            var password = Secrets.GridPass[userIdx];
            Assert.IsFalse(string.IsNullOrWhiteSpace(fullUsername),
                "LMVTestAgentUsername is empty. " +
                "Live NetworkTests cannot be performed.");
            Assert.IsFalse(string.IsNullOrWhiteSpace(password),
                "LMVTestAgentPassword is empty. " +
                "Live NetworkTests cannot be performed.");
            var username = fullUsername.Split(' ');

            // Connect to the grid
            string startLoc = 
                NetworkManager.StartLocation(startLocation, 179, 18, 32);
            Debug.Log($"Logging in " +
                      $"User: {fullUsername}, " +
                      $"Loc: {startLoc}");
            bool loginSuccessful = instance.Client.Network.Login(
                username[0],
                username[1],
                password,
                "Unit Test Framework",
                startLoc,
                "raindropcafeofficial@gmail.com");
            Assert.IsTrue(loginSuccessful, 
                $"Client failed to login, reason: " +
                $"{instance.Client.Network.LoginMessage}");
            Debug.Log("Grid returned the login message: " 
                      + instance.Client.Network.LoginMessage);

            Assert.IsTrue(
                instance.Client.Network.Connected, 
                "Client is not connected to the grid");
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

        // Implementation with the DownloadManager class.
        [UnityTest]
        public IEnumerator Test_DownloadMaptile_DownloadManager()
        {
            //ctor DLmanager
            DownloadManager dlm = new DownloadManager();
            
            
            // var request = (HttpWebRequest)HttpWebRequest.Create(
            //     new Uri(string.Format("http://map.secondlife.com/map-{0}-{1}-{2}-objects.jpg", 1, 1000, 1000))
            // );
            DownloadRequest req = new DownloadRequest(
                new Uri(string.Format("http://map.secondlife.com/map-{0}-{1}-{2}-objects.jpg", 1, 1000, 1000)),
                20 * 1000,
                null,
                null,
                (request, response, responseData, error) =>
                {
                    if (error == null && responseData != null) // success
                    {
                        Debug.Log("nice, dl is complete!");
                        
                        Debug.Log("storeing file as : " );
                        
                        
                    }
                    else // download failed
                    {
                        Debug.Log("nice, dl had errors, but the callback was raised anyways");
                    }
                }
            );
            
            
            dlm.QueueDownload(req);
            
            yield return new WaitForSeconds(10);
            yield break;
        }

        
        // a simple implementation without using DownloadManager class
        [UnityTest]
        public IEnumerator Test_DownloadMapTile_SimpleImplementation()
        {

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(
                new Uri(string.Format("http://map.secondlife.com/map-{0}-{1}-{2}-objects.jpg", 1, 1000, 1000))
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
                using (response = (HttpWebResponse)state.Request.EndGetResponse(ar))
                {
                    // Get the stream for downloading the response
                    using (var responseStream = response.GetResponseStream())
                    {
                        #region Read the response

                        // If Content-Length is set we create a buffer of the exact size, otherwise
                        // a MemoryStream is used to receive the response
                        bool nolength = (response.ContentLength <= 0) || (Type.GetType("Mono.Runtime") != null);
                        int size = (nolength) ? 8192 : (int)response.ContentLength;
                        MemoryStream ms = (nolength) ? new MemoryStream() : null;
                        byte[] buffer = new byte[size];

                        int bytesRead = 0;
                        int offset = 0;
                        int totalBytesRead = 0;
                        int totalSize = nolength ? 0 : size;

                        while (responseStream != null && (bytesRead = responseStream.Read(buffer, offset, size)) != 0)
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

                            // Fire the download progress callback for each chunk of received data
                            if (state.DownloadProgressCallback != null)
                                state.DownloadProgressCallback(state.Request, response, totalBytesRead, totalSize);
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
        


        // After much confusion, it seems that the line request.ServicePoint.MaxIdleTime = 0; is causing the issue.
        // to run this test, make the SetupRequest public.
        /*
        [UnityTest]
        public IEnumerator Test_TheSetupRequestMethod_IsCausingTheIssue()
        {
            var dlm = new DownloadManager();
            HttpWebRequest req;
            bool passTheTest_YesPlease = false;
            if (passTheTest_YesPlease == true)
            {
                req = (HttpWebRequest) HttpWebRequest.Create( 
                    new Uri(string.Format("http://map.secondlife.com/map-{0}-{1}-{2}-objects.jpg", 1, 1000, 1000))
                );
                req.Accept = null;

            }else{
                req = dlm.SetupRequest( // <-- important to use SetupRequest; if i construct the HttpWebRequest directly, its all ok.
                    new Uri(string.Format("http://map.secondlife.com/map-{0}-{1}-{2}-objects.jpg", 1, 1000, 1000))
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
        [UnityTest]
        public IEnumerator LoginAndDownloadJ2P()
        {
            LoginHeadless();

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

        // Callback for DownloadManager, this will Fetching and saving the desired image. 
        // this callback is known to run on non-main thread.
        private void Callback_DecodeAndSaveFileAsPNG(TextureRequestState state, AssetTexture assettexture)
        {
            //assert download is finished
            Debug.Log("downloaded bytes: " + assettexture.AssetData.Length);
            if (state != TextureRequestState.Finished)
            {
                Assert.Fail();
            }

            //save that image, jp2
            var basepath = instance.ClientDir + "/test_cache/";
            var relativepath1 = assettexture.AssetID.ToString() +  ".jp2";
            DirectoryHelpers.WriteToFile(assettexture.AssetData,  Path.Combine(basepath, relativepath1));
            
            //j2p -> t2d -> png 
            //needs to be on main thread...
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                assettexture.Decode(); //needs to be on main thread...
                var t2d = TexturePoolSelfImpl.GetInstance().GetFromPool(TextureFormat.ARGB32);
                assettexture.Image.ExportTex2D(t2d); //decode: assetData -> texture
                var bytes = t2d.EncodeToPNG(); 
            
                //save that image, png
                var relativepath2 = assettexture.AssetID.ToString() +  ".png";//this is a image with wrong channels.
                DirectoryHelpers.WriteToFile(bytes,  Path.Combine(basepath, relativepath2));
                
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