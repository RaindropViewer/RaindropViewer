using System;
using Disk;
using NUnit.Framework;
using OpenMetaverse.Imaging;
using UnityEngine;

namespace Tests
{
    public class AsyncImageLoaderTests
    {
        [OneTimeSetUp]
        public void Setup()
        {
            BetterStreamingAssets.Initialize(); //fuck, this is easy to forget.
        }
        
        [Test]
        public void Simple_SaveImage()
        {
            //setup bitmap-T2d to save as jp2.
            string _inputImageSubPath = "test/menhara.jp2";
            byte[] thebytes = BetterStreamingAssets.ReadAllBytes(_inputImageSubPath);
            Texture2D texture 
                = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            T2D_JP2.LoadT2DWithoutMipMaps(thebytes, texture);

            //do encode to jp2.
            byte[] encodedBytes = Array.Empty<byte>(); 
            var loaderSettings = AsyncImageLoader.LoaderSettings.Default;
            loaderSettings.format = AsyncImageLoader.FreeImage.Format.FIF_JP2;
            var saveSuccess = AsyncImageLoader.SaveImage(texture, ref encodedBytes, loaderSettings);
            
            //write jp2 to disk
            string outputDir = Application.persistentDataPath + "/ImagingTests/Simple_SaveImage.jp2";
            DirectoryHelpers.WriteToFile(encodedBytes, outputDir);
            
            //write the original file to compare.
            //WARN: Blue is swapped with Red. in the call to AsyncImageLoader.SaveImage, the texture data is flipped r<->B for sending to Freeimage.
            // var res = texture.EncodeToJPG();
            // string outputDir2 = Application.persistentDataPath + "/ImagingTests/inverted_t2d.jpeg";
            // DirectoryHelpers.WriteToFile(res, outputDir2);
        }
    }
}