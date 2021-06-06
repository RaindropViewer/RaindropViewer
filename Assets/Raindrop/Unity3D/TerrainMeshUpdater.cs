using Raindrop.Netcom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Raindrop.Rendering;
using OpenMetaverse;
using System.Threading;

namespace Raindrop.Unity3D
{
    //sets the mesh of the gameobject to match the sim's shape. 
    class TerrainMeshUpdater : MonoBehaviour
    {
        private RaindropInstance instance { get { return RaindropInstance.GlobalInstance; } }
        private RaindropNetcom netcom { get { return instance.Netcom; } }
        bool Active => instance.Client.Network.Connected;



        public bool Modified = true;
        float[,] heightTable = new float[256, 256];
        bool fetchingTerrainTexture = false;
        Texture2D terrainImage = null;
        MeshRenderer meshRenderer;
        bool terrainTextureNeedsUpdate = false;
        private OpenMetaverse.Simulator knownCurrentSim;

        private void Awake()
        {
            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = new UnityEngine.Material(Shader.Find("Standard")); //hopefully this not use reflection.

        }

        private void Update()
        {
            if (! Active)
            {
                return;
            }

            if (instance.Client.Network.CurrentSim != knownCurrentSim)
            {
                knownCurrentSim = instance.Client.Network.CurrentSim;
                resetMesh();
            }


            if (terrainTextureNeedsUpdate)
            {
                UpdateTerrainTexture();
            }

        }

        private void resetMesh()
        {
            if (terrainImage != null)
            {
                Destroy(terrainImage);
                terrainImage = null;
            }

            fetchingTerrainTexture = false;
            Modified = true;
        }


        void UpdateTerrainTexture()
        {
            if (!fetchingTerrainTexture)
            {
                fetchingTerrainTexture = true;
                ThreadPool.QueueUserWorkItem(sync =>
                {
                    Simulator sim = instance.Client.Network.CurrentSim;
                    terrainImage = TerrainSplat.Splat(instance, heightTable,
                        new UUID[] { sim.TerrainDetail0, sim.TerrainDetail1, sim.TerrainDetail2, sim.TerrainDetail3 },
                        new float[] { sim.TerrainStartHeight00, sim.TerrainStartHeight01, sim.TerrainStartHeight10, sim.TerrainStartHeight11 },
                        new float[] { sim.TerrainHeightRange00, sim.TerrainHeightRange01, sim.TerrainHeightRange10, sim.TerrainHeightRange11 });

                    fetchingTerrainTexture = false;
                    terrainTextureNeedsUpdate = false;
                });
            }
        }


    }
}
