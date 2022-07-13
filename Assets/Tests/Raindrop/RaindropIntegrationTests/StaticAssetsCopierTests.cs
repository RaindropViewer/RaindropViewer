using System.Collections;
using System.IO;
using Disk;
using NUnit.Framework;
using OpenMetaverse;
using Raindrop.Bootstrap;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.Raindrop.RaindropIntegrationTests
{
    // test that the product copies the required static assets to the user
    // folder.
    public class StaticAssetsCopierTests
    {
        [UnityTest]
        public IEnumerator StaticAssets_MainSceneOnBoot_AreCopied()
        {
            // 1. delete any files in the local cache folder
            DeleteLocalCacheFiles();

            // 2. start main scene, which includes the startupCopier
            RaindropLoader.Load();

            //wait for abit...
            yield return new WaitForSeconds(10);
            
            string expectedToExist = Path.Combine(DirectoryHelpers.GetInternalStorageDir(),
                "grids.xml");
            
            Assert.True(File.Exists(expectedToExist),"file not exist: " + expectedToExist);
            
            // 3. don't forget to unload the scene!
            RaindropLoader.Unload();

            yield break;
        }

        public static void DeleteLocalCacheFiles()
        {
            System.IO.DirectoryInfo localCacheFolder
                = new DirectoryInfo(DirectoryHelpers.GetInternalStorageDir());
            foreach (FileInfo file in localCacheFolder.GetFiles())
            {
                try
                {
                    file.Delete();
                }
                catch
                {
                    OpenMetaverse.Logger.Log("delete failed: " +
                                             file.ToString(),
                        Helpers.LogLevel.Warning);
                }
            }

            foreach (DirectoryInfo dir in localCacheFolder.GetDirectories())
            {
                dir.Delete(true);
            }
        }
    }
}