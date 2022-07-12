using UnityEngine;

namespace Raindrop.Camera
{
    public class CameraIdentifier : MonoBehaviour
    {
        public CameraType type;

        public enum CameraType
        {
            Main,
            Minimap
        }
    }
}
