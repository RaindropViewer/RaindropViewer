using UE = UnityEngine;
using OpenMetaverse;
using System;

namespace Raindrop.Utilities
{


    public static class Coverters {

        //convert grid handle to unity vector3.
        // ie get tile position in 3d render space from secondlife grid coordinates.
        // help you put the tile in the right place.
        public static UE.Vector3 Handle2Vector3(ulong handle)
        {
            uint x, y;
            OpenMetaverse.Utils.LongToUInts(handle, out x, out y);

            return new UE.Vector3(x / 256, 0, y / 256);
        }

        //get the 2d-coordinates in grid coordinate units.
        // help you get convert 3d render space 'tile' coordinates to internal representation in secondlife.
        public static ulong getRegionFromWorldPoint(UE.Vector3 worldPoint)
        {
            return Utils.UIntsToLong((uint)worldPoint.x * 256, (uint)worldPoint.z * 256);
            //throw new NotImplementedException();
        }
    }
}