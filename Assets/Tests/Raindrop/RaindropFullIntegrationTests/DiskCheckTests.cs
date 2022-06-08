using System.Collections;
using NUnit.Framework;
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
            SceneManager.LoadScene("Raindrop/Bootstrap/BootstrapScene"); 

            yield return new WaitForSeconds(3);

            Assert.True(StaticFilesCopier.GetInstance().CopyIsDoneAndNoErrors);
        }

        
        
    }
}