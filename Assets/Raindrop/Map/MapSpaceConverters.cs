using UE = UnityEngine;
using OpenMetaverse;
using System;
using Raindrop.Rendering;
using Unity.Mathematics;
using UnityEngine.Assertions;

namespace Raindrop.Utilities
{
    public static class MapSpaceConverters {
        #region Global Handle converters

        //convert grid handle to unity vector3.
        // ie get tile position in 3d render space from secondlife grid coordinates.
        // help you put the tile in the right place.
        public static UE.Vector3 Handle2MapSpace(ulong handle, uint zBuffer)
        {
            uint x, y;
            OpenMetaverse.Utils.LongToUInts(handle, out x, out y);
            return new UE.Vector3(x / 256.0f, y / 256.0f, zBuffer );
        }
        
        // convert game space map entities into their positions in the grid-space(handle space)
        public static ulong MapSpace2Handle(UE.Vector3 v3)
        {
            var handle = Utils.UIntsToLong((uint)v3.x * 256, (uint)v3.y * 256);
            return handle;
        }
        
        // convert the rotation in SL to rotation in unitymap space
        public static UE.Quaternion GlobalRot2MapRot(Quaternion globalQuaternion)
        {
            //1. get euler around the vertical axis.
            UE.Quaternion unity_space_rot = RHelp.TKQuaternion4(globalQuaternion);
            var degrees_verticalAxis_rotation = unity_space_rot.eulerAngles.y;
            
            //2. create identity rotation in the mapspace.
            UE.Quaternion mapSpace_identity = UE.Quaternion.identity; //quaternion.Euler(90,0,0);
            
            //3. apply the euler to the identitiy rotation found in (2)
            var mapSpace_rot = mapSpace_identity * UE.Quaternion.AngleAxis(degrees_verticalAxis_rotation, UE.Vector3.back);
            return mapSpace_rot;

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
        public static uint MapSpace2Grid_X(UE.Vector3 v3)
        {
            return (uint) v3.x;
        }
        // get region's coordinate Y
        public static uint MapSpace2Grid_Y(UE.Vector3 v3)
        {
            return (uint) v3.y;
        }
        
        #endregion

        #region Opensim's Global Space, aka OMV.Vector3d

        // global space as in vector3d, in meters.
        // the return is a place on the map in unity game engine space.
        public static UnityEngine.Vector3 GlobalSpaceToMapSpace(Vector3d selfGlobalPosition_meters)
        {
            uint x = (uint) selfGlobalPosition_meters.X; //get x,y up to the nearest meter.
            uint y = (uint) selfGlobalPosition_meters.Y;

            var handle = OpenMetaverse.Utils.UIntsToLong(x, y);

            UnityEngine.Vector3 agentInMapSpace = MapSpaceConverters.Handle2MapSpace(handle, 0);

            Assert.IsFalse(PostionIsOOB(agentInMapSpace), "OOB: Present map-pos is : " + agentInMapSpace);

            return agentInMapSpace;
        }

        private static bool PostionIsOOB(UE.Vector3 agentInMapSpace)
        {
            return ((agentInMapSpace.x < 0) || 
                   (agentInMapSpace.y < 0) ||
                   agentInMapSpace.x > 65536 ||
                   agentInMapSpace.y > 65536 );
        }

        #endregion


        // given the large global coordinate of this item, where is it in unity coordinates? can get quite large (>> 4000m away)
        // translate the vector2 globalMapPositions into scene vector3 transform positions
        // public static UnityEngine.Vector3 fromMapCoord(UnityEngine.Vector2 mapPos, float mapItemDepthConstant)
        // {
        //     return new UnityEngine.Vector3(mapPos.x, mapPos.y, mapItemDepthConstant);
        // }

        // if there was an object in this position in unity engine space,
        // where would we put it on the minimap?
        // public static UnityEngine.Vector2 GlobalUnity2MapPlane(UnityEngine.Vector3 GlobalUnity)
        // {
        //     return new UnityEngine.Vector2(GlobalUnity.x / 256, GlobalUnity.z / 256);
        // }

    }
}