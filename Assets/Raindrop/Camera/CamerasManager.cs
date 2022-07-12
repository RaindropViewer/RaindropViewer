using System;
using System.Collections.Generic;
using OpenMetaverse;
using UnityEngine;

namespace Raindrop.Camera
{
    public class CamerasManager : MonoBehaviour
    {
        #region Monobehavior Singleton
    
        private static CamerasManager _instance;
    
        public static CamerasManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogError("you forget to attach the singleton CamerasManager script.");
                }

                return _instance;
            } 
        }
    
        void Awake()
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    
        #endregion

        private Dictionary<CameraIdentifier.CameraType, UnityEngine.Camera> cameras = 
            new Dictionary<CameraIdentifier.CameraType, UnityEngine.Camera>();
        public CameraIdentifier.CameraType currentCam;
        //private CameraIdentifier.CameraType CurrentCamType;
        public bool Ready { get; set; } = false;

        private void Start()
        {
            //get all cameras.
            foreach(Transform child in transform)
            {
                RegisterCamera(child.gameObject);
            }

            //get ready for work.
            Ready = true;

            ActivateCamera(currentCam);
        }

        private void DeactivateAllCameras()
        {
            foreach(var cam in cameras)
            {
                cam.Value.enabled = false;
            }
        }

        private void RegisterCamera(GameObject Camera)
        {
            UnityEngine.Camera cam = Camera.GetComponent<UnityEngine.Camera>();
            var type = Camera.GetComponent<CameraIdentifier>();
            if (cam && type)
            {
                cameras.Add(type.type, cam);
            }
        }

        public void ActivateCamera(CameraIdentifier.CameraType type)
        {
            if (!Ready)
                return;
        
            try
            {
                currentCam = type;
                DeactivateAllCameras();
                cameras[currentCam].enabled = true;
                return;
            }
            catch (Exception e)
            {
                OpenMetaverse.Logger.Log("camera not available: " + type.ToString()
                    , Helpers.LogLevel.Error);
                return;
            }

            return;
        }

    }
}
