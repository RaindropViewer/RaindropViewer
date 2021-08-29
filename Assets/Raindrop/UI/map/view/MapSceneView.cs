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

namespace Raindrop.UI.Views
{
    /// <summary>
    /// the view that is in the root of the map layer/ map scene.
    /// - instantiates map tile prefabs.
    /// - keeps track of the map tiles in a dict.
    /// - contains the camera
    /// - has an api for us to specify the rendered regions.
    /// </summary>

    public class MapSceneView : MonoBehaviour
    {
        private MapPoolPresenter mapPoolPresenter;


        [SerializeField]
        public GameObject mapPrefab;

        //camera. contains the viewable range.
        [SerializeField]
        public GameObject cameraViewGO;
        private DownwardOrthoCameraView cameraView;

        private Dictionary<ulong, GameObject> map_collection = new Dictionary<ulong, GameObject>(); //tiles that are in the scene.


        //viewable area of the current map_collection.
        public int max_X, max_Y;
        public int min_X, min_Y;

        /// <summary>
        ///zoom level of map. 
        /// 1 : height of camera is 1 //see about 1 map only
        /// 2 : height of camera is 2 
        /// 10 : height of camera is 10 //see alot
        /// </summary>
        public int zoomLevel;


        private void Awake()
        {

            cameraView = cameraViewGO.GetComponent<DownwardOrthoCameraView>();

            mapPoolPresenter = new MapPoolPresenter(this);

            max_X = max_Y = max_X = max_Y = 1000;
            zoomLevel = 1;
        }


        //private UE.Vector2Int handleToVector2(ulong handle)
        //{
        //    uint x, y;
        //    Utils.LongToUInts(handle, out x, out y);
        //    return new UnityEngine.Vector2Int((int)x, (int)y);
        //}

        public void createMapTileAt(ulong handle, MapTile tile)
        {
            //var pos = handleToVector2(handle);

            UE.Vector3 posInScene = toV3(handle); 

            var map = Instantiate(mapPrefab, posInScene, UE.Quaternion.identity);
            map_collection.Add(handle, map);

            map.GetComponent<MapTileView>().setRawImage(tile.getTex());
        }

        private UE.Vector3 toV3(ulong handle)
        {
            uint x, y;
            Utils.LongToUInts(handle, out x, out y);

            return new UE.Vector3(x/256, 0, y/256);
        }

        internal DownwardOrthoCameraView getCameraView()
        {
            return cameraView;
        }

        private void clearTiles()
        {
            map_collection.Clear();

        }
        
        internal bool isPresent(ulong handle)
        {
            //var pos = handleToVector2(handle);
            return map_collection.ContainsKey(handle);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="focalPoint">Where the camera is looking at - in Sim coordinates*256 ie handle coords. </param>
        /// <param name="range"></param>
        //public void setViewableRange(OpenMetaverse.Vector2 focalPoint, OpenMetaverse.Vector2 range)
        //{
        //    var min = MapBackend.getMinVec2(range, focalPoint);
        //    var max = MapBackend.getMaxVec2(range, focalPoint);

        //    // re-composites the scene based on the viewable region
        //    updateTexturesIfNecessary(min, max);
        //}
        //public void setViewableRange(uint x,uint y, OpenMetaverse.Vector2 range)
        //{
        //    setViewableRange(new OpenMetaverse.Vector2(x,y), range);
        //}


        ////sets a map block that is 256pic * 256pic
        //private void SetMapLayer(Texture2D new_texture, Vector2Int regionXY)
        //{
        //    Debug.Log("setting the image to the new gameobject");
        //    if (mapManager.map_collection.ContainsKey(regionXY))
        //    {
        //        //update the region.
        //        //MonoBehaviour theGO;
        //        //map_collection.TryGetValue(regionXY, out theGO);

        //        //Destroy(theGO.map_tex); //delete the tex2d that is no longer (?) used.
        //    }
        //    else
        //    {
        //        GameObject mapGO = new GameObject();
        //        mapGO.transform.SetParent(this.transform);

        //        var MR = mapGO.AddComponent<MeshRenderer>();
        //        MR.sharedMaterial = new UnityEngine.Material(Shader.Find("Standard"));
        //        //MR.
        //        var MF = mapGO.AddComponent<MeshFilter>();

        //        //generate the plane.
        //        Mesh m = new Mesh();
        //        var width = 1;
        //        var height = 1;
        //        m.vertices = new Vector3[]{
        //                        new Vector3(0, 0, 0),
        //                        new Vector3(width, 0, 0),
        //                        new Vector3(width, height, 0),
        //                        new Vector3(0, height, 0)
        //        };
        //        m.uv = new Vector2[]{
        //            new Vector2(0, 0),
        //            new Vector2(0, 1),
        //            new Vector2(1, 1),
        //            new Vector2(1, 0),
        //        };
        //        m.triangles = new int[] { 0, 2, 1, 0, 3, 2 }; //clockwise?
        //        MF.mesh = m;
        //        m.RecalculateBounds();
        //        m.RecalculateNormals();

        //        MR.material.mainTexture = new_texture;

        //        map_collection.Add(regionXY, mapGO);

        //    }


        //}



    }
}
