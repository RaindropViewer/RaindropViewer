using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using Raindrop.Map;
using Raindrop.UI.Views;

namespace Raindrop.UI
{
    /// <summary>
    /// manages the pool of map textures and gameobjects.
    /// map pool.
    /// map maker
    /// 
    /// </summary>

    class MapPoolPresenter
    {
        private MapSceneView mapSceneView;

        
        //viewable area of the current map_collection.
        public int max_X, max_Y;
        public int min_X, min_Y;

        //the current zoom level that the user is requesting.
        // 1 - one 256^2 texture is 1 sim.
        // ...
        // 4 - one 256^2 texture is 8*8 sims.
        public int zoomLevel;

        public MapPoolPresenter(MapSceneView mapSceneView)
        {
            this.mapSceneView = mapSceneView;

            max_X = max_Y = max_X = max_Y = 1000;
            zoomLevel = 1;
        }


        //private void clearTiles()
        //{
        //    map_collection.Clear();

        //}

        //// min : bottom left
        //// max : top right
        //private void updateTexturesIfNecessary(OpenMetaverse.Vector2 min, OpenMetaverse.Vector2 max)
        //{
        //    //'round-up' the corners.
        //    int _max_X = (int) Mathf.CeilToInt(max.X);
        //    int _max_Y = (int) Mathf.CeilToInt(max.Y);
        //    int _min_X = (int) Mathf.CeilToInt(min.X);
        //    int _min_Y = (int) Mathf.CeilToInt(min.Y);

        //    //check if all are in the map. 
        //    ////obtain list of viewable textures.
        //    ////List<Vector2Int> list = new List<Vector2Int>();
        //    for (int i = _min_Y; i <= _max_Y; i++)
        //    {
        //        for (int j = _min_X; j <= _max_X; j++)
        //        {
        //            var tocheck = new Vector2Int(j, i);
        //            if (! map_collection.ContainsKey(tocheck))
        //            {

        //            } 
        //            //list.Add( new Vector2Int(j,i) );
        //        }
        //    }

        //}
         

    }
}
