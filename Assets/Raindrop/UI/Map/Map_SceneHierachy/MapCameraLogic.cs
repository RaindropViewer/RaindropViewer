using System.Collections.Generic;
using UnityEngine;

namespace Raindrop.UI.Views
{
    internal static class MapCameraLogic
    {
        
        /// <summary>
        /// get a list of region handles that are visible to the camera.
        /// width : the number of regions on each side of the tile. the smallest is 1, where each tile is a sim.
        /// if width is > 1, then we will return a set of handles that are the origins of each combined-tile; bottom-left sim.
        /// </summary>
        /// <returns></returns>
        public static HashSet<ulong> getVisibleRegionHandles(Vector2 min, Vector2 max, int width)
        {
            // for loop from bounds to bounds.
            HashSet<ulong> visiblehandles = new HashSet<ulong>();
            int vert_min = (int)min.y;
            int vert_max = (int)max.y + 1;
            int horz_min = (int)min.x;
            int horz_max = (int)max.x + 1;
            for (int i = horz_min; i < horz_max; i+=width) //horizontal
            {
                for (int j = vert_min; j < vert_max; j+=width) //vertical
                {
                    ulong region = OpenMetaverse.Utils.UIntsToLong((uint)(i * 256), (uint)(j * 256));
                    visiblehandles.Add(region);
                }
            }

            return visiblehandles;
        }
        
    }
}