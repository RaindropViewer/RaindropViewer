using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using OpenMetaverse;
using Plugins.CommonDependencies;
using UnityEngine;

namespace Raindrop.Map.Model
{
    // for you to get map tiles from the network.
    public class MapTilesNetwork 
    {
        private RaindropInstance instance => ServiceLocator.Instance.Get<RaindropInstance>();

        List<ulong> tileRequests = new List<ulong>(); // a list of pending fetch requests. the ulongs are the x and y world coordinates (gridX * 256), packed.
        private DownloadManager downloader;
        UnityMainThreadDispatcher mainThreadInstance;

        public MapTilesNetwork(int parallelDownloads)
        {
            downloader = new DownloadManager();
            downloader.ParallelDownloads = parallelDownloads;
            mainThreadInstance = UnityMainThreadDispatcher.Instance();
        }

        //get region tile using SL map API -- JPEG images.
        // obtains the image from URL, decodes it in main thread, the stores data in the appropriate maptile.
        // handle : Global (huge) coordinates
        public MapTile GetRegionTileExternal_SL(ulong handle, int zoom)
        {
            zoom = 1;

            // if (tryGetMapTile(handle, 1) != null)
            // {
            //     //since tile is ready in the memory, return it.
            //     return tryGetMapTile(handle, 1);
            // }
            // else
            // {
                lock (tileRequests)
                {
                    if (tileRequests.Contains(handle)) return null;
                    tileRequests.Add(handle);
                }

                uint regX, regY;
                OpenMetaverse.Utils.LongToUInts(handle, out regX, out regY);
                regX /= MapService.regionSize;
                regY /= MapService.regionSize;
                //int zoom = 1;
                MapTile res = new MapTile(256,256); //todo, use the objectpool pattern here.
                var req = new DownloadRequest(
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
                                //1. download is ready now.
                                //3. add it to the hashtable in the model (memory) 
                                //2. decode the image on the main thread.
                                //4. exit.

                                Debug.Log("fetching the texture was successful!");

                                // removed. reason: initially we wanted to push into a queue.
                                // now realise want to simplify. just decode on main thread,
                                // let the view keep polling the maptile object for ready flag.
                                // lock (mapDataQueue_lock)
                                // {
                                //     receivedDataQueue.Enqueue(new MapService.MapData(handle, responseData));
                                // }

                                //start to run jpeg decoding on the main thread.
                                mainThreadInstance.Enqueue(DecodeDataToTexAsync(res, responseData));

                                // remove from request list
                                lock (tileRequests)
                                {
                                    if (tileRequests.Contains(handle))
                                        tileRequests.Remove(handle);
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.LogError(e.Message);
                            }
                        }
                    }
                );
                req.Retries = 1;
                downloader.QueueDownload( req);

                return res; // the tile is created. but inside the tile, it is not ready and the texture is the empty texture static

                //lock (mapData.sceneTiles)
                //{
                //    mapData.sceneTiles[handle] = mapData.acquireTile(); //wtf!
                //}

            // }
        }
        
        // decode jpg bytes to texture2d.
        //must be run on the main thread as we are manipulating T2D
        // 1. decode JPEGbytes to texture2d
        // 2. replace the corresponding tiles's T2D
        private IEnumerator DecodeDataToTexAsync(MapTile tile, byte[] responseData)
        {
            try
            {
                lock (tile) //lock the tile that you want to write into.
                {
                    Texture2D tex = tile.getTex();
                    bool success = tex.LoadImage(responseData); //warn: main thread only. fixme.
                    tile.isReady = true;    
                }
                
            }
            catch (Exception w)
            {
                Debug.LogWarning("decode the texture failed : " + w.Message);
            }
            Debug.Log("completed mainthread async texture Decode.");

            yield return null;
        }

        //retrieve and decode a single tile map from the server.
        public MapTile GetRegionTilesInternal(ulong handle)
        {
            // sync with/ add to the existing requests.
            lock (tileRequests)
            {
                if (tileRequests.Contains(handle)) return null;
                tileRequests.Add(handle);
            }
            
            // instance.Client.Grid.RequestMapBlocks(
            //     GridLayerType.Objects,
            //     (ushort)regLeft,
            //     (ushort)regBottom,
            //     (ushort)regXMax,
            //     (ushort)regYMax,
            //     true);


            MapTile res = null;
            return res; // the tile is created. but inside the tile, it is not ready and the texture is the empty texture static

            
        }
    }

}