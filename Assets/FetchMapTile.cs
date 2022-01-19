using System.Collections;
using System.Collections.Generic;
using OpenMetaverse;
using Raindrop.Map.Model;
using Raindrop.ServiceLocator;
using UnityEngine;

public class FetchMapTile : MonoBehaviour
{
    public int Grid_X = 1000;
    public int Grid_Y = 1000;
    private bool modified = true;

    public TexturePlane image;

    // Update is called once per frame
    void Start()
    {
        SyncMapTile_blocking(image, Grid_X, Grid_Y);
        modified = false;
    }

    //blocks until tex is successfully printed.
    private void SyncMapTile_blocking(TexturePlane texturePlane, int gridX, int gridY)
    {
        MapFetcher mf = new MapFetcher();

        MapTile mt = mf.GetRegionTileExternal(Utils.UIntsToLong(1000,1000), 1);
        while (mt.isReady == false)
        {
            continue;
        }
            
        lock (mt.getTex())
        {
            texturePlane.ApplyTexture(mt.getTex());
        }

    }
}
