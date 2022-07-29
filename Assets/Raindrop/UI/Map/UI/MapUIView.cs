using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using OpenMetaverse;
using Plugins.CommonDependencies;
using Raindrop.Camera;
using UnityEngine;

using Raindrop.UI.map.Map_SceneHierachy;
using UnityEngine.Serialization;
using Vector3 = UnityEngine.Vector3;

namespace Raindrop.UI.Views
{
    // UI for the map viewing.
    // attach to the UI of map viewer.
    public class MapUIView : MonoBehaviour
    {
        // map manager. keeps track of mapsGOs. creates new mapGOs from prefabs. culls those that are no longer visible. fetches those that need to be viewed.
        [FormerlySerializedAs("mapSceneGraph_Root")] [SerializeField]
        // public MapScenePresenter mapScenePresenter;

        public ParcelInfoPopup parcelUI;

        public MapScenePresenter MapScenePresenter;

        private GridClient Client => RaindropInstance.GlobalInstance.Client;
       
        //global handle: handle + the offset within the simulator.
        public void OnFocusMapPosition(ulong global_handle, RaycastHit raycastHit)
        {
            //1. lerp to map postion
            MapScenePresenter.mapCameraView.lerpCamTo(raycastHit.point, 1);
            
            //2. get parcel data and show the info as a popup window
            parcelUI.Open(global_handle);
        }
        

        private void Awake()
        {
            // msp = new MapSceneController(this);
        }

        private void OnEnable()
        {
            CamerasManager.Instance.ActivateCamera(CameraIdentifier.CameraType.Minimap);
            //mapSceneGraph_Root.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            CamerasManager.Instance.ActivateCamera(CameraIdentifier.CameraType.Main);
            //mapSceneGraph_Root.gameObject.SetActive(false);
        }

    }
}