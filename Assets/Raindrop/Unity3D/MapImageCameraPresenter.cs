using OpenMetaverse;
using System;
using UnityEngine;
using Vector2 = OpenMetaverse.Vector2;

namespace Raindrop.Presenters
{
    //this just moves the camera around.
    internal class MapImageCameraPresenter : MonoBehaviour
    {
        //public MapImageManager mapmanager;
        public Camera camera;
        public GameObject cameraGO;

        internal int gridPositionX; //grid index
        internal int gridPositionY;
        internal int gridOffsetX; //inside grid
        internal int gridOffsetY;

        private int cameraHeight = 10;

        //these coordinate differences are quite condfusing.
        private void _updateCameraPos(int gridX , int gridY)
        {
            int north_south = gridX;
            int east_west = gridY;
            var uepos = new UnityEngine.Vector3(east_west, cameraHeight,north_south);
            camera.transform.position = uepos;
        }

        //sets the camera to look at this particular simulator texture.
        public void setPos(int gridX, int gridY)
        {
            _updateCameraPos(gridX,gridY);

        }


        //reset to look at a sane location
        internal void _resetCameraPos()
        {
            moveToGridCoord(1000,1000); //Daboom?
        }

        private void moveToGridCoord(int gridX, int gridY)
        {
            gridPositionX = gridX;  //north
            gridPositionY = gridY;  //east west
            _updateCameraPos(gridX, gridY);
        }

        internal void init()
        {
            if (camera == null)
            {
                camera = this.gameObject.GetComponentInChildren<Camera>();
                cameraGO = camera.gameObject;

            }

            //cameraGO = new GameObject();
            //camera = cameraGO.AddComponent<Camera>();

            //camera.depth = 5;
            //camera = cameraGO.GetComponent<Camera>();
            //camera.orthographic = true;
            //camera.clearFlags = CameraClearFlags.SolidColor;
            //camera.backgroundColor = Color.black;
            //camera.cullingMask = LayerMask.NameToLayer("Minimap"); //only render minimap layer.
            //oriantetaion
            camera.transform.position = new UnityEngine.Vector3(0, cameraHeight, 0);
            camera.transform.forward = UnityEngine.Vector3.down;
            _resetCameraPos();

            camera.transform.SetParent(this.transform);


            //make the camera from prefab.
            //sceneCamera = UnityEngine.Object.Instantiate(cameraPrefab, new Vector3(0, 0, 0), Quaternion.identity);

            //make ortho camera 

        }
    }
}