using Raindrop.UI.Views;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Raindrop.UI.Views
{
    public class SliderZoomable : MonoBehaviour
    {
        private Slider slider;
        public GameObject mapUIGO;
        private MapUI mapUI;

        // Start is called before the first frame update
        void Start()
        {

        }

        void Awake()
        {
            mapUI = mapUIGO.GetComponent<MapUI>();
            slider = gameObject.GetComponent<Slider>();
            if (slider != null)
            {
                slider.onValueChanged.AddListener(ListenerMethod);
            }

        }
        public void ListenerMethod(float value)
        {
            if (mapUI != null)
            {
                mapUI.setZoom(value);
            }
        }


    }
}