using System.Collections;
using NUnit.Framework;
using Plugins.CommonDependencies;
using Raindrop.Services.Bootstrap;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Raindrop.Tests.RaindropIntegrationTests.Helpers
{
    public static class SceneLoader
    {
        // load a empty scene with :
        // - bootstrapper (raindropinstance + servicelocator)
        // - mainthreaddispatcher object
        public static IEnumerator LoadHeadlessScene()
        {
            SceneManager.LoadScene("Tests/HeadlessBootstrapScene");
            yield return new WaitForFixedUpdate(); //testing out if this is a ok replacement to just witing an arbitrary amount of seconds.

            var bootstrap = GameObject.Find("Bootstrapper");
            Assert.True(bootstrap, "bootstrapper object not found");

            var rdbs = bootstrap.GetComponent<RaindropBootstrapper>();
            Assert.True(rdbs);

            var instance = ServiceLocator.Instance.Get<RaindropInstance>();
            Assert.True(instance != null);
        }
        
        public static void UnloadHeadlessScene()
        {
            SceneManager.UnloadSceneAsync("Tests/HeadlessBootstrapScene");

        }
    }
}