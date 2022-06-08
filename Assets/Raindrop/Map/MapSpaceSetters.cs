using UnityEngine;

namespace Raindrop.Utilities
{
    public class MapSpaceSetters
    {
        //change the rotation of 2d agent to point in same dir as the input
        public static void SetMapItemOrientation(Transform transform, Quaternion orientation)
        {
            // Quaternion newRotation = Quaternion.Euler(0,0,heading);
            transform.rotation = orientation;
        }

        // moves the transform to the 2d map position (x,y).
        public static void SetMapItemPosition(Transform transform, UnityEngine.Vector2 positionVector)
        {
            transform.position = positionVector;
        }
        
    }
}