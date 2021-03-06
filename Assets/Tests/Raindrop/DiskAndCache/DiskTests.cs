using System.Collections;
using System.Collections.Generic;
using System.IO;
using Disk;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.TestTools;
using UnityScripts.Disk;

namespace Raindrop.Tests.DiskAndCache
{
    public class DiskTests
    {
        [Test]
        public void ImportantPaths_Platforms_Print()
        {
            Debug.Log("Application.persistentDataPath, usually the internal SD for android"
                      + Application.persistentDataPath);
            // // should be  /storage/emulated/0/Android/data/com.UnityTestRunner.UnityTestRunner/files/Pictures/
            // var target = Path.Combine(Application.persistentDataPath, "ImportantPaths_Platforms_IsWritable.txt"); 
            // File.WriteAllLines(target, new List<string> {"success!"});


            Debug.Log("Application.dataPath, usually not usable for cache " + Application.dataPath);
            // // Debug.Log("GetAndroidExternalFilesDir internal"+ Disk.DirectoryHelpers.GetAndroidExternalFilesDir(true));
            // // should be  /storage/6106-8710/Android/data/com.UnityTestRunner.UnityTestRunner/files
            // target = Path.Combine(Application.dataPath, "ImportantPaths_Platforms_IsWritable.txt");
            // File.WriteAllLines(target, new List<string> {"success!"});
            // Assert.Fail();


            Debug.Log("GetInternalCacheDir " + Disk.DirectoryHelpers.GetInternalStorageDir());
            // //should be  /storage/emulated/0/Android/data/com.UnityTestRunner.UnityTestRunner/files/Pictures/
            // target = Path.Combine(DirectoryHelpers.GetInternalCacheDir(), "ImportantPaths_Platforms_IsWritable.txt");
            // File.WriteAllLines(target, new List<string> {"success!"});
        }

        [Test]
        public void InternalCachePath_Platforms_Writeable()
        {
            Debug.Log("GetInternalCacheDir " + Disk.DirectoryHelpers.GetInternalStorageDir());
            //should be  /storage/emulated/0/Android/data/com.UnityTestRunner.UnityTestRunner/files/Pictures/
            var target = Path.Combine(DirectoryHelpers.GetInternalStorageDir(), "ImportantPaths_Platforms_IsWritable.txt");
            File.WriteAllLines(target, new List<string> {"success!"});
        }

        [UnityTest]
        public IEnumerator StaticAssets_MonobehaviorOnInstantiate_MissingFile()
        {
            //1. delete grids.xml
            string GridsXmlFile = Path.Combine(DirectoryHelpers.GetInternalStorageDir(),
                "grids.xml");
            File.Delete(GridsXmlFile);
            Assert.False(File.Exists(GridsXmlFile),
                "delete grids.xml failed : " + GridsXmlFile);

            //2. do the startup copy.
            var copier = StaticFilesCopier.GetInstance();
            copier.Work();
            // var startupCopierObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            // var startupCopier = startupCopierObj.AddComponent<CopyStreamingAssetsToPersistentDataPath>();

            //wait for files to be copied...
            yield return new WaitForSeconds(3);
            Assert.True(copier.CopyIsDoneAndNoErrors);
            
            //3. grids.xml is expected to be copied
            Assert.True(File.Exists(GridsXmlFile),
                "failed to copy grids.xml from Streaming assets to this location: " + GridsXmlFile);
            
            yield break;
        }
        
        
        [UnityTest]
        public IEnumerator StaticAssets_MonobehaviorOnInstantiate_ChangedFile()
        {
            //1. change grids.xml
            string GridsXmlFile = Path.Combine(DirectoryHelpers.GetInternalStorageDir(),
                "grids.xml");
            File.WriteAllBytes(GridsXmlFile, new byte[]{0x01});

            //2. do the startup copy.
            var copier = StaticFilesCopier.GetInstance();
            copier.Work();
            // var startupCopierObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            // var startupCopier = startupCopierObj.AddComponent<CopyStreamingAssetsToPersistentDataPath>();

            //wait for files to be copied...
            yield return new WaitForSeconds(3);
            Assert.True(copier.CopyIsDoneAndNoErrors);
            
            //3. grids.xml is expected to be copied
            Assert.True(File.Exists(GridsXmlFile),
                "failed to copy grids.xml from Streaming assets to this location: " + GridsXmlFile);
            
            yield break;
        }
    }
}