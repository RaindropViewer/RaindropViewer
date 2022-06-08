using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UE = UnityEngine;

namespace Raindrop.UI.Views
{
    /// <summary>
    /// Controls the camera in map view,
    /// expressing positions in sim units (eg 1000,1000)
    /// </summary>
    public class OrthographicCameraView : MonoBehaviour
    {
        [SerializeField]
        private float MinZoom = 0.1f;
        [SerializeField]
        private float MaxZoom = 10f;
        [FormerlySerializedAs("cam")] [FormerlySerializedAs("camera")] public Camera Cam;

        // Obtain the bottom left corner of the viewable region
        // unity units. -- you need to x256 to get handle-units
        public UE.Vector2 Min
        {
            get
            {
                var pos_x = centerX - halfHeightX;
                var pos_y = centerY - halfHeightY;

                return new UE.Vector2(pos_x, pos_y);
            }
        }

        // Obtain the top right corner of the viewable region
        // unity units. -- you need to x256 to get handle-units
        public UE.Vector2 Max
        {
            get
            {
                var pos_x = centerX + halfHeightX;
                var pos_y = centerY + halfHeightY;

                return new UE.Vector2(pos_x, pos_y);
            }
        }

        /// <summary>
        /// Camera central axes
        /// </summary>
        private float centerX => this.transform.position.x;

        private float centerY => this.transform.position.y;
        private float halfHeightX => halfHeightY * Cam.aspect;
        private float halfHeightY => Cam.orthographicSize;

        /// <summary>
        /// Set the position of the camera based on the (grid) x,y coordinates -- eg: daboom is 1000,1000
        /// </summary>
        /// <param name="vec"></param>
        public void SetToGridPos(UE.Vector2 vec)
        {
            var dest = new UE.Vector3
                (vec.x, vec.y, transform.position.z);
            lerpCamTo(dest, 1);
        }


        public void lerpCamTo(Vector2 targetPosition, float duration)
        {
            var coroutine = lerpTo(targetPosition, duration);
            StartCoroutine(coroutine);
        }

        IEnumerator lerpTo(Vector2 targetPosition, float duration)
        {
            float time = 0;
            Vector2 startPosition = transform.position;

            while (time < duration)
            {
                transform.position = Vector2.Lerp(startPosition, targetPosition, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            transform.position = targetPosition;
        }


        private void Awake()
        {
            Init();
        }

        /// <summary>
        /// Sets the zoom of the orth camera.
        /// 1 : height of view is 1. this means you can see exactly a 1m square from top to bottom.
        /// 10 : height of camera is 10. this means you can see exactly 10m from top to bottom
        /// </summary>
        public float Zoom
        {
            get
            {
                return Cam.orthographicSize * 2;
            }
            set
            {
                float height = 
                    Mathf.Clamp(value / 2, MinZoom, MaxZoom); //if zoom=1, then we actually want half-height = 0.5
                Cam.orthographicSize = height;
            }
        }
        
        /// <summary>
        /// Initilise camera with relevant params
        /// </summary>
        private void Init()
        {
            if (Cam == null)
            {
                Debug.LogError("no cam assigned to map camera presenter.");
                return;
            }
        }
    }
}