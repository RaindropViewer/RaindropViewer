using System.Collections;
using NUnit.Framework;
using OpenMetaverse;
using Raindrop.Map.Model;
using UnityEngine;
using UnityEngine.TestTools;

namespace Raindrop.Tests.RaindropTests
{
    //test the map service.
    public class MapServiceTests
    {
        
        // test that the (external) map fetcher code is working.
        [UnityTest]
        public IEnumerator Test_MapService_SingleTile()
        {
            Raindrop.Unity.SceneBootstrapperGenerator.Init();
            yield return new WaitForSeconds(2);
            Assert.True(ServiceLocator.ServiceLocator.Instance != null);
            Raindrop.Unity.SceneBootstrapperGenerator.AddMainThreadDispatcher();

            BeginMapFetcher();
            var fetcher = ServiceLocator.ServiceLocator.Instance.Get<MapService>();

            //initally, the maptile is just a empty pocket
            bool isReady;
            var emptyTile = fetcher.GetMapTile(Utils.UIntsToLong(1000*256,1000*256) , 1, out isReady);
            Assert.False(isReady);
            
            // wait for the maptile to be filled by the network.
            yield return new WaitForSeconds(10);
            Assert.True(emptyTile.isReady, "tile should be ready after 10s.");
            //maptile should be ready in memory.
            Assert.True(emptyTile.getTex().width > 10);
            
            var secondRequestForSameTile = 
                fetcher.GetMapTile(Utils.UIntsToLong(1000*256,1000*256) , 1, out isReady);
            Assert.True(emptyTile.isReady, "tile should be ready in subsequent fetch");

            Assert.Pass();
            
            void BeginMapFetcher()
            {
                if (!ServiceLocator.ServiceLocator.Instance.IsRegistered<MapService>())
                {
                    Debug.Log("UIBootstrapper creating and registering MapBackend.MapFetcher!");
                    ServiceLocator.ServiceLocator.Instance.Register<MapService>(new MapService());
                    //return;
                }
            }

        }

    }
}