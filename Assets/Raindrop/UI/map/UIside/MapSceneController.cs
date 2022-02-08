using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using Raindrop.Map;
using Raindrop.UI.Views;
using Raindrop.Netcom;
using OpenMetaverse;
using Raindrop.Map.Model;
using Vector2 = UnityEngine.Vector2;
using Raindrop.UI.Model;

namespace Raindrop.UI.Presenters
{
    //some key ideas:
    /* if my finger is clicking the map, I have intention to navigate and pan the map.
     * if my finger is not clicking the map, I have no intention to do anything with the map.
     *
     * if i tap the map, the informaiton of the coordinate i tap will be displayed as a card.
     *
     * i only fetch the map if my camera is shooting at it.
     * 
     */

    public class MapSceneController
    {
        //private MapModel mapModel;


        public event EventHandler<string> MapClicked;
        public virtual void OnMapClick(ulong regionCoords) //protected virtual method
        {
            //if ProcessCompleted is not null then call delegate
            MapClicked?.Invoke(this, regionCoords.ToString() + "to implement sim name and pos as string in event. ");
        }

        public MapSceneController(MapUIView mapUIView, MapSceneView mapSceneView)
        {
            //this.mapSceneView = mapSceneView;
            //mapFetcher = ServiceLocator.ServiceLocator.Instance.Get<MapBackend>();
            // mapModel = new MapModel();

            mapSceneView.gameObject.SetActive(true);
        }


        /// <summary>
        /// get a list of region handles that are visible to the camera.
        /// </summary>
        /// <returns></returns>
        public static HashSet<ulong> getVisibleRegionHandles(Vector2 min, Vector2 max)
        {
            // for loop from bounds to bounds.
            HashSet<ulong> visiblehandles = new HashSet<ulong>();
            int vert_min = (int)min.y;
            int vert_max = (int)max.y;
            int horz_min = (int)min.x;
            int horz_max = (int)max.x;
            for (int i = horz_min; i < horz_max; i++) //horizontal
            {
                for (int j = vert_min; j < vert_max; j++) //vertical
                {
                    ulong region = OpenMetaverse.Utils.UIntsToLong((uint)(i * 256), (uint)(j * 256));
                    visiblehandles.Add(region);
                }
            }

            return visiblehandles;
        }

        internal void RedrawMap()
        {

            // var visibleHandles = getVisibleRegionHandles();

            GridLayer gridLayer = new OpenMetaverse.GridLayer{
                Bottom = 1,
                Left = 1,
                Right =1,
                Top = 1
            };
            throw new NotImplementedException();
        }
    }
}
