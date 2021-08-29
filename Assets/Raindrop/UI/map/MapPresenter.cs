using OpenMetaverse;
using OpenMetaverse.Assets;
using OpenMetaverse.Imaging;
using Raindrop.Map.Model;
using Raindrop.Netcom;
using Raindrop.UI;
using Raindrop.UI.Views;
using System;
using System.Collections.Generic;
using UniRx;
using Unity;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Raindrop.UI.Presenters
{
    public class MapPresenter
    {
        private MapViewer mapViewer;
        private MapSceneView mapSceneView;
        private MapBackend mapFetcher;
        private bool needRepaint;

        private RaindropInstance instance { get { return ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>(); } }
        private RaindropNetcom netcom { get { return instance.Netcom; } }
        private GridClient client { get { return instance.Client; } }
        bool Active => instance.Client.Network.Connected;

        /// <summary>
        /// ctor -- takes in a UI view and a gameobject view
        /// </summary>
        /// <param name="mapViewer"></param>
        public MapPresenter(MapViewer mapViewer, MapSceneView mapSceneView)
        {
            this.mapViewer = mapViewer;
            this.mapSceneView = mapSceneView;

            //mapFetcher = ServiceLocator.ServiceLocator.Instance.Get<MapBackend>();
            mapFetcher = new MapBackend();
        }


        /// <summary>
        /// get a list of region handles that are visible to the camera.
        /// </summary>
        /// <returns></returns>
        public List<ulong> calcVisibleRegionHandles()
        {
            // camera pos
            DownwardOrthoCameraView camView = mapSceneView.getCameraView();
            // camera bounds
            Vector2 min = Vector2.Max(camView.getMin(), Vector2.zero);
            Vector2 max = Vector2.Max(camView.getMax(), Vector2.zero);
            // for loop from bounds to bounds.
            List<ulong> visiblehandles = new List<ulong>();
            int vert_min = (int) min.y;
            int vert_max = (int) max.y;
            int horz_min = (int) min.x;
            int horz_max = (int) max.x;
            for (int i = horz_min; i<horz_max; i++)
            {
                for (int j = vert_min; j < vert_max ; j++)
                {
                    ulong region = Utils.UIntsToLong((uint)(i * 256), (uint)(j * 256));
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
        /// Retrieves desired tile from backend. if not present, tiles will be fetched and come in at a later time.
        /// </summary>
        public void onRefresh()
        {
            var handles = calcVisibleRegionHandles();

            foreach(var handle in handles)
            {
                var tile = mapFetcher.tryGetMapTile(handle, 1);

                if (tile == null) 
                {
                    //not avail in backend so fetch it.
                    mapFetcher.GetRegionTileExternal(handle, 1);
                    Debug.Log("fetching texture at " + handle);
                } else
                {
                    //if (mapSceneView.isPresent(handle))
                    //{

                    //}
                    //else
                    //{
                        mapSceneView.createMapTileAt(handle, tile);
                        Debug.Log("making tile at " + handle);

                    //}
                }


            }

            Debug.Log("refreshed internal images. fetching images if any..");
            return;
        }


        /// <summary>
        /// Redraws the tiles if the redraw flag is true.
        /// </summary>
        //private void redrawMap()
        //{
        //    //onRefresh(); //hacky

        //    if (needRepaint)
        //    {
        //        needRepaint = false;
        //    }
        //    else
        //    {
        //        return;
        //    }

        //    ulong handle = mapLookAt.GetLookAt();
        //    MapTile tex = mapFetcher.tryGetMapTile(handle, 1);

        //    if (tex != null)
        //    {
        //        mapSceneView.setRawImage(tex.getTex());
        //    }
        //    Debug.Log("drew images.");
        //}

        //private void Network_OnCurrentSimChanged(object sender, SimChangedEventArgs e)
        //{

        //    if (client.Network.Connected) return;

        //    Debug.Log("Network_OnCurrentSimChanged");

        //    GridRegion region;
        //    if (client.Grid.GetGridRegion(client.Network.CurrentSim.Name, GridLayerType.Objects, out region))
        //    {

        //        Texture2D _new_MapLayer = null; // LOL! its unity texture btw.

        //        UUID _MapImageID = region.MapImageID;
        //        Vector2Int regionPos = new Vector2Int(region.X,region.Y);
        //        ManagedImage nullImage;

        //        Debug.Log("requesting map image.");

        //        client.Assets.RequestImage(_MapImageID, ImageType.Baked,
        //            delegate (TextureRequestState state, AssetTexture asset)
        //            {
        //                if (state == TextureRequestState.Finished)
        //                {
        //                    Debug.Log("minimap texture fetched, decoding it!");
        //                    OpenJPEG.DecodeToImage(asset.AssetData, out nullImage, out _new_MapLayer); //this call interally calls to another function 'DecodeToImage(byte[] encoded, out ManagedImage managedImage)'

        //                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
        //                    {
        //                        SetMapLayer(_new_MapLayer, regionPos);
        //                    });
        //                }
        //                else
        //                    Debug.LogWarning("minimap failed to DL texture.");
        //            });
        //    }

        //}




    }
}