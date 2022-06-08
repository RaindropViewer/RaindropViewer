using OpenMetaverse;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Plugins.CommonDependencies;
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

        private MapTilesRAM _mapTilesRam;
        private MapTilesNetwork mapTilesNetwork;
        
        private RaindropInstance instance => ServiceLocator.Instance.Get<RaindropInstance>();
        // private bool Secondlife; //=> instance.Netcom.LoginOptions.Grid.Platform == "SecondLife";
        public bool isReady = false;

        public MapService()
        {
            //the cache.
            _mapTilesRam = new MapTilesRAM(10, receivedDataQueue);
            // the disk.
            // mapTilesDisk = new MapTilesDisk();
            // the network.
            mapTilesNetwork = new MapTilesNetwork(2);
            
            instance.Netcom.ClientConnected += NetcomOnClientConnected;
        }

        // set my SL flag as true.
        private void NetcomOnClientConnected(object sender, EventArgs e)
        {
            // Secondlife = (instance.Netcom.LoginOptions.Grid.Platform == "SecondLife");
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
            if (instance.Netcom.IsSecondlife)
            {
                res = mapTilesNetwork.GetRegionTileExternal_SL(handle, 1);
            }
            //todo: handle non-SL case.
            
            return res;
        }
        

    }
}
