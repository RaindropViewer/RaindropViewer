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


            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://www.google.com");
            //request.BeginGetResponse(new AsyncCallback(Finish), request);;

            var handle = Utils.UIntsToLong(256 * 1000, 256 * 1000);
            mapFetcher.GetRegionTileExternal(handle, 1);
        }



        private void Finish(IAsyncResult result)
        {
            Debug.Log("FINISH"); 
        }
    }
}
