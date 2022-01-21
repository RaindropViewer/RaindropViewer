using System;
using System.Collections.Generic;

namespace Raindrop.Map.Model
{
    //pooling pattern for objects -- holds UNUSED map in memory.
    public class MapTilesPool
    {
        private Queue<MapTile> tilePool;

        public int Count => tilePool.Count;


        public MapTilesPool(int poolSize)
        {
            tilePool = new Queue<MapTile>(poolSize);
            for (int i = 0; i < poolSize; i++)
            {
                tilePool.Enqueue(new MapTile(256, 256)); //TODO: tile factory.
            }
        }

        // gets a unused maptile from pool.
        public MapTile acquireTile()
        {
            if (Count <= 0)
            {
                throw new Exception("ran out of memory in mapdatapool! Please implement data recycling logic.");
            }
            else
            {
                return tilePool.Dequeue();
            }
        }

        // returns a tile to the pool.
        public void releaseTile(MapTile tile)
        {
            tile.clearTex();
            tilePool.Enqueue(tile);
        }
    }
}