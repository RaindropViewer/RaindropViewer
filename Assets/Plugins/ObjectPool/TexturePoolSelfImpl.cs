using System.Collections.Generic;
using Plugins.CommonDependencies;
using UnityEngine;

namespace Plugins.ObjectPool
{
    public class TexturePoolSelfImpl : IGameService
    {
        private static readonly Queue<Texture2D> PooledObjects = new Queue<Texture2D>();

        private static TextureFormat DefaultFormat = TextureFormat.RGBA32;
        private static bool DefaultMipMapOn = false;
        
        
        private static TexturePoolSelfImpl _instance;
        private TexturePoolSelfImpl(int defaultSize)
        {
            //fill up pool up to desired size.
            for (int i = 0; i < defaultSize; i++)
            {
                var tex = new Texture2D(256, 256, DefaultFormat, DefaultMipMapOn); 
                tex.hideFlags = HideFlags.HideAndDontSave; //this helps us delete this texture later?
                PooledObjects.Enqueue(tex);
            }
        }
        
        public static TexturePoolSelfImpl GetInstance()
        {
            if (_instance == null)
            {
                _instance = new TexturePoolSelfImpl(100);
            }
            return _instance;
        }

        public Texture2D GetFromPool(TextureFormat textureFormat)
        {
            //force 2 run in main thread, as when the texture needs to be constructed, it can only be done on main
            if (!UnityMainThreadDispatcher.isOnMainThread())
            {
                throw new WrongThreadException();
            }
            
            if (PooledObjects.Count != 0)
            {
                var fromPool = PooledObjects.Dequeue();
                // fromPool.Resize(256, 256, textureFormat, DefaultMipMapOn);
                return fromPool;
            }

            throw new PoolException("insufficient textures in pool");
            // var t2d = new Texture2D(256, 256, textureFormat, DefaultMipMapOn);
            // return t2d;
        }
        
        public void ReturnToPool(Texture2D texture)
        {
            PooledObjects.Enqueue(texture);
        }
    }
}