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

namespace Raindrop.UI.Views
{
    // UI for the map viewing.
    public class MapUI : MonoBehaviour
    {
        private MapScenePresenter msp => MapScenePresenter.getInstance();
        
        // map manager. keeps track of mapsGOs. creates new mapGOs from prefabs. culls those that are no longer visible. fetches those that need to be viewed.
        [SerializeField]
        public GameObject mapScene_GameObject;
        private MapSceneView mapSceneRenderer;

        //reset the view to daboom.
        [SerializeField]
        public GameObject resetButtonGO;
        private Button button;
        
        private void Awake()
        {
            mapSceneRenderer = mapScene_GameObject.GetComponent<MapSceneView>();

            button = resetButtonGO.GetComponent<Button>();
            button.onClick.AddListener(TaskOnClick);
        }

        private void TaskOnClick()
        {
            mapSceneRenderer.resetView();
        }


        public void setZoom(float value)
        {
            mapSceneRenderer.setZoom(value);

        }

        public MapScenePresenter getPresenter()
        {
            return this.msp;
        }
    }
}