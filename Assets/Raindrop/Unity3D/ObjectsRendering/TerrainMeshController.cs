using System;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Rendering;
using Raindrop.Netcom;
using Raindrop.Rendering;
using Raindrop.Utilities;
using UnityEngine;
using RenderSettings = Raindrop.Rendering.RenderSettings;
using Vector3 = UnityEngine.Vector3;

namespace Raindrop.Unity3D
{
    //every generated terrain object has this component.
    // it subscribes to the terrain of the sim it is responsible for rendering. 
    public class TerrainMeshController 
    {
        private RaindropInstance instance { get { return ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>(); } }
        private GridClient Client { get { return instance.Client; } }
        private RaindropNetcom netcom { get { return instance.Netcom; } }
        bool Active => instance.Client.Network.Connected;

        Simulator sim => instance.Client.Network.CurrentSim;
        //unity DS
        Texture2D terrainImage = null;

        UnityEngine.Vector3[] newVertices;
        UnityEngine.Vector2[] newUV;
        int[] newTriangles;


        float[,] heightTable = new float[256, 256];     //heightmap of terrain
        bool fetchingTerrainTexture = false;            //semaphore for reading terrain tex.
        bool terrainTextureNeedsUpdate = false;         //does the texture need to be redrawn?
        private OpenMetaverse.Simulator thisSim;
        float terrainTimeSinceUpdate = Rendering.RenderSettings.MinimumTimeBetweenTerrainUpdated + 1f; // Update terrain om first run
        // bool terrainInProgress = false;
        MeshmerizerR renderer;
        //private float lastTimeItRendered = 0f;


        Face terrainFace; //seems like a 'face' is the secondlife kind of face (where each prim can have up to 8 faces.)
        ColorVertex[] terrainVertices;
        uint[] terrainIndices;
        private bool terrainMesherIsIdle = true;
        bool Modified = true;                    //is the terrain data in OSL(backend) modified since our rendering?
        private readonly TerrainMeshView _terrainMeshView;
        // The distance away (in mapSpace) in which we will delete the sim's terrain and gameobject from RAM.
        public float deleteDistanceThreshold;


        public TerrainMeshController(TerrainMeshView terrainMeshView)
        {
            this._terrainMeshView = terrainMeshView;
            Client.Terrain.LandPatchReceived += new EventHandler<LandPatchReceivedEventArgs>(Terrain_LandPatchReceived);
            Client.Network.SimChanged += NetworkOnSimChanged;
        }

        // if we are too far away from the sim, and for more than 3 seconds, we will kill ourselves.
        private void NetworkOnSimChanged(object sender, SimChangedEventArgs e)
        {
            var istoofar = CheckIfTooFar();
            if (istoofar)
            {
                DeleteMe();
            }
            
            bool CheckIfTooFar()
            {
                ulong avi_sSim = instance.Client.Network.CurrentSim.Handle;
                if (avi_sSim != thisSim.Handle) //different sim now.
                {
                    var mySim_v3 = MapSpaceConverters.Handle2MapSpace(thisSim.Handle, 0);
                    var avatar_s_Sim_v3 = MapSpaceConverters.Handle2MapSpace(avi_sSim, 0);

                    return (Vector3.Distance(mySim_v3, avatar_s_Sim_v3) > deleteDistanceThreshold);

                    // ResetTerrainTex();
                }

                return false;
            }
        }

        private void DeleteMe()
        {
            //todo
            throw new NotImplementedException();
        }

        void Terrain_LandPatchReceived(object sender, LandPatchReceivedEventArgs e)
        {
            if (e.Simulator.Handle == thisSim.Handle)
            {
                this.Modified = true;
            }
        }
        
        // construct a flat land mesh of 256*256 size
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


        //this performs re-meshing and re-texturing. ONLY IF MODIFIED and sufficient time elapsed.
        private void Render(float timeSinceLastRenderCall)
        {
            terrainTimeSinceUpdate += timeSinceLastRenderCall;

            if (terrainMesherIsIdle == false)
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

                    terrainMesherIsIdle = false;
                    //ResetTerrainTex(/*false*/);
                    UpdateTerrain();
            }

            if (terrainTextureNeedsUpdate)
            {
                //PaintTerrain();
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

                terrainMesherIsIdle = false;
                Debug.Log("QueueUserWorkItem");
                // 1. generate heightTable from patches in memory.
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

                // 2. create mesh-face from heighttable, using the meshmeriser.
                terrainFace = renderer.TerrainMesh(heightTable, 0f, 255f, 0f, 255f); //generate mesh with heights //the result is a huge struct 'Face'
                Debug.Log("terrainFace geenerated");
                
                // 3. painting the face. we use ColorVertex objects to represent the color of the vertices of the terrain face.
                // terrainVertices = new ColorVertex[terrainFace.Vertices.Count];
                // for (int i = 0; i < terrainFace.Vertices.Count; i++) //for each vert in terrainFace, append the vert to terraiVerticies
                // {
                //     byte[] part = Utils.IntToBytes(i); // i = 0x 0000 0000 0000 0000   0000 0000 0000 0000 - 32 bits / 4 bytes. 
                //     terrainVertices[i] = new ColorVertex()
                //     {
                //         Vertex = terrainFace.Vertices[i],
                //         Color = new Color4b()
                //         {
                //             R = part[0],
                //             G = part[1],
                //             B = part[2],
                //             A = 253 // terrain picking
                //         }
                //     };
                // }
                // terrainIndices = new uint[terrainFace.Indices.Count];
                // for (int i = 0; i < terrainIndices.Length; i++)
                // {
                //     terrainIndices[i] = terrainFace.Indices[i];
                // }
                // Modified = false;
                // //terrainTextureNeedsUpdate = true;
                // terrainTimeSinceUpdate = 0f;
                //
                // terrainMesherIsIdle = true;
                
                // 4. apply this face-mesh to the gameobject's mesh component.
                // terrainMeshUpdater.setMesh(ref terrainMesh, terrainFace);

            });
        }


        //delete terrain tex 
        private void ResetTerrainTex()
        {
            if (terrainImage != null)
            {
                GameObject.Destroy(terrainImage);
                terrainImage = null;
            }

            fetchingTerrainTexture = false;
            Modified = true;

            //temporary
            //terrainTextureNeedsUpdate = true;
        }


        // todo: use cpu to generate splat map; 
        // todo: use shader to do splatting.
        void PaintTerrain()
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
                    _terrainMeshView.SetTexture(terrainImage);
                });
            }
        }

    }
}