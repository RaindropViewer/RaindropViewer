using System;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace OpenMetaverse.Imaging
{
    public class T2D
    {
        // Create a NEW T2D, by providing a bitmap in byte-array form.
        public static Texture2D LoadT2DWithoutMipMaps(byte[] thebytes, Texture2D texture)
        {
            // texture = new Texture2D(1, 1, TextureFormat.ARGB32, false); // impt there is a jobs bug here
            //also, new Texture2d can't be called outside the main thread!
            var loaderSettings = AsyncImageLoader.LoaderSettings.Default;
            loaderSettings.generateMipmap = false; // impt there is a bug here
                
            // Use hacky LoaderSettings
            var loadSuccess = AsyncImageLoader.LoadImage(texture, thebytes, loaderSettings);
                
            return texture;
        }

    }
}