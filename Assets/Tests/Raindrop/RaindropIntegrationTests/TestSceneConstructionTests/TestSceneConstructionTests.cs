﻿using System.Collections;
using NUnit.Framework;
using Tests.Raindrop.RaindropIntegrationTests.Helpers;
using UnityEngine.TestTools;

namespace Tests.Raindrop.RaindropIntegrationTests.TestSceneConstructionTests
{
    //test that it is possible to construct the test scene.
    //the test scene contains no UI, but has a bootstrapped raindropinstance
    [TestFixture()]

    public class TestSceneConstructionTests
    {
        [UnityTest]
        public IEnumerator SceneLoading_Bootstrapper_IsSuccessful()
        {
            yield return SceneLoader.LoadHeadlessScene();

            yield break;
        }

    }
}