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


// lets you view the maps by choosing parameters.
// use external API. does not need login.

namespace Raindrop.UI.Views
{
    public class MapViewer : MonoBehaviour
    {

        // map manager. keeps track of mapsGOs. creates new mapGOs from prefabs. culls those that are no longer visible. fetches those that need to be viewed.
        [SerializeField]
        public GameObject mapSceneViewGO;
        private MapSceneView mapSceneView;


        [SerializeField]
        public GameObject zoomSliderGO;
        private Slider zoomSlider;

        
        private MapPresenter presenter;


        private void Awake()
        {
            mapSceneView = mapSceneViewGO.GetComponent<MapSceneView>();

            //get reference to the view.
            zoomSlider = zoomSliderGO.GetComponent<Slider>();
            if (zoomSlider == null)
            {
                Debug.LogWarning("slider is fucked"); // fix exception type plz
            }

        }

        private void Update()
        {
            //redrawMap();
        }


        private void Start()
        {
            presenter = new MapPresenter(this, mapSceneView);
            InvokeRepeating("redrawMap", 5f, 5f);
        }


        private void redrawMap()
        {
            presenter.onRefresh(); //retrieve tiles and redraw.
        }


    }
}