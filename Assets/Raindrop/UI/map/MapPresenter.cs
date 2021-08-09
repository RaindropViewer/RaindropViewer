using OpenMetaverse;
using OpenMetaverse.Assets;
using OpenMetaverse.Imaging;
using Raindrop.Netcom;
using Raindrop.UI;
using System;
using System.Collections.Generic;
using UniRx;
using Unity;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Raindrop.Presenters
{
    // map texture module - root gameobject.

    // this manages the 2d texture objects in the map texture layer
    // min = bottom left (OM)
    // max = top right (OM)
    public class MapPresenter : MonoBehaviour
    {
        //camera. contains the viewable range.
        [SerializeField]
        public GameObject cameraPresenterGO;
        private StationaryDownwardOrthoCameraPresenter cameraPresenter;

        //map mover. Contains focal point. Moves focal point in response to screen swipes.
        [SerializeField]
        public GameObject mapMoverGO;
        private MapMover mapMover;
        //public OpenMetaverse.Vector2 focalPoint;
        //public Transform mapOrigin; // 1000,1000

        // map manager. keeps track of mapsGOs. creates new mapGOs from prefabs. culls those that are no longer visible. fetches those that need to be viewed.
        [SerializeField]
        public GameObject mapManagerGO;
        private MapPoolPresenter mapManager;

        // a reference to the avatar in the grid.
        public GameObject Avatar;

        private RaindropInstance instance { get { return ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>(); } }
        private RaindropNetcom netcom { get { return instance.Netcom; } }
        private GridClient client { get { return instance.Client; } }



        public void Awake()
        {

            if (cameraPresenterGO == null)
            {
                throw new Exception("cameraPresenterGO not assigned.");
            }
            cameraPresenter = cameraPresenterGO.GetComponent<StationaryDownwardOrthoCameraPresenter>();

            if (mapMoverGO == null)
            {
                throw new Exception("MapMoverGO not assigned.");
            }
            mapMover = mapMover.GetComponent<MapMover>();

            if (mapManagerGO == null)
            {
                throw new Exception("mapManagerGO not assigned.");
            }
            mapManager = mapManager.GetComponent<MapPoolPresenter>(); 


            //instance.Client.Network.SimChanged += Network_OnCurrentSimChanged;
        }


        private void Update()
        {
            UpdateMapViewing();
        }

        //update what is viewable and what is not.
        private void UpdateMapViewing()
        {
            var range = cameraPresenter.getRange(); // we can see this ranges now.
            ulong focalPoint = mapMover.GetLookAt();
            uint x;
            uint y;
            //var min = MapLogic.getMinVec2(range, focalPoint);
            //var max = MapLogic.getMaxVec2(range, focalPoint);
            Utils.LongToUInts(focalPoint, out x, out y);
            mapManager.setViewableRange(x, y, range);

        }

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