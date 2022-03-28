using System;
using System.Collections.Generic;
using OpenMetaverse;
using Raindrop.UI.Views;
using UnityEngine;
using UnityEngine.Serialization;
using UE = UnityEngine;

namespace Raindrop.UI.map.Map_SceneHierachy
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

        public int poolSize = 10;

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
        private Dictionary<ulong, float> map_collection_lastSeenTime = new Dictionary<ulong, float>(); //tiles and their lastseentime.
        private bool stillDrawing = false;

        public MapTileRecycler TileRecycler;
        [FormerlySerializedAs("tileTimeToLive")] [SerializeField] private float tileTimeToDestroy = 10f;

        private RaindropInstance instance => ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>();

        private void Awake()
        {
            TileRecycler = new MapTileRecycler(poolSize);
            TileRecycler.objectToPool = mapTilePrefab;
        }

        private void Start()
        {
            instance.Netcom.ClientConnected += NetcomOnClientConnected;
            instance.Netcom.ClientDisconnected += NetcomOnClientDisconnected;
        }

        private void NetcomOnClientDisconnected(object sender, DisconnectedEventArgs e)
        {
            CancelInvoke();
        }

        private void NetcomOnClientConnected(object sender, EventArgs e)
        {
            //only draw map when the user is logged in.
            InvokeRepeating("RedrawMap", RenderUpdatePeriod, RenderUpdatePeriod);
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
                    //update the tile's timer.
                    ResetTileDestructionTimer(map_collection_lastSeenTime, handle);
                }
                
                //4. for tiles that are not seen for a while, we can remove them.
                lock (map_collection_lastSeenTime) //todo: remove me?
                {
                    List<ulong> lastSeenTime_ToRemove = new List<ulong>();
                    foreach (var tile in map_collection_lastSeenTime) //common issue: "InvalidOperationException: Collection was modified; enumeration operation may not execute." 
                    {
                        float lastseentime = tile.Value;
                        ulong handle = tile.Key;
                        if (lastseentime + tileTimeToDestroy < Time.realtimeSinceStartup)
                        {
                            //destroy the tile
                            GameObject tileGO = map_collection[handle];
                            TileRecycler.ReturnToPool(tileGO);
                            map_collection.Remove(handle);
                            // map_collection_lastSeenTime.Remove(handle); //!!!
                            lastSeenTime_ToRemove.Add(handle);
                        }
                    }
                    //do removal!
                    foreach (var remove_handle in lastSeenTime_ToRemove)
                    {
                        map_collection_lastSeenTime.Remove(remove_handle);
                    }
                }
            }
        }

        private void ResetTileDestructionTimer(Dictionary<ulong, float> mapCollectionLastSeenTime, ulong handle)
        {
            lock (mapCollectionLastSeenTime)
            {
                if (! map_collection_lastSeenTime.ContainsKey(handle))
                {
                    map_collection_lastSeenTime.Add(handle, 0f);
                }
                map_collection_lastSeenTime[handle] = Time.realtimeSinceStartup;
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
            var map = TileRecycler.GetPooledObject();
            map.transform.position = posInScene;
            map.transform.rotation = UnityEngine.Quaternion.identity;
            map.transform.SetParent(mapRoot.transform);
            map.SetActive(true);
            // var map = 
            //     Instantiate(mapTilePrefab, posInScene, UnityEngine.Quaternion.identity, mapRoot.transform);
            return map;
        }

    }

    public class MapTileRecycler
    {
        public int DefaultSize = 10;
        public List<GameObject> pooledObjects = new List<GameObject>();
        public GameObject objectToPool;
        [SerializeField] public bool shouldExpand = true;

        
        public MapTileRecycler(int defaultSize)
        {
            DefaultSize = defaultSize;
        }

        public GameObject GetPooledObject() {
            for (int i = 0; i < pooledObjects.Count; i++) {
                if (!pooledObjects[i].activeInHierarchy) {
                    return pooledObjects[i];
                }
            }
            
            // "lazy instantiation" is over here :)
            if (shouldExpand) {
                GameObject obj = (GameObject)MonoBehaviour.Instantiate(objectToPool);
                obj.SetActive(false);
                pooledObjects.Add(obj);
                return obj;
            } else {
                return null;
            }
        }

        public void ReturnToPool(GameObject gameObject)
        {
            gameObject.SetActive(false);
        }

    }
}
