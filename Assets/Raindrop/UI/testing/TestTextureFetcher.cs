using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Raindrop
{
    class TestTextureFetcher : MonoBehaviour
    {
        [SerializeField]
        public MapLogic.MapFetcher mapFetcher;

        private void Awake()
        {

            mapFetcher = new MapLogic.MapFetcher();
        }


        public void testfetch()
        {


            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format("http://map.secondlife.com/map-1-1000-1000-objects.jpg"));
            //request.Timeout = 3 * 1000;
            //request.BeginGetResponse(new AsyncCallback(Finish), request); ;

            // Create an object to hold all of the state for this request


            //IAsyncResult result = request.BeginGetResponse(Finish, request);
            //ServicePointManager.ServerCertificateValidationCallback =
            //    (sender, certificate, chain, sslPolicyErrors) => true;


            var handle = Utils.UIntsToLong(256 * 1000, 256 * 1000);
            mapFetcher.GetRegionTileExternal(handle, 1);
            Debug.Log("end of start fetch subroutine");
        }



        private void Finish(IAsyncResult result)
        {
            Debug.Log("FINISH"); 
        }
    }
}
