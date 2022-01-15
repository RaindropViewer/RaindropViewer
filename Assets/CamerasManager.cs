using System;
using System.Collections;
using System.Collections.Generic;
using Gameframe.GUI.Utility;
using JetBrains.Annotations;
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

    private Dictionary<string, Camera> cameras = new Dictionary<string, Camera>();
    public Camera currentCam;

    private void Start()
    {
        //get all cameras.
        foreach(Transform child in transform)
        {
            RegisterCamera(child.gameObject);
        }
    }

    private void RegisterCamera(GameObject childGameObject)
    {
        Camera cam = childGameObject.GetComponent<Camera>();
        if (cam)
        {
            cameras.Add(childGameObject.name, cam);
        }
    }

    //warn: silent failure
    // return null if camera not found.
    [CanBeNull]
    public Camera ActivateCamera(string name)
    {
        Camera cam = cameras[name];
        if (cam)
        {
            //1 deactive current
            currentCam.gameObject.SetActive(false);
            //2 active current
            currentCam = cam;
            cam.gameObject.SetActive(true);
        }

        return cam;
    } 
}
