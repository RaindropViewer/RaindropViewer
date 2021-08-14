using OpenMetaverse;
using Raindrop.Network;
using Raindrop.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UniRx;
using UnityEngine;
using static Raindrop.MapDataPool;

namespace Raindrop
{
    public class MapLogic
    {
        //range logic
        internal static OpenMetaverse.Vector2 getMinVec2(OpenMetaverse.Vector2 range, OpenMetaverse.Vector2 focalPoint)
        {
            return focalPoint - new OpenMetaverse.Vector2(range.X / 2 , range.Y / 2);
        }
        
        internal static OpenMetaverse.Vector2 getMaxVec2(OpenMetaverse.Vector2 range, OpenMetaverse.Vector2 focalPoint)
        {
            return focalPoint + new OpenMetaverse.Vector2(range.X / 2 , range.Y / 2);
        }

        //backend image fetch logic


        //backend, map fetching class
        public class MapFetcher
        {
            private MapDataPool mapData;
            List<ulong> tileRequests = new List<ulong>(); // a list of pending fetch requests. the ulongs are the x and y world coordinates (gridX * 256), packed.
            uint regionSize = 256;
            private ParallelDownloader downloader;
            int poolSize = 10;

            public MapFetcher()
            {
                mapData = new MapDataPool(poolSize);
                downloader = new ParallelDownloader();
            }

            public MapTile GetMapTile(ulong handle, int zoom)
            {
                if (mapData.regionTiles.ContainsKey(handle))
                {
                    return mapData.regionTiles[handle];
                }

                return null;
            }


            //get region tile using SL map API -- JPEG images.
            public MapTile GetRegionTileExternal(ulong handle, int zoom)
            {
                if ( (zoom > 4) || (zoom < 1) )
                {
                    return null;
                }


                if (mapData.regionTiles.ContainsKey(handle))
                {
                    return mapData.regionTiles[handle];
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
                                    //using (MemoryStream s = new MemoryStream(responseData)) //s of JPEG
                                    //{

                                    lock (mapData.regionTiles)
                                    {
                                        //decode http response data into texture2d
                                        MapTile tex = mapData.getUnusedTex();

                                        //run jpeg decoding on the main thread, for now.
                                        MainThreadDispatcher.StartCoroutine(TestAsync(tex, responseData));

                    
                                        //Image.FromStream(s);
                                        //mapData.regionTiles[handle] = 
                                        //needRepaint = true;
                                        
                                        mapData.regionTiles.Add( handle,tex);
                                    }

                                    //}
                                    
                                }
                                catch (Exception e)
                                {
                                    Debug.LogError(e.Message);
                                }
                            }
                        }
                    );


                    lock (mapData.regionTiles)
                    {
                        mapData.regionTiles[handle] = null;
                    }

                    return null;
                }
            }

            private IEnumerator TestAsync(MapTile tex, byte[] responseData)
            {
                try
                {
                    bool success = tex.texture.LoadImage(responseData);

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