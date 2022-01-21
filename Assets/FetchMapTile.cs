using System.Collections;
using System.Collections.Generic;
using OpenMetaverse;
using Raindrop.Map.Model;
using Raindrop.ServiceLocator;
using UnityEngine;

// a simple view to get a maptile from the map service.
public class FetchMapTile : MonoBehaviour
{
    public int Grid_X = 1000;
    public int Grid_Y = 1000;
    private bool _needsTextureRefresh = true;

    public TexturePlane image;
    public MapTile MapTile;
    
    // Update is called once per frame
    void Start()
    {
        MapTile = RetrieveMapTile_blocking(image, Grid_X, Grid_Y);
        _needsTextureRefresh = true;
    }

    // todo: refactor into a routine
    void update()
    {
        if (_needsTextureRefresh == false)
        {
            return;
        }

        lock (MapTile)
        {
            if (MapTile.isReady)
            {
                image.ApplyTexture(MapTile.getTex());
            }
        }
    }

    //blocks until tex is successfully printed.
    private MapTile RetrieveMapTile_blocking(TexturePlane texturePlane, int gridX, int gridY)
    {
        MapService mapService = new MapService();

        bool isReady;
        MapTile mt = mapService.GetMapTile(Utils.UIntsToLong(1000,1000), 1, out isReady);
        return mt;
        // while (mt.isReady == false)
        // {
        //     continue;
        // }
        //     
        // lock (mt.getTex())
        // {
        //     texturePlane.ApplyTexture(mt.getTex());
        // }

    }
}
