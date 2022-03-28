using OpenMetaverse;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Raindrop.ServiceLocator;
using UnityEngine;

namespace Raindrop.Map.Model
{
    /// <summary>
    /// Provides the map textures on demand.
    ///
    /// previously...:
    /// Contains logic to fetch and decode the map textures.
    /// </summary>
    public class MapService : IGameService
    {
        public static uint regionSize = 256;

        // fixme: unused
        public class MapData
        {
            ulong gridLoc;
            byte[] dataJPG;

            public MapData(ulong gridLoc, byte[] dataJPG)
            {
                this.gridLoc = gridLoc;
                this.dataJPG = dataJPG;
            }   
        }

        
        //a queue of map data that want to be decoded.
        // fixme: unused
        ConcurrentQueue<MapData> receivedDataQueue = new ConcurrentQueue<MapData>();

        // fixme: unused
        UnityMainThreadDispatcher mainThreadInstance;
        private object mapDataQueue_lock = new object();
        
        private MapTilesRAM _mapTilesRam;
        private MapTilesNetwork mapTilesNetwork;
        
        private RaindropInstance instance => ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>();
        private bool Secondlife => instance.Netcom.LoginOptions.Grid.Platform == "SecondLife";

        public MapService()
        {
            mainThreadInstance = UnityMainThreadDispatcher.Instance();

            //the cache.
            _mapTilesRam = new MapTilesRAM(10, receivedDataQueue);
            // the disk.
            // mapTilesDisk = new MapTilesDisk();
            // the network.
            mapTilesNetwork = new MapTilesNetwork(2);
        }

        /// <summary>
        /// API to Get the map tile at specific region handle and zoom level.
        /// - you will need to poll the ready flag in the Maptile until it is ready.
        /// Caveats:
        /// - Only zoom level 1 is supported.
        /// - if not present, a webrequest for the tile will be made.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="zoom"></param>
        /// <param name="isReady">if the returned MapTile is ready for display.</param>
        /// <returns></returns>
        public MapTile GetMapTile(ulong handle, int zoom)
        {
            MapTile res = null;
            bool isTileAvailable = _mapTilesRam.tryGetTile_RAM(handle, out res);
            if (isTileAvailable)
            {
                return res;
            }
            //this call to network, returns a maptile that is empty inside for now.
            if (Secondlife)
            {
                res = mapTilesNetwork.GetRegionTileExternal_SL(handle, 1);
            }
            return res;
        }
        

    }
}
