using OpenMetaverse;
using System;
using UnityEngine;
//using Vector2 = OpenMetaverse.Vector2;
using UE = UnityEngine;

namespace Raindrop.UI.Views
{
    /// <summary>
    /// Controls the camera
    /// </summary>
    internal class DownwardOrthoCameraView : MonoBehaviour
    {
        public GameObject cameraGO;
        private Camera camera;

        //map mover. Contains focal point.
        [SerializeField]
        public GameObject MapLookAtGO;
        private MapLookAt mapLookAt;

        private UE.Vector2 min;
        private UE.Vector2 max;

        /// <summary>
        /// 2D axes
        /// </summary>
        private float centerX => this.transform.position.x;
        private float centerY => this.transform.position.z;


        private void Awake()
        {
            //viewableSize = camera.orthographicSize; //orthographicSize is half the size of the vertical viewing volume. 
            init();
            //mapLookAt = mapLookAt.GetComponent<MapLookAt>();
        }

        private void Update()
        {

        }

        /// <summary>
        /// set the look at, parameters of the camera.
        /// </summary>
        /// <param name="LookAt"></param>
        public void init(GameObject LookAt)
        {
            this.MapLookAtGO = LookAt;
        }

        /// <summary>
        /// Sets the zoom of the orth camera. a value of 1 means that the height of the viewing is 1. a value of 10 means the height of the viewing is 10.
        /// </summary>
        /// <param name="zoom"></param>
        public void setZoom(float zoom)
        {
            var maxZoom = 10;
            zoom = Mathf.Clamp(zoom, 1, maxZoom);
            setVertHeight(zoom);
        }

        /// <summary>
        /// get the viewable range (height and width) of the camera as x,y tuple. Units are in orthographic units.
        /// </summary>
        /// <returns></returns>
        //public OpenMetaverse.Vector2 getRange()
        //{
        //    return new Vector2(getHorzRange(), getVertRange());
        //}

        

        /// <returns></returns>
        /// <summary>
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

        ////these coordinate differences are quite condfusing.
        //// update cam to look at specified grid's location 
        //private void _updateCameraPos(int gridX , int gridY)
        //{
        //    int north_south = gridX;
        //    int east_west = gridY;
        //    var uepos = new UnityEngine.Vector3(east_west, cameraHeight,north_south);
        //    camera.transform.position = uepos;
        //}

        ////sets the camera to look at this particular simulator texture.
        //public void setPos(int gridX, int gridY)
        //{
        //    _updateCameraPos(gridX,gridY);

        //}


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

            //if (isDownward)
            //{
            //    cameraGO.transform.LookAt(transform.position + UnityEngine.Vector3.down);
            //}

            //if (isOrtho)
            //{
            //    camera.orthographic = true;
            //}

            //cameraGO = new GameObject();
            //camera = cameraGO.AddComponent<Camera>();

            //camera.depth = 5;
            //camera = cameraGO.GetComponent<Camera>();
            //camera.orthographic = true;
            //camera.clearFlags = CameraClearFlags.SolidColor;
            //camera.backgroundColor = Color.black;
            //camera.cullingMask = LayerMask.NameToLayer("Minimap"); //only render minimap layer.
            //oriantetaion
            //camera.transform.position = new UnityEngine.Vector3(0, cameraHeight, 0);
            //camera.transform.forward = UnityEngine.Vector3.down;
            //_resetCameraPos();

            //camera.transform.SetParent(this.transform);


            //make the camera from prefab.
            //sceneCamera = UnityEngine.Object.Instantiate(cameraPrefab, new Vector3(0, 0, 0), Quaternion.identity);

            //make ortho camera 

        }
    }
}