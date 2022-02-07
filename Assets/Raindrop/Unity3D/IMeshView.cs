using UnityEngine;

namespace Raindrop.Unity3D
{
    // a universal interface to allow us to:
    // 2. set mesh
    // 3. set texture
    
    public interface IMeshView
    {
        // set texture to object
        public void SetTexture(Texture2D texture);
        
        // set mesh to object
        public void SetMesh(Mesh mesh);
    }
}