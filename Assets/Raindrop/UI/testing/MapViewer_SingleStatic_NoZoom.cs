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
using Raindrop.Map;
using Raindrop.Map.Model;
using Raindrop.UI.Views;

// lets you view the maps by choosing parameters.
// use external API. does not need login.
public class MapViewer_SingleStatic_NoZoom : MonoBehaviour
{
    
    [SerializeField]
    public MapBackend mapFetcher;

    [SerializeField]
    public GameObject mapTileGO;
    private MapTileView iv;

    [SerializeField]
    public GameObject zoomSliderGO;
    private Slider zoomSlider;

    [SerializeField]
    public GameObject mapMoverGO;
    private MapLookAt mapMover;

    private float timeStart;
    private int imagesCount;
    private int currentFileIndex;

    bool needRepaint = false;

    System.Threading.Timer repaint;
    private void Awake()
    {
        mapFetcher = new MapBackend();

    }

    private void Update()
    {
        //redrawMap();
    }


    private void Start()
    {
        InvokeRepeating("redrawMap", 5f, 5f);

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
            Debug.LogWarning("slider is fucked"); // fix exception type plz
        }

        //get reference to the view.
        mapMover = mapMoverGO.GetComponent<MapLookAt>();
        if (mapMover == null)
        {
            throw new System.Exception("mapMoverGO is fucked"); // fix exception type plz
        }

        mapFetcher = new MapBackend();


        // for testing
        onRefresh();
    }

    /// <summary>
    /// Retrieves desired tile from backend.
    /// </summary>
    public void onRefresh()
    {
        ulong handle = mapMover.GetLookAt();
        int zoom = (int)zoomSlider.value;
        // how 2 convert look at floats into uints? (uints are gridpos * 256)
        if (mapFetcher.tryGetMapTile(handle, 1) == null) //hack for now.
        {
            mapFetcher.GetRegionTileExternal(handle, 1);
        } 
        
        needRepaint = true;

        Debug.Log("refreshed internal images. fetching images if any..");
        return;
    }

    /// <summary>
    /// Redraws the tiles if the redraw flag is true.
    /// </summary>
    private void redrawMap( )
    {
        //onRefresh(); //hacky

        if (needRepaint)
        {
            needRepaint = false;
        } else
        {
            return;
        }
        ulong handle = mapMover.GetLookAt();
        MapTile tex = mapFetcher.tryGetMapTile(handle, 1);

        if (tex != null) 
        {
            iv.setRawImage(tex.getTex());
        }
        Debug.Log("drew images.");
    }
}
