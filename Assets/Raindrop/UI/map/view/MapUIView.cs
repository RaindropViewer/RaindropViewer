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
        public MapSceneView mapSceneGraph_Root;

        private MapScenePresenter msp;
            
        //reset the view to daboom.
        [SerializeField]
        public Button button;


        private void Awake()
        {
            button.onClick.AddListener(OnClick_ResetView);

            msp = new MapScenePresenter(this, mapSceneGraph_Root);
        }

        private void OnClick_ResetView()
        {
            mapSceneGraph_Root.resetView();
        }


        public void setZoom(float value)
        {
            mapSceneGraph_Root.setZoom(value);
        }

        public MapScenePresenter getPresenter()
        {
            return this.msp;
        }
    }
}