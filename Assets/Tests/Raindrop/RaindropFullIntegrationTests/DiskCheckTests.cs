using System.Collections;
using NUnit.Framework;
using Raindrop.Bootstrap;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityScripts.Disk;

namespace Raindrop.Tests.RaindropFullIntegrationTests
{
    public class DiskCheckTests
    {
        [UnityTest]
        public IEnumerator StaticCacheCopier_Test()
        {            
            RaindropLoader.Load();

            yield return new WaitForSeconds(3);

            Assert.True(StaticFilesCopier.GetInstance().CopyIsDoneAndNoErrors);
            
            RaindropLoader.Unload();
        }
    }
}