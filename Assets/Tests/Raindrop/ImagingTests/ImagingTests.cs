using Disk;
using NUnit.Framework;
using OpenMetaverse;
using OpenMetaverse.Imaging;
using Raindrop;
using UnityEngine;

namespace Tests.Raindrop
{
    [TestFixture()]
    public class ImagingTests
    {
        // test if the j2p decode and encode can work.
        // test if able to write to disk.
        // have to manual check the images on the disk to see if the test passes.
        
        public static string _inputImageSubPath = "test/menhara.jp2";

        #region Setup, teardown
        private static RaindropInstance instance;
        public static string clientDir;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            BetterStreamingAssets.Initialize(); //fuck, this is easy to forget.
            instance = new RaindropInstance(new GridClient());
            clientDir = instance.ClientDir;
        }
        
        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            instance.CleanUp();
        }
        #endregion
        
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
        public void ImagingTests_ReadJP2FromDisk_LoadIntoTexture2D_WriteToDiskAsJpeg()
        {
            float timeStart = Time.realtimeSinceStartup;

            Assert.True(BetterStreamingAssets.FileExists(_inputImageSubPath));
            
            byte[] thebytes = BetterStreamingAssets.ReadAllBytes(_inputImageSubPath);
            Assert.True(thebytes.Length > 0);


            Texture2D texture 
                = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            T2D_JP2.LoadT2DWithoutMipMaps(thebytes, texture);

            Assert.True(texture.height > 5); //todo   so arbitrary
            
            var outbytes = texture.EncodeToJPG(100);
            
            string appDataDir =  clientDir + "/ImagingTests/menhara_out_clientDir.jpg";
            DirectoryHelpers.WriteToFile(outbytes, appDataDir);
            #if UNITY_EDITOR
            #elif UNITY_ANDROID //warn, this is true if we are in editor...
                string outPath_GetAndroidExternalFilesDir_internal =  
                    Disk.DirectoryHelpers.GetAndroidExternalFilesDir(false) + "/ImagingTests/menhara_out_GetAndroidExternalFilesDir_internal.jpg";
                string outPath_GetAndroidExternalFilesDir_external =  
                    Disk.DirectoryHelpers.GetAndroidExternalFilesDir(true) + "/ImagingTests/menhara_out_GetAndroidExternalFilesDir_external.jpg";
                
                Disk.DirectoryHelpers.WriteToFile(outbytes, outPath_GetAndroidExternalFilesDir_internal);
                Disk.DirectoryHelpers.WriteToFile(outbytes, outPath_GetAndroidExternalFilesDir_external);
#endif

            float timeEnd = Time.realtimeSinceStartup;
            Debug.Log($"took time to read jp2, convert to jpg, and save : {(timeEnd - timeStart).ToString()} ");
            Debug.Log($"read : in StreamingAssets, -  {_inputImageSubPath} ");

        }
    }
}