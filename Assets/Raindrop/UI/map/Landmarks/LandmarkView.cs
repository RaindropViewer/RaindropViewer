using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using FMOD;
using OpenMetaverse;
using Raindrop.Services.Bootstrap;
using Raindrop.UI.views;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Logger = UnityEngine.Logger;
using Vector3 = OpenMetaverse.Vector3;

//MVC style of fetch and drawing the contents.
public class LandmarkView : MonoBehaviour
{
    private const string LoadingString = "Loading...";
    public ImageDisplayView imageView; //composite view :)
    public TMP_Text parcelName;
    public TMP_Text simName;
    public TMP_Text localCoords;
    public TMP_Text parcelDesc;

    private LandmarkController _controller;
    
    // Start is called before the first frame update
    void Start()
    {
        LandmarkController controller = new LandmarkController(this);
        if (imageView == null)
        {
            OpenMetaverse.Logger.Log("mandmarkview's image is not properly linked ", Helpers.LogLevel.Error);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // PollPresenterAndUpdate();
    }

    // access the modified bool in presenter, if it is true, we need to update ourselves.
    public void RenderView(Vector3 pos)
    {
        if (! Globals.isOnMainThread())
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                RenderView(pos);
            } );
        }
        
        RenderLocalPos(pos);
        
        // lock (_controller.mutex)
        // {
        //     if (_controller.data_modified == false)
        //     {
        //         return;
        //     }
        //
        //     //update ui.
        //     parcelName.text = _controller.parcelName;
        //     _controller.data_modified = false;
        // }
        
    }

    private void RenderLocalPos(Vector3 pos)
    {
        this.localCoords.text = pos.ToString();
    }

    // clear the text and image to nulls.
    public void InitialiseView()
    {
        parcelDesc.text = LoadingString;
        parcelName.text = LoadingString;
        simName.text = LoadingString;
        localCoords.text = LoadingString;
    }
}