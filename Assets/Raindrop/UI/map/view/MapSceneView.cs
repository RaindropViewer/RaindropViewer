using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using Raindrop.Map;
using OpenMetaverse;
using UE = UnityEngine;
using Raindrop.Map.Model;
using Raindrop.Presenters;
using Vector2 = UnityEngine.Vector2;
using System.Collections;
using Raindrop.UI.Presenters;
using Raindrop.Utilities;

namespace Raindrop.UI.Views
{
    /// <summary>
    /// View. this is map tiles in 3d space. i use presenter.
    /// </summary>

    public class MapSceneView : MonoBehaviour
    {
        [SerializeField]
        private const float RenderUpdatePeriod = 2f;
        [SerializeField]
        public bool fetchOn = false;

        //the parent to spawn the map tiles under.
        public GameObject mapRoot;

        [SerializeField]
        public GameObject mapTilePrefab;

        //camera. contains the viewable range.
        [SerializeField]
        public DownwardOrthoCameraView cameraView;
        private MapSceneController _mapPoolController;
        private Dictionary<ulong, GameObject> map_collection = new Dictionary<ulong, GameObject>(); //tiles that are in the scene.
        //private Dictionary<UUID, MapEntity> agent_collection = new Dictionary<UUID, MapEntity>(); //agents that are in the scene.

        internal void resetView()
        {
            cameraView.setToGridPos(new Vector2(1000,1000));
        }


        //viewable area of the current map_collection.
        // public int max_X, max_Y;
        // public int min_X, min_Y;

        /// <summary>
        ///zoom level of map. 
        /// 1 : height of camera is 1 //see about 1 map only
        /// 2 : height of camera is 2 
        /// 10 : height of camera is 10 //see alot
        /// </summary>
        public float zoomLevel;


        private void Awake()
        {
            
            
        }

        private void Start()
        {

            initModule();

            if (fetchOn)
            {
                //'redraw' map every 5 secs.
                InvokeRepeating("RedrawMap", RenderUpdatePeriod, RenderUpdatePeriod);
            }
        }



        public void initModule()
        {
            resetCamera_DABOOM();
        }

        private void resetCamera_DABOOM()
        {
            // max_X = max_Y = max_X = max_Y = 1000;
            zoomLevel = 1;
        }

        internal void setZoom(float value)
        {
            if (value >= 0.1f)
            {
                zoomLevel = value;
            }
        }

        internal DownwardOrthoCameraView getCameraView()
        {
            return cameraView;
        }

        private void clearFrontendTiles()
        {
            map_collection.Clear();

        }
        
        internal bool isPresentInFrontend(ulong handle)
        {
            //var pos = handleToVector2(handle);
            return map_collection.ContainsKey(handle);

        }

        public void lerpCamTo()
        {
            StartCoroutine("lerpTo");

        }

        IEnumerator lerpTo(Transform transform, Vector2 targetPosition, float duration)
        {
            float time = 0;
            Vector2 startPosition = transform.position;

            while (time < duration)
            {
                transform.position = Vector2.Lerp(startPosition, targetPosition, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            transform.position = targetPosition;
        }


        //private void Update()
        //{
        //    fetchMapsIfRequired();
        //    redrawMap();
        //}

        //private void fetchMapsIfRequired()
        //{
        //    if (visibleMapTiles.requireFetching())
        //    {
        //        visibleMapTiles.fetch(list regionAreas);
        //    }
        //}

        private void RedrawMap()
        {
            //only redraw if the root of mapGraph is activated.
            if (! this.isActiveAndEnabled)
            {
                return;
            }
            

            //1. get the camera's location
            // var x = cameraView.centerX;
            // var y = cameraView.centerY;

            //2. obtain the range of its view in grid coordinates.
            var handles =
                MapSceneController.getVisibleRegionHandles(
                    cameraView.min,
                    cameraView.max + Vector2.one); // add 1 int to the result, as each sim is 1-large.

            
            //3. spawn the tiles that are in this space.
            foreach (var handle in handles)
            {
                if (! map_collection.ContainsKey(handle))
                {
                    //not avail in scene, so instantiate the tile.
                    var tileGO = SpawnOnMapPlane(handle);
                    map_collection.Add(handle,tileGO);
                }
            }

            // bool needRedraw = true;
            //
            // if (needRedraw)
            // {
            //     _mapPoolController.RedrawMap();
            // }
        }
        
        //in the crazy large global handle coordinates
        public GameObject SpawnOnMapPlane(ulong handle)
        {
            UnityEngine.Vector3 posInScene = 
                Raindrop.Utilities.MapSpaceConverters.Handle2Vector3(handle, mapTileZBuffer);

            var map = 
                Instantiate(mapTilePrefab, posInScene, UnityEngine.Quaternion.identity, mapRoot.transform);
            // map.layer = mapRoot.layer; //inherit layer.
            return map;
        }


        public static uint mapTileZBuffer { get; set; }
    }
}
