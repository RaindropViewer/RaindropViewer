using System.Collections;
using NUnit.Framework;
using Raindrop.Tests.RaindropIntegrationTests.Helpers;
using UnityEngine.TestTools;

namespace Raindrop.Tests.RaindropIntegrationTests.HeadlessScene
{
    //test that it is possible to construct the test scene.
    //the test scene contains no UI, but has a bootstrapped raindropinstance
    [TestFixture()]

    public class HeadlessSceneConstructionTests
    {
        [UnityTest]
        public IEnumerator SceneLoading_HeadlessBootstrapScene_IsSuccessful()
        {
            yield return SceneLoader.LoadHeadlessScene();
            yield break;
        }

    }
}