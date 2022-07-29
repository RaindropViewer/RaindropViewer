using System;
using System.Collections.Generic;
using OpenMetaverse;
using Plugins.CommonDependencies;
using Plugins.ObjectPool;
using Raindrop.UI.Views;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UE = UnityEngine;

namespace Raindrop.UI.map.Map_SceneHierachy
{
    /// <summary>
    /// hello, i update and spawn entities in the map-space. 
    /// </summary>

    public class MapScenePresenter : MonoBehaviour
    {
        RaindropInstance Instance => RaindropInstance.GlobalInstance;
        GridClient Client => Instance.Client;
        
        [SerializeField]
        private const float RenderUpdatePeriod = 2f;

        //a pause button to stop the updating.
        [SerializeField]
        public bool fetchOn = false;

        [SerializeField]
        public uint mapTileZBuffer;

        //the parent to spawn the map tiles under.
        public GameObject mapRoot;

        [SerializeField]
        public GameObject mapTilePrefab;

        //camera. contains the viewable range.
        [FormerlySerializedAs("cameraView")] [SerializeField]
        public OrthographicCameraView mapCameraView;
        [FormerlySerializedAs("cameraView2")] public OrthographicCameraView minimapCameraView;
        
        // track what we are fetching, have fetched, and is present in the scene.
        // the region handle and it's associated tile.
        private Dictionary<ulong, GameObject> regionTiles = new Dictionary<ulong, GameObject>(); //tiles that are in the scene.
        // the ongoing tile requests
        List<ulong> tileRequests = new List<ulong>();
        // the last-draw time of the tiles.
        private Dictionary<ulong, float> map_collection_lastSeenTime = new Dictionary<ulong, float>(); //tiles and their lastseentime.
        // private bool stillDrawing = false;

        //the internal data cache
        private MapSceneData MapSceneData;
        
        // flags for logic.
        private bool needRedraw;

        //the tiles.
        public int poolSize = 10;
        public MapTileRecycler TileRecycler;
        [SerializeField] private float tileTimeToDestroy = 10f;
        

        
        private void Awake()
        {
            TileRecycler = new MapTileRecycler(poolSize);
            TileRecycler.objectToPool = mapTilePrefab;

            MapSceneData = new MapSceneData();
        }

        private void Start()
        {
            Init();
        }

        public void Init()
        {
            RegisterClientEvents();
        }

        public void OnDisable()
        {
            UnregisterClientEvents(Client);
        }


        void RegisterClientEvents()
        {
            Client.Grid.GridItems += new EventHandler<GridItemsEventArgs>(Grid_GridItems);
            Client.Grid.GridRegion += new EventHandler<GridRegionEventArgs>(Grid_GridRegion);
            Client.Grid.GridLayer += new EventHandler<GridLayerEventArgs>(Grid_GridLayer);
            
            Instance.Netcom.ClientConnected += NetcomOnClientConnected;
            Instance.Netcom.ClientDisconnected += NetcomOnClientDisconnected;
        }

        void UnregisterClientEvents(GridClient Client)
        {
            if (Client == null) return;
            Client.Grid.GridItems -= new EventHandler<GridItemsEventArgs>(Grid_GridItems);
            Client.Grid.GridRegion -= new EventHandler<GridRegionEventArgs>(Grid_GridRegion);
            Client.Grid.GridLayer -= new EventHandler<GridLayerEventArgs>(Grid_GridLayer);
            
            Instance.Netcom.ClientConnected -= NetcomOnClientConnected;
            Instance.Netcom.ClientDisconnected -= NetcomOnClientDisconnected;
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

        //avatar goes in here
        void Grid_GridItems(object sender, GridItemsEventArgs e)
        {
            foreach (MapItem item in e.Items)
            {
                if (item is MapAgentLocation)
                {
                    MapAgentLocation loc = (MapAgentLocation)item;
                    lock (MapSceneData.regionMapItems)
                    {
                        if (!MapSceneData.regionMapItems.ContainsKey(item.RegionHandle))
                        {
                            MapSceneData.regionMapItems[loc.RegionHandle] = new List<MapItem>();
                        }
                        MapSceneData.regionMapItems[loc.RegionHandle].Add(loc);
                    }
                    
                    needRedraw = true;
                }
            }
        }
        
        //region-is-present comes in here
        //is ignored if we are on SL grid.
        void Grid_GridRegion(object sender, GridRegionEventArgs e)
        {
            if (Instance.Netcom.IsTeleporting) // ????
                return;
            needRedraw = true;
            if (e is null)
            {
                Debug.LogWarning("e is null");
                return;
            }
            if (MapSceneData.regions is null)
            {
                Debug.LogWarning("MapSceneData.regions is null");
                return;
            }
            // this is causing some NRE
            // MapSceneData.regions[e.Region.RegionHandle] = e.Region;
            if (e.Region.Access != SimAccess.NonExistent
                && e.Region.MapImageID != UUID.Zero
                && !tileRequests.Contains(e.Region.RegionHandle)
                && !regionTiles.ContainsKey(e.Region.RegionHandle))
            {

                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    DownloadRegionTile(e.Region.RegionHandle, e.Region.MapImageID);
                });
            }
        }


        void Grid_GridLayer(object sender, GridLayerEventArgs e)
        {
        }        
        
        // retrieve a tile from the asset server, and in the callback, decode to maptile.
        void DownloadRegionTile(ulong handle, UUID imageID)
        {
            Assert.IsTrue(UnityMainThreadDispatcher.isOnMainThread());
            
            if (regionTiles.ContainsKey(handle)) return;

            lock (tileRequests)
                if (!tileRequests.Contains(handle))
                    tileRequests.Add(handle);


            Uri url = Client.Network.CurrentSim.Caps.GetTextureCapURI();
            if (url != null)
            {
                Client.Assets.RequestImage(imageID, ImageType.Normal, (state, asset) =>
                {
                    if (state != TextureRequestState.Finished)
                        return;
                    if (asset?.AssetData == null)
                    {
                        //give up and stop trying for texture.
                        return;
                    }
                    
                    // case: app is already closed.
                    if (!UnityMainThreadDispatcher.Exists())
                        return;
                    
                    UnityMainThreadDispatcher.Instance().Enqueue(
                        () =>
                        {
                            try
                            {
                                asset.Decode(); //warn: main thread only.
                                var t2d = TexturePoolSelfImpl.GetInstance().GetFromPool(TextureFormat.ARGB32);
                                asset.Image.ExportTex2D(t2d); //decode: assetData -> texture
                                
                                //spawn in the tile.
                                var mapPlanePos = GetMapPlanePosition(handle);
                                // Debug.Log("renderer wants to render map tile at " + mapPlanePos.x + " " +
                                //           mapPlanePos.y);
                                var tileGO = SpawnOnMapPlane(mapPlanePos);
                                
                                //apply texture.
                                var textureComponent = tileGO.GetComponent<TexturableMesh>();
                                textureComponent.ApplyTexture(t2d);

                                regionTiles.Add(handle, tileGO);
                                
                                needRedraw = true;
                            }
                            catch (Exception e)
                            {
                                Debug.LogError("maptile (jp2) decode failed at " + e.Message);
                            }
                            finally
                            {
                                lock (tileRequests)
                                {
                                    if (tileRequests.Contains(handle))
                                    {
                                        tileRequests.Remove(handle);
                                    }
                                }
                            }
                        });
                });
            }
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
            // if (stillDrawing)
            // {
            //     return;
            // }
            // stillDrawing = true;
            
            // AddAndRemoveTiles();
            bool cameraIsMoved = true; 
            if (cameraIsMoved)
            {
                SendRequestForMapBlocks();
            }

            
            
            // stillDrawing = false;

            void AddAndRemoveTiles()
            {
                //1. get the zoom level based on height of camera.
                int zoom = (int)mapCameraView.Zoom;

                //2. obtain the range that the camera can see, in grid coordinates.
                var handles =
                    MapCameraLogic.getVisibleRegionHandles(
                        mapCameraView.Min,
                        mapCameraView.Max,
                        zoom); // add 1 int to the result, as each sim is 1-large.
                var minimapHandles = MapCameraLogic.getVisibleRegionHandles(
                        minimapCameraView.Min,
                        minimapCameraView.Max,
                        1);
                handles.UnionWith(minimapHandles);

                int maxFetchPerLoop = 4;
                //3. spawn the tiles that are in this space.
                foreach (var handle in handles)
                {
                    if (!regionTiles.ContainsKey(handle))
                    {
                        //rate-limiter...
                        if (maxFetchPerLoop-- == 0)
                        {
                            break;
                        }

                        //not avail in scene, so instantiate the tile.
                        var mapPlanePos = GetMapPlanePosition(handle);
                        // Debug.Log("renderer wants to render map tile at " + mapPlanePos.x + " " + mapPlanePos.y);
                        var tileGO = SpawnOnMapPlane(mapPlanePos);
                        regionTiles.Add(handle, tileGO);
                    }
                    //reset the tile's timer.
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
                            GameObject tileGO = regionTiles[handle];
                            TileRecycler.ReturnToPool(tileGO);
                            regionTiles.Remove(handle);
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

        private void SendRequestForMapBlocks()
        {
            Instance.Client.Grid.RequestMapBlocks(GridLayerType.Objects, 
                (ushort)mapCameraView.Min.x, 
                (ushort)mapCameraView.Min.y, 
                (ushort)mapCameraView.Max.x, 
                (ushort)mapCameraView.Max.y,
                true);

            Instance.Client.Grid.RequestMapBlocks(GridLayerType.Objects, 
                (ushort)minimapCameraView.Min.x, 
                (ushort)minimapCameraView.Min.y, 
                (ushort)minimapCameraView.Max.x, 
                (ushort)minimapCameraView.Max.y,
                true);
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

        //get the handle/sim's position in the unity space.
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


    internal class MapSceneData
    {
        //items appearing in the block requests
        public Dictionary<ulong, List<MapItem>> regionMapItems =
            new Dictionary<ulong, List<MapItem>>();

        //list of regions that are present on grid.
        public Dictionary<ulong, GridRegion> regions =
            new Dictionary<ulong, GridRegion>();
        
    }
}
