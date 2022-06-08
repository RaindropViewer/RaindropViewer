using OpenMetaverse;
using Raindrop;
using Raindrop.Services;
using System.Collections;
using System.Collections.Generic;
using Plugins.CommonDependencies;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class RaindropMainCameraDrawDistance : MonoBehaviour
{
    public float DrawDistance;
    // private UIService ui => ServiceLocator.Instance.Get<UIService>();
    private RaindropInstance Instance => ServiceLocator.Instance.Get<RaindropInstance>();
    private GridClient Client => Instance.Client;

    //private bool Active => ui.ScreensManager.TopCanvas.canvasType == CanvasType.Game;

    // Start is called before the first frame update
    void Start()
    {
        if (!Instance.GlobalSettings.ContainsKey("draw_distance"))
        {
            Instance.GlobalSettings["draw_distance"] = DrawDistance;
        }

        this.GetComponent<Camera>().farClipPlane = DrawDistance;
    }

    void Update()
    {
        //if (!Active)
        //{
        //    return;
        //}

    }
}
