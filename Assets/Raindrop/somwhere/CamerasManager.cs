using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using OpenMetaverse;
using UnityEngine;

public class CamerasManager : MonoBehaviour
{
    #region Monobehavior Singleton stuff
    
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

    private Dictionary<CameraIdentifier.CameraType, Camera> cameras = 
        new Dictionary<CameraIdentifier.CameraType, Camera>();
    public Camera currentCam;
    public bool Ready { get; set; } = false;

    private void Start()
    {
        //get all cameras.
        foreach(Transform child in transform)
        {
            RegisterCamera(child.gameObject);
        }

        Ready = true;
    }

    private void RegisterCamera(GameObject Camera)
    {
        Camera cam = Camera.GetComponent<Camera>();
        var type = Camera.GetComponent<CameraIdentifier>();
        if (cam && type)
        {
            cameras.Add(type.type, cam);
            
            if (Camera.activeInHierarchy)
            {
                currentCam = cam;
            }
        }
    }

    public void ActivateCamera(CameraIdentifier.CameraType type)
    {
        if (!Ready)
            return;
        
        try
        {
            Camera cam = cameras[type];
            //1 deactive current
            currentCam.gameObject.SetActive(false);
            //2 active current
            currentCam = cam;
            cam.gameObject.SetActive(true);
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
