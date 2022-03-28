using System;
using System.Threading;
using OpenMetaverse.Rendering;
using UnityEngine;

namespace Raindrop.Services.Bootstrap
{
    public class Globals : MonoBehaviour
    {
        public static Thread GMainThread;
        public Texture2D DefaultTexture;
        public static MeshmerizerR renderer { get; set; }
        public static string FullAppName { get; set; }
        public static string SimpleAppName { get; set; }


        public static bool isOnMainThread()
        {
            return Globals.GMainThread.Equals(System.Threading.Thread.CurrentThread);
        }

        public static void Init()
        {
            GMainThread = System.Threading.Thread.CurrentThread;
            
            InitDefaultTextures();

            void InitDefaultTextures()
            {
                //init void map's  # hex 1D475F
                Textures.DefaultMapVoidTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                Color color;
                ColorUtility.TryParseHtmlString("#1D475F", out color);
                Textures.DefaultMapVoidTexture.SetPixel(0, 0, color);
                Textures.DefaultMapVoidTexture.Apply();
            }
            
            
            renderer = new MeshmerizerR();

            FullAppName = Application.productName + " " + Application.version; 
            SimpleAppName = Application.productName;
        }

        public static class Textures
        {
            public static Texture2D DefaultMapVoidTexture { get; set; }
        }
    }

    public class DefaultTextures
    {
        public static Texture2D DefaultUnknownTexture;

        public DefaultTextures(Texture2D defaultTexture)
        {
            DefaultUnknownTexture = defaultTexture;
        }
        
        public Texture2D GetDefaultUnknownTexture()
        {
            return DefaultUnknownTexture;
        }
        
    }
}