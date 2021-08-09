using OpenMetaverse;
using System;
using UnityEngine;
using Vector2 = OpenMetaverse.Vector2;

namespace Raindrop.Presenters
{
    //the camera no longer moves. however the zoom is still controlled here.
    //the map moves.
    internal class StationaryDownwardOrthoCameraPresenter : MonoBehaviour
    {
        public GameObject cameraGO;
        public Camera camera;

        private void Awake()
        {
            //viewableSize = camera.orthographicSize; //orthographicSize is half the size of the vertical viewing volume. 
            init();
        }

        // get the viewable range of the camera as x,y tuple.
        public OpenMetaverse.Vector2 getRange()
        {
            return new Vector2(getHorzRange(), getVertRange());
        }

        // get the vertical height of the camera as float.
        public float getVertHeight()
        {
            return camera.orthographicSize;
        }
        // set the vertical height of the camera as float.
        public void setVertHeight(float height)
        {
            camera.orthographicSize = height;
            return;
        }

        private float getVertRange()
        {
            return camera.orthographicSize * 2;
        }
        private float getHorzRange()
        {
            return camera.orthographicSize * 2 * camera.aspect;
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

        internal void init()
        {
            if (cameraGO == null || !cameraGO.GetComponent<Camera>())
            {
                Debug.LogError("no cam assigned to map camera presenter.");
                //camera = this.gameObject.GetComponentInChildren<Camera>();
                return;
            }
            camera = cameraGO.GetComponent<Camera>();

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