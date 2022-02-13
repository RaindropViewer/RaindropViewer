using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Better.StreamingAssets;
using System;
using Raindrop;
using UnityEngine.UI;
using Raindrop.Presenters;
using OpenMetaverse;
using Raindrop.Map;
using Raindrop.Services.Bootstrap;
using Raindrop.UI;
using Raindrop.UI.Presenters;
using UnityEngine.Serialization;

namespace Raindrop.UI.Views
{
    // UI for the map viewing.
    // attach to the UI of map viewer.
    public class MapUIView : MonoBehaviour
    {
        // map manager. keeps track of mapsGOs. creates new mapGOs from prefabs. culls those that are no longer visible. fetches those that need to be viewed.
        [SerializeField]
        public MapScenePresenter mapSceneGraph_Root;

        private MapSceneController msp;

        private void Awake()
        {
            msp = new MapSceneController(this, mapSceneGraph_Root);
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

        public MapSceneController getPresenter()
        {
            return this.msp;
        }
    }
}