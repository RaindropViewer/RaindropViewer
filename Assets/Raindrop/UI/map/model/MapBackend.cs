using OpenMetaverse;
using Raindrop.Network;
using Raindrop.UI;
using ServiceLocator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UniRx;
using UnityEngine;

namespace Raindrop.Map.Model
{
    /// <summary>
    /// Contains logic to fetch and decode the map textures.
    /// </summary>

    //backend image fetch logic



    // map fetching class.
    // contains a dict for tracking the tile maps.
    // contains the reusable pool to discard and recycle tile maps.
    public class MapBackend : IGameService
    {
        MapTilesManager mapTilesManager;
        List<ulong> tileRequests = new List<ulong>(); // a list of pending fetch requests. the ulongs are the x and y world coordinates (gridX * 256), packed.
        uint regionSize = 256;
        private ParallelDownloader downloader;
        int poolSize = 10;

        UnityMainThreadDispatcher mainThreadInstance;

        public MapBackend()
        {
            mapTilesManager = new MapTilesManager(10);

            downloader = new ParallelDownloader();

            mainThreadInstance = UnityMainThreadDispatcher.Instance();
        }

        /// <summary>
        /// API to Get the map tile at specific region handle and zoom level. Only zoom level 1 is supported.
        /// gets the map tile at handle if present.
        /// if not present, a webrequest for the tile will be made. a notification of download success will not be reported to the caller of this function.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="zoom"></param>
        /// <returns></returns>
        public MapTile tryGetMapTile(ulong handle, int zoom)
        {
            MapTile res = null;

            //try
            //{
            res = mapTilesManager.tryGetTile(handle);
            //}
            //catch (Exception)
            //{
            //uint x, y;
            //Utils.LongToUInts(handle, out x, out y);
            //Debug.LogError("no tile found at " + x + " " + y);
            //}
            return res;
        }


        //get region tile using SL map API -- JPEG images.
        // obtains the image from URL, decodes it in main thread, the stores data in the appropriate maptile.
        public MapTile GetRegionTileExternal(ulong handle, int zoom)
        {
            if ((zoom > 4) || (zoom < 1))
            {
                return null;
            }


            if (tryGetMapTile(handle, 1) != null)
            {
                return tryGetMapTile(handle, 1);
            }
            else
            {
                lock (tileRequests)
                {
                    if (tileRequests.Contains(handle)) return null;
                    tileRequests.Add(handle);
                }

                uint regX, regY;
                Utils.LongToUInts(handle, out regX, out regY);
                regX /= regionSize;
                regY /= regionSize;
                //int zoom = 1;

                downloader.QueueDownlad(
                    new Uri(string.Format("http://map.secondlife.com/map-{0}-{1}-{2}-objects.jpg", zoom, regX, regY)),
                    20 * 1000,
                    null,
                    null,
                    (request, response, responseData, error) =>
                    {
                        if (error == null && responseData != null)
                        {
                            try
                            {
                                Debug.Log("fetching the texture was successful!");

                                //lock (mapData.sceneTiles)
                                //{
                                //decode http response data into texture2d
                                //MapTile tex = mapData.getTile();
                                MapTile tile = mapTilesManager.setEmptyTile(handle); //Tile is a empty tile right now -- we write to it soon.

                                //run jpeg decoding on the main thread, for now.
                                mainThreadInstance.Enqueue(DecodeDataToTexAsync(tile, responseData));

                                //set the tile into tile manager
                                //mapTilesManager.setTile(handle, tile);
                                //}

                                lock (tileRequests)
                                    if (tileRequests.Contains(handle))
                                        tileRequests.Remove(handle);

                            }
                            catch (Exception e)
                            {
                                Debug.LogError(e.Message);
                            }
                        }
                    }
                );


                //lock (mapData.sceneTiles)
                //{
                //    mapData.sceneTiles[handle] = mapData.acquireTile(); //wtf!
                //}

                return null;
            }
        }

        private IEnumerator DecodeDataToTexAsync(MapTile tex, byte[] responseData)
        {
            try
            { //check for failed downlaod.
                Texture2D _tex = tex.getTex();
                bool success = _tex.LoadImage(responseData);

            }
            catch (Exception w)
            {
                Debug.LogWarning("decode the texture failed : " + w.Message);
            }
            Debug.Log("completed mainthread async texture Decode.");

            yield return null;

        }


    }
}
