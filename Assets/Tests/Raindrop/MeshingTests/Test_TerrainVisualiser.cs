using System;
using OpenMetaverse.Rendering;
using Raindrop.Render;
using Tests.Raindrop.MeshingTests;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using Mesh = UnityEngine.Mesh;

//help us see that the terrain mesher works.
public class Test_TerrainVisualiser : MonoBehaviour
{
    #region params

    public int HeightmapX = 256;
    public int HeightmapY = 256;
    public int TerrainSize = 256;
    public float noiseAmplitude = 10f;
    public float noisePeriod = 10f;

    private int HeightmapX_prev = 256;
    private int HeightmapY_prev = 256;
    private int TerrainSize_prev = 256;
    private float noiseFactor_prev = 10f;
    private float noisePeriod_prev = 10f;
    
    private bool needsupdate = true;
    private bool isWorking = false;

    public MeshFilter MeshFilter;
    public Mesh generatedMesh;

    #endregion

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
        if (isWorking)
        {
            return;
        }

        bool paramsChanged = (HeightmapX_prev != HeightmapX) ||
                            (HeightmapY_prev != HeightmapY) ||
                            (TerrainSize_prev != TerrainSize) || 
                            (Mathf.Abs(noiseFactor_prev - noiseAmplitude) > 0.01f) ||
                            (Mathf.Abs(noisePeriod_prev - noisePeriod) > 0.01f);
        if (paramsChanged)
        {
            needsupdate = true;
            HeightmapX_prev = HeightmapX;
            HeightmapY_prev = HeightmapY;
            TerrainSize_prev = TerrainSize;
            noiseFactor_prev = noiseAmplitude;
            noisePeriod_prev = noisePeriod;
        }
        
        if (needsupdate)
        {
            needsupdate = false;
            isWorking = true;
            PerformMeshing(HeightmapX,HeightmapY, TerrainSize, noiseAmplitude);
            isWorking = false;
        }
    }

    private void PerformMeshing(int heightmapXResolution, int heightmapYResolution, int terrainSize, float noiseFactor)
    {
        if (terrainSize < 2 || heightmapXResolution < 2 || heightmapYResolution < 2)
        {
            return;
        }

        var zMap = new float[heightmapXResolution,heightmapYResolution];
        TerrainTestFunctions.GenerateNoiseZMap((uint)heightmapXResolution, (uint)heightmapYResolution, ref zMap, noiseFactor, noisePeriod);
        var mesher = new MeshmerizerR();
        Face face = mesher.TerrainMesh(zMap,0, terrainSize,0,terrainSize);

        UniMesher.Mesh_Face(face, ref generatedMesh);
    }
}