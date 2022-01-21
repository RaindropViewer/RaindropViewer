using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using Raindrop.Map;
using Raindrop.UI.Views;
using Raindrop.Netcom;
using OpenMetaverse;
using Raindrop.Map.Model;
using Vector2 = UnityEngine.Vector2;

namespace Raindrop.UI.Model
{
    /// <summary>
    /// Model
    /// 
    /// </summary>

    class MapModel
    {
        // i fetch images.
        private MapService _mapService;
        // i cache images to disk.
        private MapCache mapCache;

        #region globalrefs
        private RaindropInstance instance { get { return ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>(); } }
        private RaindropNetcom netcom { get { return instance.Netcom; } }
        private GridClient client { get { return instance.Client; } }
        bool Active => instance.Client.Network.Connected;
        #endregion

        #region instance vars
        //the current zoom level that the user is requesting.
        // 1 - one 256^2 texture is 1 sim.
        // ...
        // 4 - one 256^2 texture is 8*8 sims.
        public int zoomLevel;

        #endregion
        
        public MapModel()
        {
            //mapFetcher = UtilsUnity.ServiceLocator.Instance.Get<MapFetcher>();
            //mapFetcher = new MapBackend();
        }


        /// <summary>
        /// get a list of region handles that are visible in the specified bounds.
        /// </summary>
        /// <returns></returns>
        public HashSet<ulong> getVisibleRegionHandles(Vector2 min, Vector2 max)
        {
            // for loop from bounds to bounds.
            HashSet<ulong> visiblehandles = new HashSet<ulong>();
            int vert_min = (int)min.y;
            int vert_max = (int)max.y;
            int horz_min = (int)min.x;
            int horz_max = (int)max.x;
            for (int i = horz_min; i < horz_max; i++)
            {
                for (int j = vert_min; j < vert_max; j++)
                {
                    ulong region = OpenMetaverse.Utils.UIntsToLong((uint)(i * 256), (uint)(j * 256));
                    visiblehandles.Add(region);
                }
            }

            if (visiblehandles.Count > 30)
            {
                throw new Exception("too many tiles to downlaod bro!");
            }

            return visiblehandles;
        }



        /// <summary>
        /// Calls retrieve operation on network if tiles that are supposed to be present are not present.
        /// return true if redraw is required.
        /// </summary>
        public void RefreshMapPool(Vector2 min, Vector2 max)
        {
            var handles = getVisibleRegionHandles(min, max);

            //foreach (var handle in handles)
            //{
            //    if (! mapCache.mapTileIsPresent(handle, 1))
            //    {
            //        //not avail in cache so fetch it.
            //        var res = mapFetcher.GetRegionTileExternal(handle, 1);
            //        Debug.Log("fetching texture at " + handle);
            //        mapCache
            //    }

            //    return ;
            //}



        }
    }
}
