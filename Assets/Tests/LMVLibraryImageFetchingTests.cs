using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using NUnit.Framework;
using OpenMetaverse;
using OpenMetaverse.Assets;
using OpenMetaverse.Packets;
using Raindrop.Netcom;
using Raindrop.Services.Bootstrap;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using System.Net;
using OpenMetaverse.Http;
using Raindrop.Map.Model;
using Raindrop.ServiceLocator;

namespace Raindrop.Tests
{
    [TestFixture]
    public class LMVLibraryImageFetchingTests
    {
        private static RaindropInstance instance;

        void RaindropInit()
        {
            //make the main thread dispatcher
            GameObject mainThreadDispatcher_Test = new GameObject("mainThreadDispatcher_Test");
            mainThreadDispatcher_Test.AddComponent<UnityMainThreadDispatcher>();
            
            instance = new RaindropInstance(new GridClient());
            // Client.Self.Movement.Fly = true;
            // Register callbacks
            // Client.Network.RegisterCallback(PacketType.ObjectUpdate, ObjectUpdateHandler);
            //Client.Self.OnTeleport += new MainAvatar.TeleportCallback(OnTeleportHandler)
        }
        
        void login()
        {
            // required to prevent throwing exception later on:
            // some message like:   Unhandled log message: '[Error] 22:34:22 [ERROR] - <TanukiDEV Resident>: Setting server side baking failed'. Use UnityEngine.TestTools.LogAssert.Expect
            instance.Client.Settings.SEND_AGENT_APPEARANCE = false;
            
            var fullusername = "***REMOVED*** resident"; //Environment.GetEnvironmentVariable("LMVTestAgentUsername");
            var password = "***REMOVED***"; // Environment.GetEnvironmentVariable("LMVTestAgentPassword");
            Assert.IsFalse(string.IsNullOrWhiteSpace(fullusername),
                "LMVTestAgentUsername is empty. Live NetworkTests cannot be performed.");
            Assert.IsFalse(string.IsNullOrWhiteSpace(password),
                "LMVTestAgentPassword is empty. Live NetworkTests cannot be performed.");
            var username = fullusername.Split(' ');

            Debug.Log($"Logging in {fullusername}...");
            // Connect to the grid
            string startLoc = NetworkManager.StartLocation("Hooper", 179, 18, 32);
            //changed:
            Debug.Log("startLoc : " + startLoc);
            Assert.IsTrue(instance.Client.Network.Login(username[0], username[1], password, "Unit Test Framework", startLoc,
                "contact@openmetaverse.co"), "Client failed to login, reason: " + instance.Client.Network.LoginMessage);
            //changed:
            Debug.Log("login message: " + instance.Client.Network.LoginMessage);
            Debug.Log("Done");

            Assert.IsTrue(instance.Client.Network.Connected, "Client is not connected to the grid");
        }

        
        // DownloadManager variant of 'wtf' WebRequest.
        // (Used to) Fail in unity editor, android target, net standard 2.0
        // (Used to) unity completely hang at call to HttpWebRequest.BeginGetResponse()  
        [UnityTest]
        public IEnumerator wtf1_WebRequest_BeginGetResponse_PASS()
        {
            DownloadManager dl = new DownloadManager();
            dl.QueueDownload(new DownloadRequest(
                new Uri(string.Format("http://map.secondlife.com/map-{0}-{1}-{2}-objects.jpg", 1, 1000, 1000)),
                5000,
                null,
                null,
                (request, response, responseData, error) =>
                {
                    Debug.Log(("there was a reply"));
                }
            ));
            
            Assert.Pass();
            yield break;
        }
        
        
        // simple variant of 'wtf' WebRequest.
        // Passes in unity editor, android target, net standard 2.0
        // this call to HttpWebRequest.BeginGetResponse() is ok, does not hang 
        // note that some sort of exception was raised somewhere and the debugger break for it... but not relevant. this passes.
        [UnityTest]
        public IEnumerator wtf2_WebRequest_BeginGetResponse_PASS()
        {
            var request = HttpWebRequest.Create(
                new Uri(string.Format("http://map.secondlife.com/map-{0}-{1}-{2}-objects.jpg", 1, 1000, 1000))
            );

            request.BeginGetResponse(new AsyncCallback(FinishWebRequest), request);

            yield return new WaitForSeconds(10);
            yield break;
            
        
            void FinishWebRequest(IAsyncResult ar)
            {
                Debug.Log("net done.");
            
                Assert.Pass();

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

        // test that the (external) map fetcher code is working.
        [UnityTest]
        public IEnumerator Test_MapService_SingleTile()
        {
            Raindrop.Unity.SceneBootstrapperGenerator.Init();
            yield return new WaitForSeconds(2);
            Assert.True(ServiceLocator.ServiceLocator.Instance != null);
            Raindrop.Unity.SceneBootstrapperGenerator.AddMainThreadDispatcher();

            BeginMapFetcher();
            var fetcher = ServiceLocator.ServiceLocator.Instance.Get<MapService>();

            //initally, the maptile is just a empty pocket
            bool isReady;
            var emptyTile = fetcher.GetMapTile(Utils.UIntsToLong(1000*256,1000*256) , 1, out isReady);
            Assert.False(isReady);
            
            // wait for the maptile to be filled by the network.
            yield return new WaitForSeconds(10);
            Assert.True(emptyTile.isReady, "tile should be ready after 10s.");
            //maptile should be ready in memory.
            Assert.True(emptyTile.getTex().width > 10);
            
            var secondRequestForSameTile = 
                fetcher.GetMapTile(Utils.UIntsToLong(1000*256,1000*256) , 1, out isReady);
            Assert.True(emptyTile.isReady, "tile should be ready in subsequent fetch");

            Assert.Pass();
            
            void BeginMapFetcher()
            {
                if (!ServiceLocator.ServiceLocator.Instance.IsRegistered<MapService>())
                {
                    Debug.Log("UIBootstrapper creating and registering MapBackend.MapFetcher!");
                    ServiceLocator.ServiceLocator.Instance.Register<MapService>(new MapService());
                    //return;
                }
            }

        }

        //able to login and grab a user's profile pic and save to disk
        [UnityTest]
        public IEnumerator LoginAndDownloadJ2P()
        {
            RaindropInit();
            login();

            yield return new WaitForSeconds(15);
            
            //request that image (using DownloadManager)
            instance.Client.Assets.RequestImage(UUID.Parse(
                    "ed891aaa-b031-cd08-77d7-4d0a15a2b8c5"),
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
            Debug.Log("downloaded bytes: " + assettexture.AssetData.Length);
            if (state != TextureRequestState.Finished)
            {
                Assert.Fail();
            }

            //save that image, j2p
            var basepath = instance.ClientDir + "/test_cache/";
            var relativepath1 = assettexture.AssetID.ToString() +  ".j2p";
            Helper.WriteToFile(assettexture.AssetData,  Path.Combine(basepath, relativepath1));
            
            //j2p -> t2d -> png 
            //needs to be on main thread...
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                assettexture.Decode(); //needs to be on main thread...
                var t2d = assettexture.Image.ExportTex2D();
                var bytes = t2d.EncodeToPNG();
            
                //save that image, png
                var relativepath2 = assettexture.AssetID.ToString() +  ".png";
                Helper.WriteToFile(bytes,  Path.Combine(basepath, relativepath2));
                
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