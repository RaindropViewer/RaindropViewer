using System.Collections;
using NUnit.Framework;
using Plugins.CommonDependencies;
using Raindrop.Map.Model;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Utils = OpenMetaverse.Utils;


namespace Raindrop.Tests.RaindropFullIntegrationTests
{
    //test the map service.
    public class MapServiceTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            //load the main scene.
            SceneManager.LoadScene("Raindrop/Bootstrap/BootstrapScene"); 
        }
        
        // test that the (external) map fetcher code is working.
        [UnityTest]
        public IEnumerator Test_MapService_SingleTile()
        {
            BeginMapFetcher();
            var fetcher = ServiceLocator.Instance.Get<MapService>();
            
            if (! fetcher.isReady)
                Assert.Fail("new version of map fetcher is not able to pass this test. Considering adding 2 types of fetcher methods; one for SL_external, one for CAPS/grid api ");

            //initally, the maptile is just a empty pocket
            //bool isReady;
            var emptyTile = fetcher.GetMapTile(Utils.UIntsToLong(1000*256,1000*256) , 1);
            Assert.False(emptyTile.isReady);
            
            // wait for the maptile to be filled by the network.
            yield return new WaitForSeconds(10);
            Assert.True(emptyTile.isReady, "tile should be ready after 10s.");
            //maptile should be ready in memory.
            Assert.True(emptyTile.getTex().width > 10);
            
            var secondRequestForSameTile = 
                fetcher.GetMapTile(Utils.UIntsToLong(1000*256,1000*256) , 1);
            Assert.True(emptyTile.isReady, "tile should be ready in subsequent fetch");

            Assert.Pass();
            
            void BeginMapFetcher()
            {
                if (!ServiceLocator.Instance.IsRegistered<MapService>())
                {
                    Debug.Log("UIBootstrapper creating and registering MapBackend.MapFetcher!");
                    ServiceLocator.Instance.Register<MapService>(new MapService());
                    //return;
                }
            }

        }

    }
}