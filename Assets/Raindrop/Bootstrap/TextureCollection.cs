using UnityEngine;

namespace Raindrop.Bootstrap
{
    [CreateAssetMenu]
    public class TextureCollection : ScriptableObject
    {
        public Texture2D[] Textures;
     
        public int Count
        {
            get
            {
                if (Textures != null) return Textures.Length;
                return 0;
            }
        }
     
        public Texture2D Find( string searchFor, bool ignoreCase = true)
        {
            if (ignoreCase)
            {
                searchFor = searchFor.ToLower();
            }
               
            foreach( var m in Textures)
            {
                var n = m.name;
     
                if (ignoreCase)
                {
                    n = n.ToLower();
                }
     
                if (n == searchFor)
                {
                    return m;
                }
            }
            return null;
        
        }

        
        
        
    }
}