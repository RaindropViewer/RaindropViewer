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
    public class DownwardOrthoCameraView : MonoBehaviour
    {
        private const float minZoom = 0.1f;
        private const float maxZoom = 10f;
        public Camera camera;

        // Obtain the bottom left corner of the viewable region
        // unity units. -- you need to x256
        public UE.Vector2 min
        {
            get
            {
                var pos_x = centerX - halfHeightX;
                var pos_y = centerY - halfHeightY;

                return new UE.Vector2(pos_x, pos_y);
            }
        }

        // Obtain the top right corner of the viewable region
        // unity units. -- you need to x256
        public UE.Vector2 max
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
        public float centerX => this.transform.position.x;
        public float centerY => this.transform.position.y;
        private float halfHeightX => halfHeightY * camera.aspect;
        private float halfHeightY => camera.orthographicSize;

        /// <summary>
        /// Set the position of the camera based on the (grid) x,y coordinates -- eg: daboom is 1000,1000
        /// </summary>
        /// <param name="vec"></param>
        public void setToGridPos(UE.Vector2 vec)
        {
            this.transform.position = new UE.Vector3
                (vec.x, vec.y, transform.position.z);
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
            float h_height = Mathf.Clamp(zoom / 2, minZoom, maxZoom); //if zoom=1, then we actually want half-height = 0.5
            camera.orthographicSize = h_height;
        } 
        public float getZoom()
        {
            return camera.orthographicSize * 2;
        }
        
        // /// <returns></returns>
        // /// <summary>
        // /// Obtain the bottom left corner of the viewable region
        // /// In unity units. -- you need to x256
        // /// </summary>
        // /// <returns></returns>
        // public UE.Vector2 getMin()
        // {
        //     var pos_x = centerX - halfHeightX;
        //     var pos_y = centerY - halfHeightY;
        //
        //     min.Set(pos_x, pos_y);
        //     return min;
        // }
        //
        // /// <summary>
        // /// Obtain the top right corner of the viewable region
        // /// In unity units. -- you need to x256
        // /// </summary>
        // public UE.Vector2 getMax()
        // {
        //     var pos_x = centerX + halfHeightX;
        //     var pos_y = centerY + halfHeightY;
        //
        //     max.Set(pos_x, pos_y);
        //     return max;
        // }

        /// <summary>
        /// Get the half-height of the camera. 
        /// </summary>
        /// <returns></returns>
        // private float getVertRange()
        // {
        //     return camera.orthographicSize;
        // }

        /// <summary>
        /// Get the half-width of the ortho camera. 
        /// </summary>
        /// <returns></returns>
        // private float getHorzRange()
        // {
        //     return camera.orthographicSize * camera.aspect;
        // }


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
            if (camera == null)
            {
                Debug.LogError("no cam assigned to map camera presenter.");
                return;
            }
        }
    }
}