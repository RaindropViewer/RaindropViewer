using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using OpenMetaverse.Imaging;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Raindrop.Tests.ImagingTests
{
    public class ImagingTests
    {
        // test if the j2p decode and encode can work.
        // test if able to write to disk.
        // have to manual check the images on the disk to see if the test passes.
        [TestFixture()]
        public class RaindropIntegrationTests
        {
            
            public static string _appDataDir = Application.persistentDataPath;
            public string outPath_persistentDataPath =  _appDataDir + "/Pictures/menhara_out_persistentDataPath.jpg";
            
            
            
            public string outFOlder_Pre =  _appDataDir + "/Pictures/";
            
            public static string _inputImageSubPath = "test/menhara.jp2";
            public static string _largeImageRelativePath = "test/largeimage.jp2";
            
            public string inFolder_Pre = "test/jp2/";
                // (pathToTGAFolder, "*.tga", SearchOption.AllDirectories);
                // public string testImgPath = BetterStreamingAssets + "\\test\\menhara.jp2";
                //
            // #else   
            //
            //     public string _appDataDir = Application.persistentDataPath;
            //
            //     public string testImgPath = _appDataDir + "\\Pictures\\menhara.jp2";
            //     public string outPath =  _appDataDir + "\\Pictures\\menhara_out.jpg";
            // #endif
            //
                
                
            //can read the image data from path.
            [Test]
            public void readBytes_ImagePath()
            {
                BetterStreamingAssets.Initialize(); //fuck, this is easy to forget.
                Assert.True(BetterStreamingAssets.FileExists(_inputImageSubPath), "SA file not exist : \n " + _inputImageSubPath);

                byte[] thebytes = BetterStreamingAssets.ReadAllBytes(_inputImageSubPath);

                Assert.True(thebytes.Length > 0, "File is empty size");
            }

            //can read and decode the jp2 from streamingassets. then save to local caching directory.
            [Test]
            public void readAndDecodeAndSave_ImagePath()
            {
                BetterStreamingAssets.Initialize(); //fuck, this is easy to forget.
                
                float timeStart = Time.realtimeSinceStartup;

                Assert.True(BetterStreamingAssets.FileExists(_inputImageSubPath));
                
                byte[] thebytes = BetterStreamingAssets.ReadAllBytes(_inputImageSubPath);
                Assert.True(thebytes.Length > 0);
                
                
                var texture = T2D.LoadT2DWithoutMipMaps(thebytes);

                Assert.True(texture.height > 5); //todo   so arbitrary
                
                var outbytes = texture.EncodeToJPG(100);
                #if UNITY_EDITOR
                
                    WriteToFile(outbytes, outPath_persistentDataPath);
                #elif UNITY_ANDROID //warn, this is true if we are in editor...
                    string outPath_GetAndroidExternalFilesDir_internal =  
                        GetAndroidExternalFilesDir(false) + "/Pictures/menhara_out_GetAndroidExternalFilesDir_internal.jpg";
                    string outPath_GetAndroidExternalFilesDir_external =  
                        GetAndroidExternalFilesDir(true) + "/Pictures/menhara_out_GetAndroidExternalFilesDir_external.jpg";
                    
                    WriteToFile(outbytes, outPath_GetAndroidExternalFilesDir_internal);
                    WriteToFile(outbytes, outPath_GetAndroidExternalFilesDir_external);
                    WriteToFile(outbytes, outPath_persistentDataPath);

                #endif



                float timeEnd = Time.realtimeSinceStartup;
                Debug.Log($"took time to read jp2, convert to jpg, and save : {(timeEnd - timeStart).ToString()} ");
                Debug.Log($"read : in StreamingAssets, -  {_inputImageSubPath} ");

            }



            [Test]
            [UnityPlatform (RuntimePlatform.Android)]
            public void printImportantPaths_Android()
            {
                //get from SA
                // var j2p_bytes = BetterStreamingAssets.ReadAllBytes(_largeImageRelativePath);
                //save to local disk.
                // File.WriteAllBytes(Application.persistentDataPath + "largeImage_out.j2p", j2p_bytes);
                // File.WriteAllBytes(Application.persistentDataPath + "largeImage_out.j2p", j2p_bytes);

                // List<string> outputs;
                // for (int i = 0; i < inputs.Length; i++)
                // {
                //     outputs.Add(string.Format(outFOlder_Pre, )  );
                // }
                //
                // RDS(input, output);

                Debug.Log("Application.persistentDataPath" + Application.persistentDataPath);
                // should be  /storage/emulated/0/Android/data/com.UnityTestRunner.UnityTestRunner/files/Pictures/
                
                Debug.Log("Application.sataPath" + Application.dataPath);
                Debug.Log("GetAndroidExternalFilesDir internal"+ GetAndroidExternalFilesDir(true));
                // should be  /storage/6106-8710/Android/data/com.UnityTestRunner.UnityTestRunner/files

                Debug.Log("GetAndroidExternalFilesDir prefersdcard"+ GetAndroidExternalFilesDir(false));
                //should be  /storage/emulated/0/Android/data/com.UnityTestRunner.UnityTestRunner/files/Pictures/
                
            }
            
            // returns SD card if the bool is true OR the sd card is not available.
            // otherwise i will return internal-but-shared storage
            private static string GetAndroidExternalFilesDir(bool preferSDcard)
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        // Get all available external file directories (emulated and sdCards)
                        AndroidJavaObject[] externalFilesDirectories = context.Call<AndroidJavaObject[],AndroidJavaObject[]>("getExternalFilesDirs", null);
                        AndroidJavaObject emulated = null;
                        AndroidJavaObject sdCard = null;

                        for (int i = 0; i < externalFilesDirectories.Length; i++)
                        {
                            AndroidJavaObject directory = externalFilesDirectories[i];
                            using (AndroidJavaClass environment = new AndroidJavaClass("android.os.Environment"))
                            {
                                // Check which one is the emulated and which the sdCard.
                                bool isRemovable = environment.CallStatic<bool> ("isExternalStorageRemovable", directory);
                                bool isEmulated = environment.CallStatic<bool> ("isExternalStorageEmulated", directory);
                                if (isEmulated)
                                    emulated = directory;
                                else if (isRemovable && isEmulated == false)
                                    sdCard = directory;
                            }
                        }
                        // Return the sdCard if available
                        if (sdCard != null && preferSDcard)
                            return sdCard.Call<string>("getAbsolutePath");
                        else
                            return emulated.Call<string>("getAbsolutePath");
                    }
                }
            }
        }
    }
}