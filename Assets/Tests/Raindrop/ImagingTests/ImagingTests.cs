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
        private static RaindropInstance instance;
        public static string clientDir;
        public static string _inputImageSubPath = "test/menhara.jp2";

        [OneTimeSetUp]
        public void setup()
        {
            BetterStreamingAssets.Initialize(); //fuck, this is easy to forget.
            instance = new RaindropInstance(new GridClient());
            clientDir = instance.ClientDir;
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

        
        
                
            // [UnityTest]
            // // managedimage -> texture2d -> managed image test
            // /* 1. create managedimage 2x2 red-blue-green- black image by code.
            //  * 2. call convert to texture2d.
            //  * 3. print to screen.
            //  * 4. convert back to managed image.
            //  * 5. print to disk.
            //  */ //weird ass test
            // public IEnumerator ManagedImage_Texture2D_conversions()
            // {
            //     //1 load the image using unity's texture 2d (known to be correct.).
            //     var tex = new Texture2D(1024,1024);
            //     var b = File.ReadAllBytes("C:\\Users\\Alexis\\Pictures\\menhara.jpg");
            //     tex.LoadImage(b);
            //         // OpenMetaverse.Imaging.LoadTGAClass.LoadTGA(
            //         // "C:\\Users\\Alexis\\Pictures\\menhara.tga");
            //         
            //         
            //     
            //     //1b. check image loading integrity.
            //     // var bytesa = tex.EncodeToJPG(100);
            //     // System.IO.File.WriteAllBytes(Path.Combine(Application.persistentDataPath, "menhara_from_t2d.jpg"), bytesa);
            //     // mi.Blue[2] = 0xFF; 
            //     // mi.Red[0] = 0xFF; 
            //     // mi.Green[1] = 0xFF;
            //     // // mi.Blue = new byte[]{0x00,0x00,0xFF,0x00}; 
            //     // mi.Red = new byte[]{0xFF,0x00,0x00,0x00}; 
            //     // mi.Green = new byte[]{0x00,0xFF,0x00,0x00}; 
            //     
            //     //2 print mi to disk in some easy to read format.
            //     // error! this image is written to disk upside down!.
            //     ManagedImage mi = new ManagedImage(tex);
            //     var tgaBytes = mi.ExportTGA();
            //     Debug.Log("writing to "+ Path.Combine(Application.persistentDataPath, "exportTGA_managed_image1.tga").ToString());
            //     System.IO.File.WriteAllBytes(Path.Combine(Application.persistentDataPath, "exportTGA_managed_image1.tga"), tgaBytes);
            //
            //     //3. convert to t2d and show on screen.
            //     Texture2D t2d = mi.ExportTex2D();
            //     // plane.make
            //     // plane.show(t2d)
            //     
            //     var bytes = t2d.EncodeToJPG(100);
            //     System.IO.File.WriteAllBytes(Path.Combine(Application.persistentDataPath, "EncodeToJPG_Texture2d.jpg"), bytes);
            //
            //     yield return new WaitForSeconds(1);
            //     
            //     //4. convert back to tga, save it as a 2nd file.
            //     ManagedImage mi2 = new ManagedImage(t2d);
            //     // error:this method writes a up-side down image.
            //     var tgaBytes2 = mi2.ExportTGA();
            //     System.IO.File.WriteAllBytes(Path.Combine(Application.persistentDataPath, "exportTGA_managedimage2.tga"), tgaBytes2);
            //     
            //     Assert.Pass();
            //     yield break;
            // }

        }
    }
}