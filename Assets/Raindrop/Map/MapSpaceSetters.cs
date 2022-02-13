using UnityEngine;

namespace Raindrop.Utilities
{
    public class MapSpaceSetters
    {
        //change the rotation of 2d agent to point in same dir as the input
        public static void SetMapItemRotation(Transform transform, UnityEngine.Vector3 rotation_InMap)
        {
            transform.eulerAngles = rotation_InMap;
        }

        // moves the transform to the 2d map position (x,y).
        public static void SetMapItemPosition(Transform entitiyTransform, UnityEngine.Vector2 mapPos)
        {
            entitiyTransform.position = mapPos;
        }
        
    }
}