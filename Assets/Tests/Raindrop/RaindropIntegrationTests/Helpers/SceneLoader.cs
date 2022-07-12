using System.Collections;
using NUnit.Framework;
using Plugins.CommonDependencies;
using Raindrop.Bootstrap;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Raindrop.Tests.RaindropIntegrationTests.Helpers
{
    public static class SceneLoader
    {
        // load a empty scene with :
        // - bootstrapper (raindropinstance + servicelocator)
        // - mainthreaddispatcher gameobject
        public static IEnumerator LoadHeadlessScene()
        {
            var loadSceneOperation = 
                SceneManager.LoadSceneAsync("Tests/HeadlessBootstrapScene");
            while (!loadSceneOperation.isDone)
            {
                yield return null;
            }

            var bootstrap = GameObject.Find("Bootstrapper");
            Assert.True(bootstrap, "bootstrapper object not found");
            var raindropBootstrapper = 
                bootstrap.GetComponent<RaindropBootstrapper>();
            Assert.True(raindropBootstrapper);

            var instance = ServiceLocator.Instance.Get<RaindropInstance>();
            Assert.True(instance != null);
        }
        
        public static void UnloadHeadlessScene()
        {
            SceneManager.UnloadSceneAsync("Tests/HeadlessBootstrapScene");
        }
    }
}