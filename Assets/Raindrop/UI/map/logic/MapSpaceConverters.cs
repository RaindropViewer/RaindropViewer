using UE = UnityEngine;
using OpenMetaverse;
using System;

namespace Raindrop.Utilities
{


    public static class MapSpaceConverters {
        #region Global Handle converters

        //convert grid handle to unity vector3.
        // ie get tile position in 3d render space from secondlife grid coordinates.
        // help you put the tile in the right place.
        public static UE.Vector3 Handle2Vector3(ulong handle, uint zBuffer)
        {
            uint x, y;
            OpenMetaverse.Utils.LongToUInts(handle, out x, out y);
            return new UE.Vector3(x / 256, y / 256, zBuffer );
        }
        
        // convert game space map entities into their positions in the grid-space(handle space)
        public static ulong Vector32Handle(UE.Vector3 v3)
        {
            var handle = Utils.UIntsToLong((uint)v3.x * 256, (uint)v3.y * 256);
            return handle;
        }


        //get the 2d-coordinates in grid coordinate units.
        // help you get convert 3d render space 'tile' coordinates to internal representation in secondlife.
        [Obsolete]
        public static ulong getRegionFromWorldPoint(UE.Vector3 worldPoint)
        {
            return Utils.UIntsToLong((uint)worldPoint.x * 256, (uint)worldPoint.z * 256);
            //throw new NotImplementedException();
        }
        
        #endregion


        #region Region's coordinate Converters

        // get region's coordinate X
        public static uint Vector32_GridX(UE.Vector3 v3)
        {
            return (uint) v3.x;
        }
        // get region's coordinate Y
        public static uint Vector32_GridY(UE.Vector3 v3)
        {
            return (uint) v3.y;
        }
        
        #endregion
    }
}