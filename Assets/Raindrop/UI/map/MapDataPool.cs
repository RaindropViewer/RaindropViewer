using System;
using System.Collections.Generic;
using UnityEngine;

namespace Raindrop
{
    //holds data for the map in memory.
    //the model.
    public class MapDataPool
    {
        public class MapTile
        {
            // it seems that a tile from the http is ~ 17KB in JPEG format; making it kind of spammable at the highest zoom level of 4.
            public int size; //in sims.
            public Texture2D texture;

            public int gridPosX;
            public int gridPosY;

            public static Texture2D emptyTexture = Texture2D.redTexture;

            public MapTile(int width, int height)
            {
                texture = emptyTexture;
                size = 0;
                gridPosX = 0;
                gridPosY = 0;
            }

            public void clearTex()
            {
                texture = emptyTexture;
                size = 0;
                gridPosX = 0;
                gridPosY = 0;
            }

            public Vector2Int regionMin => new Vector2Int(gridPosX, gridPosY);
            public Vector2Int regionMax => new Vector2Int(gridPosX + size - 1, gridPosY + size - 1); //inclusive
        }


        //tiles showing.
        public Dictionary<ulong, MapTile> regionTiles = new Dictionary<ulong, MapTile>();
        public int texCount => regionTiles.Count;

        // tiles that can be recycled.
        private List<MapTile> tilesToRecycle = new List<MapTile>();
        private Queue<MapTile> tilePool;
        //public int poolSize = 10;
        //List<ulong> tileRequests = new List<ulong>();

        public MapDataPool(int poolSize)
        {
            tilePool = new Queue<MapTile>(poolSize);
            for (int i = 0; i < poolSize; i++)
            {
                tilePool.Enqueue(new MapTile(256, 256));
            }

        }

        // gets a maptile from available pool.
        public MapTile getUnusedTex()
        {
            if (tilePool.Count == 0)
            {
                recycleTex();
                return tilePool.Dequeue();
            }
            else
            {
                return tilePool.Dequeue();
            }
        }

        // marks the texture/region for recycling.
        public void markTileToBeRecycled(MapTile tile)
        {
            tilesToRecycle.Add(tile);

        }

        // try to drop old textures and return them to the pool.
        private void recycleTex()
        {
            //foreach (var tile in tilesToRecycle)
            //{
            //    MapTile _tex;
            //    regionTiles.TryGetValue(reg, out _tex);
            //    tilePool.Enqueue(_tex);
                
            //}
        }
    }
}