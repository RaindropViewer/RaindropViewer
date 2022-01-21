using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using Raindrop.Map;
using Raindrop.Map.Model;

namespace Raindrop
{
    // contains a function and mapfetcher for fetching and decoding the map tile. does not show the image.

    class TestTextureFetcher : MonoBehaviour
    {
        [SerializeField]
        public MapService MapService; //fetching logic + pooling data here
        

        private void Awake()
        {
            MainThreadDispatcher.Initialize();
            MapService = new MapService();
        }


        public void testfetch()
        {

            //IAsyncResult result = request.BeginGetResponse(Finish, request);
            //ServicePointManager.ServerCertificateValidationCallback =
            //    (sender, certificate, chain, sslPolicyErrors) => true;


            var handle = Utils.UIntsToLong(256 * 1000, 256 * 1000);
            throw new NotImplementedException("we recently refactored the mapservice interface.");
            // MapService.GetRegionTileExternal(handle, 1);
            Debug.Log("end of initial fetch call.");
        }


    }
}
