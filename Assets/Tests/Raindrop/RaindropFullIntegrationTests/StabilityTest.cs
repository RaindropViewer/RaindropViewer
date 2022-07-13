using System.Collections;
using NUnit.Framework;
using Raindrop.Bootstrap;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.Raindrop.RaindropFullIntegrationTests
{
    [TestFixture()]

    public class StabilityTest
    {
        // Check if we can load and unload game in the test environment,
        // consecutively.
        // Addresses github issue #10
        [UnityTest]
        public IEnumerator StabilityTest_Restart_RaindropApp()
        {
            int iterations = 3;
            for (int i = 0; i < iterations; i++)
            {
                Debug.Log($"scene iteration {i} start");
                Debug.Log("Load bootstrap scene: ");
                
                RaindropLoader.Load();
                
                yield return new WaitForSeconds(8);
                
                Debug.Log("unload entire scene: ");
                
                RaindropLoader.Unload();
                
                yield return new WaitForSeconds(2);
                Debug.Log($"scene iteration {i} done");
            }
        }
    }
}