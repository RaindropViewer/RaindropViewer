using System;
using System.Collections;
using System.Collections.Generic;
using OpenMetaverse;
using Plugins.CommonDependencies;
using Raindrop.Map.Model;
using Raindrop.Utilities;
using UnityEngine;
using UnityEngine.Serialization;
using Logger = UnityEngine.Logger;

// a simple view to get a maptile from the map service.
[RequireComponent(typeof(TexturableMesh))]
public class FetchMapTile_ExternalAPI : MonoBehaviour
{
    public bool useExternalMap = false;
    
    //[Header("Fetch my maptile and display it, based on the location i am assigned to.")]
    public uint Grid_X => MapSpaceConverters.MapSpace2Grid_X(this.transform.position);//  1000;
    public uint Grid_Y => MapSpaceConverters.MapSpace2Grid_Y(this.transform.position);
    //public ulong handle => MapSpaceConverters.Vector32Handle(this.transform.position);

    private bool _needsTextureRefresh = false; 

    [FormerlySerializedAs("image")] public TexturableMesh unityTile;
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
        if (useExternalMap)
        {
            Fetch();
        }
        else
        {
            //this new way, the application of texture is done by the spawner already.
            //do nothing
        }
        
    }

    void Fetch()
    {
        MapTile = RetrieveMapTile(unityTile, Grid_X, Grid_Y);
        _needsTextureRefresh = true;
    }

    void Update()
    {
        //maptile Gameobject is ready to be textured?
        if (MapTile == null)
        {
            return;
        }
        
        //does maptile already have a texture?
        if (_needsTextureRefresh == false)
        {
            return;
        }

        //is tile already textured?
        // if (unityTile.isTextured)
        // {
        //     return; 
        // }

        //do texturing
        lock (MapTile)
        {
            //is the texture bytes ready?
            if (MapTile.isReady)
            {
                //apply the texture, finally!
                _needsTextureRefresh = false;
                unityTile.ApplyTexture(MapTile.getTex());
            }
        }
    }


    //retrieve tile from backend.
    private MapTile RetrieveMapTile(TexturableMesh texturableMesh, uint gridX, uint gridY)
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
