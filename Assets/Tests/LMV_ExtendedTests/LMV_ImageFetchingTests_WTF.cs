using System;
using System.Collections;
using System.Net;
using OpenMetaverse;
using UnityEngine;
using UnityEngine.TestTools;

namespace Raindrop.Tests.LMV_ExtendedTests
{
    public class LMV_ImageFetchingTests_WTF
    { 
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
    }
}