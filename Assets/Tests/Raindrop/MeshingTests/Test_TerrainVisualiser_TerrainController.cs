using Raindrop.Unity3D;
using UnityEngine;
using Mesh = UnityEngine.Mesh;

//help us see that the terrain mesher works.
namespace Raindrop.Tests.MeshingTests
{
    public class Test_TerrainVisualiser_TerrainController : MonoBehaviour
    {
        #region params
        // heightmap of this sim. should be 256^2?
        // public int HeightmapX = 256;
        // public int HeightmapY = 256;
        public int TerrainSize = 256;

        // private int HeightmapX_prev = 256;
        // private int HeightmapY_prev = 256;
        private int TerrainSize_prev = 256;
        // private float noiseFactor_prev = 10f;
        // private float noisePeriod_prev = 10f;
    
        private bool needsupdate = true;
        private bool isBusyWorking = false;

        public MeshFilter MeshFilter;
        public Mesh generatedMesh;

        #endregion

        public TerrainMeshController component_under_test;
    
        private void Awake()
        {
            generatedMesh = new Mesh();
            if (MeshFilter is null)
            {
                return;
            }
            MeshFilter.mesh = generatedMesh;
        
        }

        void Update()
        {
            if (isBusyWorking)
            {
                return;
            }

            bool paramsChanged = (TerrainSize_prev != TerrainSize);
            if (paramsChanged)
            {
                needsupdate = true;
                TerrainSize_prev = TerrainSize;
            }
        
            if (needsupdate)
            {
                needsupdate = false;
                component_under_test.Modified = true;

                // isBusyWorking = true;
                // PerformMeshing(HeightmapX,HeightmapY, TerrainSize, noiseAmplitude);
                // isBusyWorking = false;
            }
        }
        //
        // private void PerformMeshing(int heightmapXResolution, int heightmapYResolution, int terrainSize, float noiseFactor)
        // {
        //     if (terrainSize < 2 || heightmapXResolution < 2 || heightmapYResolution < 2)
        //     {
        //         return;
        //     }
        //
        //     var zMap = new float[heightmapXResolution,heightmapYResolution];
        //     TerrainTestFunctions.GenerateNoiseZMap((uint)heightmapXResolution, (uint)heightmapYResolution, ref zMap, noiseFactor, noisePeriod);
        //     var mesher = new MeshmerizerR();
        //     Face face = mesher.TerrainMesh(zMap,0, terrainSize,0,terrainSize);
        //
        //     UniMesher.Mesh_Face(face, ref generatedMesh);
        // }
    }
}