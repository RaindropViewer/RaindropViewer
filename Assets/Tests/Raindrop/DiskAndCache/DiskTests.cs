using System.Collections;
using System.Collections.Generic;
using System.IO;
using Disk;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.TestTools;

namespace Raindrop.Tests.DiskAndCache
{
    public class DiskTests
    {
        // fixme: after implementing on-sd/on-device caching preferences
        [Test]
        public void ImportantPaths_Platforms_IsWritable()
        {
            Debug.Log("Application.persistentDataPath, usually the internal SD for android" + Application.persistentDataPath);
            // should be  /storage/emulated/0/Android/data/com.UnityTestRunner.UnityTestRunner/files/Pictures/
            var target = Path.Combine(Application.persistentDataPath, "ImportantPaths_Platforms_IsWritable.txt"); 
            File.WriteAllLines(target, new List<string> {"success!"});
            
                
            Debug.Log("Application.dataPath, usually not usable for cache " + Application.dataPath);
            // Debug.Log("GetAndroidExternalFilesDir internal"+ Disk.DirectoryHelpers.GetAndroidExternalFilesDir(true));
            // should be  /storage/6106-8710/Android/data/com.UnityTestRunner.UnityTestRunner/files
            target = Path.Combine(Application.dataPath, "ImportantPaths_Platforms_IsWritable.txt");
            File.WriteAllLines(target, new List<string> {"success!"});
            Assert.Fail();


            Debug.Log("GetInternalCacheDir "+ Disk.DirectoryHelpers.GetInternalCacheDir());
            //should be  /storage/emulated/0/Android/data/com.UnityTestRunner.UnityTestRunner/files/Pictures/
            target = Path.Combine(DirectoryHelpers.GetInternalCacheDir(), "ImportantPaths_Platforms_IsWritable.txt");
            File.WriteAllLines(target, new List<string> {"success!"});
        }

        [UnityTest]
        public IEnumerator StaticAssets_MonobehaviorOnInstantiate_AreCopied()
        {
            string expectedToBeDeleted = Path.Combine(DirectoryHelpers.GetInternalCacheDir(),
                "grids.xml");
            Assert.False(File.Exists(expectedToBeDeleted),
                "please manually delete this folder to start the test : " + DirectoryHelpers.GetInternalCacheDir());

            
            var startupCopierObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var startupCopier = startupCopierObj.AddComponent<CopyStreamingAssetsToPersistentDataPath>();

            //wait for abit...
            yield return new WaitForSeconds(3);
            Assert.True(startupCopier.copyIsDone);
            
            string expectedToExist = Path.Combine(DirectoryHelpers.GetInternalCacheDir(),
                                     "grids.xml");
            
            Assert.True(File.Exists(expectedToExist),"file not exist: " + expectedToExist);
            
            yield break;
        }
    }
}