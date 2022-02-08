using System.Collections;
using System.IO;
using Disk;
using NUnit.Framework;
using OpenMetaverse;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Raindrop.Tests.Raindrop.RaindropIntegrationTests
{
    //test that the static copier works in the product.
    public class StaticAssetsCopierTests
    {
        
        [UnityTest]
        public IEnumerator StaticAssets_MainSceneOnBoot_AreCopied()
        {
            // dont want to do this kind of deletion in my test anymore.
            // 1. delete any files in the local cache folder
            // System.IO.DirectoryInfo localCacheFolder 
            //     = new DirectoryInfo(DirectoryHelpers.GetInternalCacheDir());
            // foreach (FileInfo file in localCacheFolder.GetFiles())
            // {
            //     try
            //     {
            //         file.Delete();
            //     }
            //     catch
            //     {
            //         OpenMetaverse.Logger.Log("delete failed: " + 
            //                    file.ToString(),
            //             Helpers.LogLevel.Warning);
            //     }
            // }
            // foreach (DirectoryInfo dir in localCacheFolder.GetDirectories())
            // {
            //     dir.Delete(true); 
            // }
            
            // 2. start main scene, which includes the startupCopier
            SceneManager.LoadScene("Scenes/MainScene");
            // var startupCopierObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            // var startupCopier = startupCopierObj.AddComponent<CopyStreamingAssetsToPersistentDataPath>();

            //wait for abit...
            yield return new WaitForSeconds(10);
            // Assert.True(startupCopier.copyIsDone);
            
            string expectedToExist = Path.Combine(DirectoryHelpers.GetInternalCacheDir(),
                "grids.xml");
            
            Assert.True(File.Exists(expectedToExist),"file not exist: " + expectedToExist);
            
            yield break;
        }
    }
}