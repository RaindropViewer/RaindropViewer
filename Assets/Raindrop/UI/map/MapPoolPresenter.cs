using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using Raindrop.Map;

namespace Raindrop.UI
{
    // manages the pool of map textures and gameobjects.
    // map pool.
    // map maker

    class MapPoolPresenter : MonoBehaviour
    {


        [SerializeField]
        public GameObject mapPrefab;

        private Dictionary<Vector2Int, GameObject> map_collection; //coordinates -> GO


        //viewable area of the current map_collection.
        public int max_X, max_Y;
        public int min_X, min_Y;

        //the current zoom level that the user is requesting.
        // 1 - one 256^2 texture is 1 sim.
        // ...
        // 4 - one 256^2 texture is 8*8 sims.
        public int zoomLevel;

        private void Awake()
        {
            max_X = max_Y = max_X = max_Y = 1000;
            zoomLevel = 1;
            var map = Instantiate(mapPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            map_collection.Add(new Vector2Int(1000,1000), map);
        }

        private void createMapTileAt(Vector2Int pos, int zoomLevel)
        {
            var map = Instantiate(mapPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            map_collection.Add(pos, map);


        }
        private void clearTiles()
        {
            map_collection.Clear();

        }

        //private void setViewableRangeMinMax(OpenMetaverse.Vector2 min, OpenMetaverse.Vector2 max)
        //{
        //    // re-composites the scene based on the viewable region
        //    updateTexturesIfNecessary(min, max);
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="focalPoint">Where the camera is looking at - in Sim coordinates*256 ie handle coords. </param>
        /// <param name="range"></param>
        public void setViewableRange(OpenMetaverse.Vector2 focalPoint, OpenMetaverse.Vector2 range)
        {
            var min = MapLogic.getMinVec2(range, focalPoint);
            var max = MapLogic.getMaxVec2(range, focalPoint);

            // re-composites the scene based on the viewable region
            updateTexturesIfNecessary(min, max);
        }
        public void setViewableRange(uint x,uint y, OpenMetaverse.Vector2 range)
        {
            setViewableRange(new OpenMetaverse.Vector2(x,y), range);
        }

        // min : bottom left
        // max : top right
        private void updateTexturesIfNecessary(OpenMetaverse.Vector2 min, OpenMetaverse.Vector2 max)
        {
            //'round-up' the corners.
            int _max_X = (int) Mathf.CeilToInt(max.X);
            int _max_Y = (int) Mathf.CeilToInt(max.Y);
            int _min_X = (int) Mathf.CeilToInt(min.X);
            int _min_Y = (int) Mathf.CeilToInt(min.Y);

            //check if all are in the map. 
            ////obtain list of viewable textures.
            ////List<Vector2Int> list = new List<Vector2Int>();
            for (int i = _min_Y; i <= _max_Y; i++)
            {
                for (int j = _min_X; j <= _max_X; j++)
                {
                    var tocheck = new Vector2Int(j, i);
                    if (! map_collection.ContainsKey(tocheck))
                    {

                    } 
                    //list.Add( new Vector2Int(j,i) );
                }
            }

        }

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
