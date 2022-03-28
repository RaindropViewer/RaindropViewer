using System.Collections.Generic;
using Raindrop.ServiceLocator;
using UniRx;
using UniRx.Toolkit;
using UnityEngine;

namespace Raindrop
{
    public class TexturePool : IGameService
    {
        public int DefaultSize = 10;
        public static Queue<Texture2D> freeObjects = new Queue<Texture2D>();
        //public static List<Texture2D> usedObjects = new List<Texture2D>();
        public static bool shouldExpand = true;

        public static TextureFormat DefaultFormat = TextureFormat.ARGB32;
        public static bool DefaultMipMapOn = false;
        
        public TexturePool(int defaultSize)
        {
            DefaultSize = defaultSize;
            //fill up freeobjects up to default size.
            for (int i = 0; i < DefaultSize; i++)
            {
                freeObjects.Enqueue(
                    new Texture2D(256, 256, TextureFormat.ARGB32, DefaultMipMapOn));
            }
        }

        public static Texture2D Get(TextureFormat textureFormat)
        {
            if (freeObjects.Count != 0)
            {
                var fromPool = freeObjects.Dequeue();
                fromPool.Resize(256, 256, textureFormat, DefaultMipMapOn);
                //usedObjects.Add(fromPool);
                return fromPool;
            }

            if (shouldExpand)
            {
                var t2d = new Texture2D(256, 256, textureFormat, DefaultMipMapOn);
                //usedObjects.Add(t2d);
                return t2d;
            }
            else
            {
                return null;
            }
        }


        public static void ReturnToPool(Texture2D texture)
        {
            freeObjects.Enqueue(texture);
        }

    }
}