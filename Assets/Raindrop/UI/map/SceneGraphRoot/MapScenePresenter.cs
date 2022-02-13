using System.Collections.Generic;
using UnityEngine;
using UE = UnityEngine;

namespace Raindrop.UI.Views
{
    /// <summary>
    /// hello, i spawn map tiles inside of the 3d space. 
    /// </summary>

    public class MapScenePresenter : MonoBehaviour
    {
        [SerializeField]
        private const float RenderUpdatePeriod = 2f;
        [SerializeField]
        public bool fetchOn = false;

        [SerializeField]
        public uint mapTileZBuffer;

        //the parent to spawn the map tiles under.
        public GameObject mapRoot;

        [SerializeField]
        public GameObject mapTilePrefab;

        //camera. contains the viewable range.
        [SerializeField]
        public OrthographicCameraView cameraView;
        public OrthographicCameraView cameraView2;
        private Dictionary<ulong, GameObject> map_collection = new Dictionary<ulong, GameObject>(); //tiles that are in the scene.
        private bool stillDrawing = false;
                
        private void Start()
        {
            Init();

            InvokeRepeating("RedrawMap", RenderUpdatePeriod, RenderUpdatePeriod);
        }

        public void Init()
        {
            //resetView();
        }
        

        private void RedrawMap()
        {
            //only redraw if the root of mapGraph is activated.
            if (! this.isActiveAndEnabled)
            {
                return;
            }

            /// ??? need?
            if (! fetchOn)
            {
                return;
            }

            //prevent drawing if prevous pass is not done.
            if (stillDrawing)
            {
                return;
            }
            stillDrawing = true;

            DrawTiles();

            stillDrawing = false;

            void DrawTiles()
            {
                //1. get the zoom level based on height of camera.
                int zoom = (int)cameraView.Zoom;

                //2. obtain the range that the camera can see, in grid coordinates.
                var handles =
                    MapCameraLogic.getVisibleRegionHandles(
                        cameraView.Min,
                        cameraView.Max,
                        zoom); // add 1 int to the result, as each sim is 1-large.
                var minimapHandles = MapCameraLogic.getVisibleRegionHandles(
                        cameraView2.Min,
                        cameraView2.Max,
                        1);
                handles.UnionWith(minimapHandles);

                int maxFetchPerLoop = 4;
                //3. spawn the tiles that are in this space.
                foreach (var handle in handles)
                {
                    if (!map_collection.ContainsKey(handle))
                    {
                        //rate-limiter...
                        if (maxFetchPerLoop-- == 0)
                        {
                            break;
                        }

                        //not avail in scene, so instantiate the tile.
                        var mapPlanePos = GetMapPlanePosition(handle);
                        Debug.Log("renderer wants to render map tile at " + mapPlanePos.x + " " + mapPlanePos.y);
                        var tileGO = SpawnOnMapPlane(mapPlanePos);
                        map_collection.Add(handle, tileGO);
                    }
                }
            }
        }

        private UE.Vector3 GetMapPlanePosition(ulong handle)
        {
            UnityEngine.Vector3 posInScene = 
                Raindrop.Utilities.MapSpaceConverters.Handle2MapSpace(handle, mapTileZBuffer);
            return posInScene;
        }

        //Spawn the tile, at the global "handle" coordinates
        public GameObject SpawnOnMapPlane(UE.Vector3 posInScene)
        {
            var map = 
                Instantiate(mapTilePrefab, posInScene, UnityEngine.Quaternion.identity, mapRoot.transform);
            return map;
        }

    }
}
