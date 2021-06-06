
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;

public class WaveGenerator : MonoBehaviour
{
    [Header("Wave Parameters")]
    public float waveScale;
    public float waveOffsetSpeed;
    public float waveHeight;

    [Header("References and Prefabs")]
    public MeshFilter waterMeshFilter;
    private Mesh waterMesh;


    NativeArray<Vector3> waterVertices;
    NativeArray<Vector3> waterNormals;

    JobHandle meshModificationJobHandle; // 1
    UpdateMeshJob meshModificationJob; // 2

    [BurstCompile]

    private struct UpdateMeshJob : IJobParallelFor
    {
        // 1
        public NativeArray<Vector3> vertices;

        // 2
        [ReadOnly]
        public NativeArray<Vector3> normals;

        // 3
        public float offsetSpeed;
        public float scale;
        public float height;

        // 4
        public float time;

        private float Noise(float x, float y)
        {
            float2 pos = math.float2(x, y);
            return noise.snoise(pos);
        }


        public void Execute(int i)
        {
            // 1
            if (normals[i].z > 0f)
            {
                // 2
                var vertex = vertices[i];

                // 3
                float noiseValue =
                Noise(vertex.x * scale + offsetSpeed * time, vertex.y * scale +
                offsetSpeed * time);

                // 4
                vertices[i] =
                new Vector3(vertex.x, vertex.y, noiseValue * height + 0.3f);
            }

        }


    }

    private void Start()
    {
        waterMesh = waterMeshFilter.mesh;

        waterMesh.MarkDynamic(); // 1

        waterVertices =
        new NativeArray<Vector3>(waterMesh.vertices, Allocator.Persistent); // 2

        waterNormals =
        new NativeArray<Vector3>(waterMesh.normals, Allocator.Persistent);

    }
    private void OnDestroy()
    {
        waterVertices.Dispose();
        waterNormals.Dispose();
    }

    private void Update()
    {
        // 1
        meshModificationJob = new UpdateMeshJob()
        {
            vertices = waterVertices,
            normals = waterNormals,
            offsetSpeed = waveOffsetSpeed,
            time = Time.time,
            scale = waveScale,
            height = waveHeight
        };

        // 2
        meshModificationJobHandle =
        meshModificationJob.Schedule(waterVertices.Length, 64);

    }

    private void LateUpdate()
    {
        // 1
        meshModificationJobHandle.Complete();

        // 2
        waterMesh.SetVertices(meshModificationJob.vertices);

        // 3
        waterMesh.RecalculateNormals();

    }

}
