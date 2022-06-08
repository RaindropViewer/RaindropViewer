using System;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace OpenMetaverse.Imaging
{
    public class T2D_JP2
    {
        // Create a NEW T2D, by providing a bitmap in byte-array form.
        public static void LoadT2DWithoutMipMaps(byte[] thebytes, Texture2D texture)
        {
            // texture = new Texture2D(1, 1, TextureFormat.ARGB32, false); // impt there is a jobs bug here
            //also, new Texture2d can't be called outside the main thread!
            var loaderSettings = AsyncImageLoader.LoaderSettings.Default;
            loaderSettings.generateMipmap = false; // impt there is a bug here
                
            // Use hacky LoaderSettings
            var loadSuccess = AsyncImageLoader.LoadImage(texture, thebytes, loaderSettings);
                
            return;
        }

        //input: texture2d
        // output: bytes as jp2
        // creates memory allocation that is returned as jp2 array.
        public static void Convert_TextureBitmap_To_JP2Bytes_NoMipMap(Texture2D texture, ref byte[] jp2)
        {
            var loaderSettings = AsyncImageLoader.LoaderSettings.Default;
            loaderSettings.format = AsyncImageLoader.FreeImage.Format.FIF_JP2;
            var saveSuccess = AsyncImageLoader.SaveImage(texture, ref jp2, loaderSettings);
            return;
        }
        
        //input: color32[]
        // output: bytes as jp2
        // creates memory allocation that is returned as jp2 array.
        public static void Convert_ColorArray_To_JP2Bytes_NoMipMap(Color32[] texture, ref byte[] jp2)
        {
            var loaderSettings = AsyncImageLoader.LoaderSettings.Default;
            loaderSettings.format = AsyncImageLoader.FreeImage.Format.FIF_JP2;
            var saveSuccess = AsyncImageLoader.SaveImage(texture, ref jp2, loaderSettings);
            return;
        }
        
    }
}