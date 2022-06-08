using NUnit.Framework;
using Raindrop.Render;
using Mesh = UnityEngine.Mesh;

namespace Raindrop.Tests.MeshingTests
{
    public class Cube
    {
        [Test]
        public void Meshing_SimpleCube()
        {
            Mesh mesh = new Mesh();
            UniMesher.Mesh_Cube(1, ref mesh);

            Assert.True(mesh.vertexCount == 4*6); //there are 4 verts per face. 6 faces.
            Assert.True(mesh.uv.Length == 4*6); //there are 1 uv per vert per face. thus total = 1*4*6
            Assert.True(mesh.subMeshCount == 1); //there are 1 uv per vert per face. thus total = 1*4*6
            
            Assert.Pass();
        }
    }
}

