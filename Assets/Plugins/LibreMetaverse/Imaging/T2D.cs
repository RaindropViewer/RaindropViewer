using UnityEngine;

namespace OpenMetaverse.Imaging
{
    public class T2D
    {
        public static Texture2D LoadT2DWithoutMipMaps(byte[] thebytes)
        {
            var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false); // impt there is a jobs bug here
            var loaderSettings = AsyncImageLoader.LoaderSettings.Default;
            loaderSettings.generateMipmap = false; // impt there is a bug here
                
            // Use hacky LoaderSettings
            var loadSuccess = AsyncImageLoader.LoadImage(texture, thebytes, loaderSettings);
                
            return texture;
        }

    }
}