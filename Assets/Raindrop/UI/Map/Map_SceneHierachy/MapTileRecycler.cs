using System.Collections.Generic;
using UnityEngine;

namespace Raindrop.UI.map.Map_SceneHierachy
{
    public class MapTileRecycler
    {
        public int DefaultSize = 10;
        public List<GameObject> pooledObjects = new List<GameObject>();
        public GameObject objectToPool;
        [SerializeField] public bool shouldExpand = true;

        
        public MapTileRecycler(int defaultSize)
        {
            DefaultSize = defaultSize;
        }

        public GameObject GetPooledObject() {
            for (int i = 0; i < pooledObjects.Count; i++) {
                if (!pooledObjects[i].activeInHierarchy) {
                    return pooledObjects[i];
                }
            }
            
            // "lazy instantiation" is over here :)
            if (shouldExpand) {
                GameObject obj = (GameObject)MonoBehaviour.Instantiate(objectToPool);
                obj.SetActive(false);
                pooledObjects.Add(obj);
                return obj;
            } else {
                return null;
            }
        }

        public void ReturnToPool(GameObject gameObject)
        {
            gameObject.SetActive(false);
        }

    }
}