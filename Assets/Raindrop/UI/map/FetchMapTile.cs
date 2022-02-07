using System;
using System.Collections;
using System.Collections.Generic;
using OpenMetaverse;
using Raindrop.Map.Model;
using Raindrop.ServiceLocator;
using Raindrop.Utilities;
using UnityEngine;
using UnityEngine.Serialization;
using Logger = UnityEngine.Logger;

// a simple view to get a maptile from the map service.
[RequireComponent(typeof(Texturable))]
public class FetchMapTile : MonoBehaviour
{
    //[Header("Fetch my maptile and display it, based on the location i am assigned to.")]
    public uint Grid_X => MapSpaceConverters.Vector32_GridX(this.transform.position);//  1000;
    public uint Grid_Y => MapSpaceConverters.Vector32_GridY(this.transform.position);
    //public ulong handle => MapSpaceConverters.Vector32Handle(this.transform.position);
    
    private bool _needsTextureRefresh = false; 

    [FormerlySerializedAs("image")] public Texturable unityTile;
    public MapTile MapTile;
    
    // Update is called once per frame
    void Start()
    {
        //MapTile = RetrieveMapTile_blocking(unityTile, Grid_X, Grid_Y);
        // _needsTextureRefresh = true;
    }

    //the spawner will enable the tile when it is set in the right position.
    private void OnEnable()
    {
        Fetch();
    }

    void Fetch()
    {
        MapTile = RetrieveMapTile_blocking(unityTile, Grid_X, Grid_Y);
        _needsTextureRefresh = true;
    }

    void Update()
    {
        if (_needsTextureRefresh == false)
        {
            return;
        }

        lock (MapTile)
        {
            if (MapTile.isReady)
            {
                //apply the texture, finally!
                _needsTextureRefresh = false;
                unityTile.ApplyTexture(MapTile.getTex());
            }
        }
    }

    //blocks until tex is successfully printed.
    private MapTile RetrieveMapTile_blocking(Texturable texturable, uint gridX, uint gridY)
    {
        try
        {
            MapService mapService = ServiceLocator.Instance.Get<MapService>();
            //bool isReady;
            MapTile mt = mapService.GetMapTile(Utils.UIntsToLong(gridX * 256, gridY * 256), 1);
            return mt;
        }
        catch
        {
            OpenMetaverse.Logger.Log
                ("initisalision sequence error with map fetcher ", Helpers.LogLevel.Error);
        }
        return null;
    }
}
