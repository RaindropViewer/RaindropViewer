using Raindrop.Map.Model;
using System.Collections;
using Raindrop.ServiceLocator;
using UnityEngine;

namespace Raindrop.UI.Views
{
    //Generic gameobject spawner interface.
    public class SceneSpawner : MonoBehaviour, IGameService
    {
        public GameObject mapTilePrefab;

        public void SpawnOnMapPlane(MapTile tile)
        {

            MainThreadCall(tile);


        }





        public IEnumerator SpawnMapTileInMainThread(MapTile tile)
        {
            ulong handle = tile.getLoc();
            UnityEngine.Vector3 posInScene = Raindrop.Utilities.Coverters.Handle2Vector3(handle);

            var map = Instantiate(mapTilePrefab, posInScene, UnityEngine.Quaternion.identity);
            //map_collection.Add(handle, map);



            yield return null;
        }
        public void MainThreadCall(MapTile tile)
        {
            var _task = SpawnMapTileInMainThread(tile);

            UnityMainThreadDispatcher.Instance().Enqueue(_task);
        }


    }
}