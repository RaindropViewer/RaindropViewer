using System;
using UnityEngine;
using Raindrop;
using OpenMetaverse;
using UnityEngine.Serialization;
using Vector3 = UnityEngine.Vector3;

namespace Raindrop.UI.Views
{
    //an instance of a map 'look at' ; the focal point of camera.
    public class MapLookAt : MonoBehaviour
    {
        [FormerlySerializedAs("cameraToControl")] 
        public OrthographicCameraView orthoCam;
        
        //grid coordinates * 256
        [SerializeField]
        public uint lookAt_x => Convert.ToUInt32(floatingLookAt_x * 256); //1000 * 256;
        [SerializeField]
        public uint lookAt_y => Convert.ToUInt32(floatingLookAt_y * 256); //1000 * 256;

        //grid coordinates * 1
        [SerializeField]
        private float floatingLookAt_x = 1000;
        [SerializeField]
        private float floatingLookAt_y = 1000;
        
        //values when the finger is not released yet.
        [FormerlySerializedAs("lookAt_pre_release_x")] [SerializeField]
        public float lookAt_old_x = 1000;
        [FormerlySerializedAs("lookAt_pre_release_y")] [SerializeField]
        public float lookAt_old_y = 1000;



        /// <summary>
        /// get lookat of the camera in ( gridCoord * 256 ) units
        /// </summary>
        /// <returns></returns>

        [Obsolete]
        public ulong GetLookAt()
        {
            return OpenMetaverse.Utils.UIntsToLong(lookAt_x, lookAt_y);

        }

        /// <summary>
        /// Move the *floating* lookAt by this amount.
        /// moves by an relative amount - that is, all parameters of this call parameters are relative to the *inital touch location*.
        /// </summary>
        /// <param name="x">change of x - in units where there are 256 units per Sim. </param>
        /// <param name="y"></param>
        [Obsolete]
        public void SetLookAt_Relative_OnDrag(float x, float y)
        {
            floatingLookAt_x = lookAt_old_x + x;
            floatingLookAt_y = lookAt_old_y + y;

            orthoCam.transform.position = 
                new UnityEngine.Vector3(
                floatingLookAt_x,
                 this.transform.position.x,
                floatingLookAt_y);

        }

        /// <summary>
        /// Sets the lookAt to a final position. Call this when the finger is released. (finger come up.)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetLookAt_Relative_OnRelease(float x, float y)
        {
            floatingLookAt_x = lookAt_old_x + x;
            floatingLookAt_y = lookAt_old_y + y;

            lookAt_old_x = floatingLookAt_x;
            lookAt_old_y = floatingLookAt_y;
        }

        //units is in world space.
        public void SetLookAt_ByDelta(float deltaX, float deltaY)
        {
            
            orthoCam.transform.position -= 
                (Vector3) new UnityEngine.Vector2(
                    deltaX,
                    deltaY);
        }

        //if > 1 means zoom OUT, if < 1 means zoom in .
        public void SetLookAt_Zoom_ByDelta(float zoomRatio)
        {
            orthoCam.Zoom *= zoomRatio;

        }
    }
}