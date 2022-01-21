using NUnit.Framework;
using UnityEngine;

namespace Raindrop.Tests.DiskAndCache
{
    public class DiskTests
    {
        // fixme: after implementing on-sd/on-device caching preferences
        [Test]
        public void printImportantPaths_Platforms()
        {
            Debug.Log("Application.persistentDataPath, usually the internal SD for android" + Application.persistentDataPath);
            // should be  /storage/emulated/0/Android/data/com.UnityTestRunner.UnityTestRunner/files/Pictures/
                
            Debug.Log("Application.dataPath, usually not usable for cache " + Application.dataPath);
            // Debug.Log("GetAndroidExternalFilesDir internal"+ Disk.DirectoryHelpers.GetAndroidExternalFilesDir(true));
            // should be  /storage/6106-8710/Android/data/com.UnityTestRunner.UnityTestRunner/files

            // Debug.Log("GetAndroidExternalFilesDir prefersdcard"+ Disk.DirectoryHelpers.GetAndroidExternalFilesDir(false));
            //should be  /storage/emulated/0/Android/data/com.UnityTestRunner.UnityTestRunner/files/Pictures/
                
        }    
    }
}