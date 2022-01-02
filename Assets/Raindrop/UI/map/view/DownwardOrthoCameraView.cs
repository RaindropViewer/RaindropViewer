using OpenMetaverse;
using System;
using UnityEngine;
//using Vector2 = OpenMetaverse.Vector2;
using UE = UnityEngine;

namespace Raindrop.UI.Views
{
    /// <summary>
    /// Controls the camera in map view
    /// </summary>
    internal class DownwardOrthoCameraView : MonoBehaviour
    {
        private const float minZoom = 0.1f;
        private const float maxZoom = 10f;
        public GameObject cameraGO;
        private Camera camera;

        private UE.Vector2 min;
        private UE.Vector2 max;

        /// <summary>
        /// 2D axes
        /// </summary>
        private float centerX => this.transform.position.x;
        private float centerY => this.transform.position.z;

        /// <summary>
        /// Set the position of the camera based on the (grid) x,y coordinates -- eg: daboom is 1000,1000
        /// </summary>
        /// <param name="vec"></param>
        public void setToGridPos(UE.Vector2 vec)
        {
            this.transform.position = new UE.Vector3(vec.x, transform.position.y, vec.y);
        }

        private void Awake()
        {
            init();
        }



        /// <summary>
        /// Sets the zoom of the orth camera. a value of 1 means that the height of the viewing is 1. a value of 10 means the height of the viewing is 10.
        /// </summary>
        /// <param name="zoom"></param>
        public void setZoom(float zoom)
        {
            zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
            setVertHeight(zoom);
        }
        
        /// <returns></returns>
        /// <summary>
        /// Obtain the bottom left corner of the viewable region
        /// In unity units. -- you need to x256
        /// </summary>
        /// <returns></returns>
        public UE.Vector2 getMin()
        {
            var pos_x = centerX - getHorzRange();
            var pos_y = centerY - getVertRange();

            min.Set(pos_x, pos_y);
            return min;
        }

        /// <summary>
        /// Obtain the top right corner of the viewable region
        /// In unity units. -- you need to x256
        /// </summary>
        public UE.Vector2 getMax()
        {
            var pos_x = centerX + getHorzRange();
            var pos_y = centerY + getVertRange();

            max.Set(pos_x, pos_y);
            return max;
        }

        // set the vertical height of the camera as float.
        private void setVertHeight(float height)
        {
            camera.orthographicSize = height;
            return;
        }

        /// <summary>
        /// Get the half-height of the camera. 
        /// </summary>
        /// <returns></returns>
        private float getVertRange()
        {
            return camera.orthographicSize;
        }

        /// <summary>
        /// Get the half-width of the ortho camera. 
        /// </summary>
        /// <returns></returns>
        private float getHorzRange()
        {
            return camera.orthographicSize * camera.aspect;
        }


        ////reset to look at a sane location
        //internal void _resetCameraPos()
        //{
        //    moveToGridCoord(1000,1000); //Daboom?
        //}

        //private void moveToGridCoord(int gridX, int gridY)
        //{
        //    gridPositionX = gridX;  //north
        //    gridPositionY = gridY;  //east west
        //    _updateCameraPos(gridX, gridY);
        //}

        /// <summary>
        /// Initilise camera with relevant params
        /// </summary>
        internal void init()
        {
            if (cameraGO == null || !cameraGO.GetComponent<Camera>())
            {
                Debug.LogError("no cam assigned to map camera presenter.");
                //camera = this.gameObject.GetComponentInChildren<Camera>();
                return;
            }
            camera = cameraGO.GetComponent<Camera>();
             
        }
    }
}