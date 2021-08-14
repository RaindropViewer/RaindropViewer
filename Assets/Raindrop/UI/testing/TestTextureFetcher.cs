using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Raindrop
{
    // contains a function and mapfetcher for fetching and decoding the map tile. does not show the image.

    class TestTextureFetcher : MonoBehaviour
    {
        [SerializeField]
        public MapLogic.MapFetcher mapFetcher; //fetching logic + pooling data here
        

        private void Awake()
        {
            MainThreadDispatcher.Initialize();
            mapFetcher = new MapLogic.MapFetcher();
        }


        public void testfetch()
        {

            //IAsyncResult result = request.BeginGetResponse(Finish, request);
            //ServicePointManager.ServerCertificateValidationCallback =
            //    (sender, certificate, chain, sslPolicyErrors) => true;


            var handle = Utils.UIntsToLong(256 * 1000, 256 * 1000);
            mapFetcher.GetRegionTileExternal(handle, 1);
            Debug.Log("end of initial fetch call.");
        }


    }
}
