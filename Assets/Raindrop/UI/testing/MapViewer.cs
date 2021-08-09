using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Better.StreamingAssets;
using System;
using Raindrop;
using UnityEngine.UI;
using Raindrop.Presenters;
using OpenMetaverse;
using static Raindrop.MapDataPool;

// lets you view the maps by choosing parameters.
// use external API. does not need login.
public class MapViewer : MonoBehaviour
{
    
    [SerializeField]
    public MapLogic.MapFetcher mapFetcher;

    [SerializeField]
    public GameObject mapTileGO;
    private MapTileView iv;

    [SerializeField]
    public GameObject zoomSliderGO;
    private Slider zoomSlider;

    [SerializeField]
    public GameObject mapMoverGO;
    private MapMover mapMover;

    private float timeStart;
    private int imagesCount;
    private int currentFileIndex;

    bool needRepaint = false;

    System.Threading.Timer repaint;
    private void Awake()
    {
        //in main thread, before all uses.
        //BetterStreamingAssets.Initialize();
    } 
    

    private void Start()
    {
        InvokeRepeating("drawImage", 5f, 30f);

        //get reference to the view.
        iv = mapTileGO.GetComponent<MapTileView>();
        if (iv == null)
        {
            throw new System.Exception("Imageview is fucked"); // fix exception type plz
        }

        //get reference to the view.
        zoomSlider = zoomSliderGO.GetComponent<Slider>();
        if (zoomSlider == null)
        {
            throw new System.Exception("slider is fucked"); // fix exception type plz
        }

        //get reference to the view.
        mapMover = mapMoverGO.GetComponent<MapMover>();
        if (mapMover == null)
        {
            throw new System.Exception("mapMoverGO is fucked"); // fix exception type plz
        }

        mapFetcher = new MapLogic.MapFetcher();
    }

    //call this to redraw everything based on new params
    public void onRefresh()
    {
        ulong handle = mapMover.GetLookAt();
        int zoom = (int)zoomSlider.value;
        // how 2 convert look at floats into uints? (uints are gridpos * 256)
        mapFetcher.GetRegionTileExternal(handle, zoom);
        
        needRepaint = true;

        Debug.Log("refreshed internal images. fetching images if any..");
        return;
    }

    private void drawImage( )
    {
        onRefresh(); //hacky

        if (needRepaint)
        {
            needRepaint = false;
        } else
        {
            return;
        }
        ulong handle = mapMover.GetLookAt();
        MapTile tex = mapFetcher.GetMapTile(handle, 1);
        iv.setRawImage(tex.texture);
        Debug.Log("drew images.");
    }
}
