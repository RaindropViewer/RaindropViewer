using OpenMetaverse;
using Raindrop.Network;
using Raindrop.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UniRx;
using UnityEngine;

namespace Raindrop.Map
{
    /// <summary>
    /// Contains logic to fetch and decode the map textures.
    /// </summary>

    public class MapLogic
    {
        #region range logic

        //    ---------@  max
        //    |        |
        //    |        |
        //    |        |
        //    |        |
        //min @---------

        // range - bottom left corner 
        internal static OpenMetaverse.Vector2 getMinVec2(OpenMetaverse.Vector2 range, OpenMetaverse.Vector2 focalPoint)
        {
            return focalPoint - new OpenMetaverse.Vector2(range.X / 2 , range.Y / 2);
        }
        
        // range - top right corner

        internal static OpenMetaverse.Vector2 getMaxVec2(OpenMetaverse.Vector2 range, OpenMetaverse.Vector2 focalPoint)
        {
            return focalPoint + new OpenMetaverse.Vector2(range.X / 2 , range.Y / 2);
        }

        #endregion

        //backend image fetch logic


        // map fetching class.
        // contains a dict for tracking the tile maps.
        // contains the reusable pool to discard and recycle tile maps.
        public class MapFetcher
        {
            MapDataManager mapDataMgr;
            List<ulong> tileRequests = new List<ulong>(); // a list of pending fetch requests. the ulongs are the x and y world coordinates (gridX * 256), packed.
            uint regionSize = 256;
            private ParallelDownloader downloader;
            int poolSize = 10;

            UnityMainThreadDispatcher mainThreadInstance;

            public MapFetcher()
            {
                mapDataMgr = new MapDataManager(10);

                downloader = new ParallelDownloader();

                mainThreadInstance = UnityMainThreadDispatcher.Instance();
            }

            /// <summary>
            /// API to Get the map tile at specific region handle and zoom level. Only zoom level 1 is supported.
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="zoom"></param>
            /// <returns></returns>
            public MapTile GetMapTile(ulong handle, int zoom)
            {
                MapTile res = null;

                //try
                //{
                res = mapDataMgr.tryGetTile(handle);
                //}
                //catch (Exception)
                //{
                uint x, y;
                Utils.LongToUInts(handle, out x, out y);
                Debug.LogError("no tile found at " + x + " " + y);
                //}
                return res;
            }


            //get region tile using SL map API -- JPEG images.
            // obtains the image from URL, decodes it in main thread, the stores data in the appropriate maptile.
            public MapTile GetRegionTileExternal(ulong handle, int zoom)
            {
                if ( (zoom > 4) || (zoom < 1) )
                {
                    return null;
                }


                if (GetMapTile(handle, 1) != null)
                {
                    return GetMapTile(handle, 1);
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
                                        Debug.Log("1");
                                        //decode http response data into texture2d
                                        //MapTile tex = mapData.getTile();
                                        MapTile tex = mapDataMgr.setEmptyTile(handle); //Tile is a empty tile right now -- we write to it soon.

                                        Debug.Log("2");
                                        //run jpeg decoding on the main thread, for now.
                                        mainThreadInstance.Enqueue(DecodeDataToTexAsync(tex, responseData));

                                        Debug.Log("3");
                                        //set the tile into tile manager
                                        mapDataMgr.setTile( handle,tex);
                                        Debug.Log("4");
                                    //}

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
                {
                    Texture2D _tex = tex.getTex();
                    bool success = _tex.LoadImage(responseData);

                }
                catch (Exception w)
                {
                    Debug.Log("decode the texture failed : " + w.Message);
                }
                Debug.Log("completed mainthread async texture Decode.");

                yield return null;

            }


            // get image from cache or asset manager. returns tex2D.
            // should only be used for the minimap. Use http map API fetch for normal map.
            //Texture2D DownloadRegionTile(ulong handle, UUID imageID, GridClient Client)
            //{
            //    //if (mapData.regionTiles.ContainsKey(handle)) return;

            //    //lock (tileRequests)
            //    //    if (!tileRequests.Contains(handle))
            //    //        tileRequests.Add(handle);


            //    Uri url = Client.Network.CurrentSim.Caps.GetTextureCapURI();
            //    if (url != null)
            //    {
            //        if (Client.Assets.Cache.HasAsset(imageID))
            //        {
            //            Texture2D img;
            //            OpenMetaverse.Imaging.ManagedImage mi;
            //            if (OpenMetaverse.Imaging.OpenJPEG.DecodeToImage(Client.Assets.Cache.GetCachedAssetBytes(imageID), out mi, out img))
            //            {
            //                return img;
            //                //mapData.regionTiles[handle] = img;
            //            }
            //            lock (tileRequests)
            //                if (tileRequests.Contains(handle))
            //                    tileRequests.Remove(handle);
            //        }
            //        else
            //        {
            //            downloader.QueueDownlad(
            //                new Uri($"{url}/?texture_id={imageID}"),
            //                30 * 1000,
            //                "image/x-j2c",
            //                null,
            //                (request, response, responseData, error) =>
            //                {
            //                    if (error == null && responseData != null)
            //                    {
            //                        Image img;
            //                        OpenMetaverse.Imaging.ManagedImage mi;
            //                        if (OpenMetaverse.Imaging.OpenJPEG.DecodeToImage(responseData, out mi, out img))
            //                        {
            //                            mapData.regionTiles[handle] = img;
            //                            Client.Assets.Cache.SaveAssetToCache(imageID, responseData);
            //                        }
            //                    }

            //                    lock (tileRequests)
            //                    {
            //                        if (tileRequests.Contains(handle))
            //                        {
            //                            tileRequests.Remove(handle);
            //                        }
            //                    }
            //                });
            //        }
            //    }
            //    else
            //    {
            //        Client.Assets.RequestImage(imageID, (state, assetTexture) =>
            //        {
            //            switch (state)
            //            {
            //                case TextureRequestState.Pending:
            //                case TextureRequestState.Progress:
            //                case TextureRequestState.Started:
            //                    return;

            //                case TextureRequestState.Finished:
            //                    if (assetTexture?.AssetData != null)
            //                    {
            //                        Image img;
            //                        OpenMetaverse.Imaging.ManagedImage mi;
            //                        if (OpenMetaverse.Imaging.OpenJPEG.DecodeToImage(assetTexture.AssetData, out mi, out img))
            //                        {
            //                            mapData.regionTiles[handle] = img;
            //                        }
            //                    }
            //                    lock (tileRequests)
            //                        if (tileRequests.Contains(handle))
            //                            tileRequests.Remove(handle);
            //                    break;

            //                default:
            //                    lock (tileRequests)
            //                        if (tileRequests.Contains(handle))
            //                            tileRequests.Remove(handle);
            //                    break;
            //            }
            //        });
            //    }
            //}
        }
    }
}