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
using RenderSettings = Raindrop.Rendering.RenderSettings;
using OpenMetaverse.Rendering;

namespace Raindrop.Unity3D
{
    //sets the mesh of the gameobject to match the sim's shape. 
    class TerrainMeshUpdater : MonoBehaviour
    {
        private RaindropInstance instance { get { return RaindropInstance.GlobalInstance; } }
        private GridClient Client { get { return instance.Client; } }
        private RaindropNetcom netcom { get { return instance.Netcom; } }
        bool Active => instance.Client.Network.Connected;

        //unity DS
        Texture2D terrainImage = null;
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        UnityEngine.Mesh terrainMesh;

        UnityEngine.Vector3[] newVertices;
        UnityEngine.Vector2[] newUV;
        int[] newTriangles;


        public bool Modified = true;                    //is the terrain data in OSL(backend) modified since our rendering?
        float[,] heightTable = new float[256, 256];     //heightmap of terrain
        bool fetchingTerrainTexture = false;            //semaphore for reading terrain tex.
        bool terrainTextureNeedsUpdate = false;         //does the texture need to be redrawn?
        private OpenMetaverse.Simulator knownCurrentSim;
        float terrainTimeSinceUpdate = Rendering.RenderSettings.MinimumTimeBetweenTerrainUpdated + 1f; // Update terrain om first run
        bool terrainInProgress = false;
        MeshmerizerR renderer;
        //private float lastTimeItRendered = 0f;

        Simulator sim => instance.Client.Network.CurrentSim;

        Face terrainFace; //seems like a 'face' is the secondlife kind of face (where each prim can have up to 8 faces.)
        ColorVertex[] terrainVertices;
        uint[] terrainIndices;
        private bool lastWorkWasDone = true;

        private void Awake()
        {
            //1 mesh renderer component
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = new UnityEngine.Material(Shader.Find("Standard")); //hopefully this not use reflection.

            //2 mesh filter component (owns the mesh)
            meshFilter = gameObject.AddComponent<MeshFilter>();
            
            //2.1 make terrain mesh of 256*256 at zero height and pass to meshfilter
            terrainMesh = new UnityEngine.Mesh(); //make the mesh.
            GetComponent<MeshFilter>().mesh = terrainMesh;         //assign this mesh to the meshfiltercomponent

            //mesh.vertices = newVertices;
            //mesh.uv = newUV;
            //mesh.triangles = newTriangles;
            buildBasicLandMesh();


        }

        private void Start()
        {


            Client.Terrain.LandPatchReceived += new EventHandler<LandPatchReceivedEventArgs>(Terrain_LandPatchReceived);


        }


        void Terrain_LandPatchReceived(object sender, LandPatchReceivedEventArgs e)
        {
            if (e.Simulator.Handle == Client.Network.CurrentSim.Handle)
            {
                this.Modified = true;
            }
        }

        private void buildBasicLandMesh()
        {

            int step = 1;
            for (int x = 0; x < 256; x += step)
            {
                for (int y = 0; y < 256; y += step)
                {
                    float z = 0;
                    UnityEngine.Vector3[] newVertices;

                    heightTable[x, y] = z;
                }
            }
        }


        private void Update()
        {
            if (! Active) //guard clause: don't continue if disconnected
            {
                return;
            }

            if (instance.Client.Network.CurrentSim != knownCurrentSim) //different sim now.
            {
                knownCurrentSim = instance.Client.Network.CurrentSim;
                ResetTerrain();
            }


            if (terrainTextureNeedsUpdate)
            {
                UpdateTerrainTexture();
            }

            render(Time.deltaTime);

        }

        //this performs re-meshing and re-texturing. ONLY IF MODIFIED and sufficient time elapsed.
        private void render(float timeSinceLastFrame)
        {
            terrainTimeSinceUpdate += timeSinceLastFrame;

            if (lastWorkWasDone == false)
            {
                return;
            }

            if (sim == null )
            {
                //Debug.Log("sim is null");
                return;
            }
            if (sim.Terrain == null)
            {
                //Debug.Log("sim.Terrain is null");
                return;
            }

            Debug.Log("terrain conditions. both are not null");

            if (Modified && terrainTimeSinceUpdate > RenderSettings.MinimumTimeBetweenTerrainUpdated)
            {
                Debug.Log("Processing new rendering of terrain!");

                if (!terrainInProgress)
                {
                    terrainInProgress = true;
                    ResetTerrain(/*false*/);
                    UpdateTerrain();
                }
            }

            if (terrainTextureNeedsUpdate)
            {
                UpdateTerrainTexture();
            }


            if (terrainIndices == null || terrainVertices == null)
            {
                Debug.Log("terrain indices is null");
                return;
            }

            //set texture to mesh



            //update / draw new mesh.


        }
        private void UpdateTerrain()
        {
            if (sim == null || sim.Terrain == null)
            {
                Debug.Log("update terrain failed as the sim or terrain is null");
                return;
            }

            Debug.Log("putting into threadpool");

            ThreadPool.QueueUserWorkItem(sync =>
            {

                lastWorkWasDone = false;
                Debug.Log("QueueUserWorkItem");
                int step = 1;

                for (int x = 0; x < 256; x += step)
                {
                    for (int y = 0; y < 256; y += step)
                    {
                        float z = 0;
                        int patchNr = ((int)x / 16) * 16 + (int)y / 16;
                        if (sim.Terrain[patchNr] != null
                            && sim.Terrain[patchNr].Data != null)
                        {
                            float[] data = sim.Terrain[patchNr].Data;
                            z = data[(int)x % 16 * 16 + (int)y % 16];
                        }
                        heightTable[x, y] = z;
                    }
                }
                Debug.Log("finished terrain height work!");

                terrainFace = renderer.TerrainMesh(heightTable, 0f, 255f, 0f, 255f); //generate mesh with heights //the result is a huge struct 'Face'

                Debug.Log("terrainFace geenerated");
                //generate mesh with colors
                terrainVertices = new ColorVertex[terrainFace.Vertices.Count];
                for (int i = 0; i < terrainFace.Vertices.Count; i++) //for each vert in terrainFace, append the vert to terraiVerticies
                {
                    byte[] part = Utils.IntToBytes(i);
                    terrainVertices[i] = new ColorVertex()
                    {
                        Vertex = terrainFace.Vertices[i],
                        Color = new Color4b()
                        {
                            R = part[0],
                            G = part[1],
                            B = part[2],
                            A = 253 // terrain picking
                        }
                    };
                }
                terrainIndices = new uint[terrainFace.Indices.Count];
                for (int i = 0; i < terrainIndices.Length; i++)
                {
                    terrainIndices[i] = terrainFace.Indices[i];
                }
                terrainInProgress = false;
                Modified = false;
                terrainTextureNeedsUpdate = true;
                terrainTimeSinceUpdate = 0f;

                lastWorkWasDone = true;
            });
        }

        //delete terrain tex 
        private void ResetTerrain()
        {
            if (terrainImage != null)
            {
                Destroy(terrainImage);
                terrainImage = null;
            }

            fetchingTerrainTexture = false;
            Modified = true;

            //temporary
            //terrainTextureNeedsUpdate = true;
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
                    meshRenderer.material.mainTexture = terrainImage;
                });
            }
        }


    }
}
