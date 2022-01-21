using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using OpenMetaverse;
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
            private static RaindropInstance instance = new RaindropInstance(new GridClient());
            public static string clientDir = instance.ClientDir;
            public string appDataDir =  clientDir + "/ImagingTests/menhara_out_persistentDataPath.jpg";
            public static string _inputImageSubPath = "test/menhara.jp2";

            [OneTimeSetUp]
            public void setup()
            {
                BetterStreamingAssets.Initialize(); //fuck, this is easy to forget.
            }
            
            //can read the image data from path.
            [Test]
            public void readBytes_ImagePath()
            {
                Assert.True(BetterStreamingAssets.FileExists(_inputImageSubPath), "SA file not exist : \n " + _inputImageSubPath);

                byte[] thebytes = BetterStreamingAssets.ReadAllBytes(_inputImageSubPath);

                Assert.True(thebytes.Length > 0, "File is empty size");
            }

            //can read and decode the jp2 from streamingassets. then save to local caching directory.
            [Test]
            public void readAndDecodeAndSave_ImagePath()
            {
                float timeStart = Time.realtimeSinceStartup;

                Assert.True(BetterStreamingAssets.FileExists(_inputImageSubPath));
                
                byte[] thebytes = BetterStreamingAssets.ReadAllBytes(_inputImageSubPath);
                Assert.True(thebytes.Length > 0);


                Texture2D texture 
                    = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                T2D.LoadT2DWithoutMipMaps(thebytes, texture);

                Assert.True(texture.height > 5); //todo   so arbitrary
                
                var outbytes = texture.EncodeToJPG(100);
                
                Helper.WriteToFile(outbytes, appDataDir);
                #if UNITY_EDITOR
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
        }
    }
}