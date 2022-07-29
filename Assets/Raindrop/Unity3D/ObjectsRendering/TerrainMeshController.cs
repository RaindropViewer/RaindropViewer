using System;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Rendering;
using Plugins.CommonDependencies;
using Raindrop.Netcom;
using Raindrop.Render;
using Raindrop.Rendering;
using UnityEngine;
using Logger = OpenMetaverse.Logger;
using Mesh = UnityEngine.Mesh;
using RenderSettings = Raindrop.Rendering.RenderSettings;

namespace Raindrop.Unity3D
{
    //every generated terrain object has this component.
    // it subscribes to the terrain of the sim it is responsible for rendering. 
    public class TerrainMeshController 
    {
        private RaindropInstance instance { get { return RaindropInstance.GlobalInstance; } }
        private GridClient Client { get { return instance.Client; } }
        private RaindropNetcom netcom { get { return instance.Netcom; } }
        bool Active => instance.Client.Network.Connected;

        public bool isReadyToMesh => instance.Client.Network.CurrentSim != null;

        Simulator sim = null;
        Texture2D terrainImage = null;

        UnityEngine.Vector3[] newVertices;
        UnityEngine.Vector2[] newUV;
        int[] newTriangles;
        
        float[,] heightTable = new float[256, 256];     //heightmap of terrain
        bool fetchingTerrainTexture = false;            //semaphore for reading terrain tex.
        bool terrainTextureNeedsToBeSplatted = false;         //does the texture need to be redrawn?
        private OpenMetaverse.Simulator thisSim;
        float terrainTimeSinceUpdate = Rendering.RenderSettings.MinimumTimeBetweenTerrainUpdated + 1f; // Update terrain om first run
        MeshmerizerR renderer;
        
        Face terrainFace; //seems like a 'face' is the secondlife kind of face (where each prim can have up to 8 faces.)
        ColorVertex[] terrainVertices;
        uint[] terrainIndices;
        
        private bool terrainMesherIsIdle = true;
        public bool Modified = false;                    //is the terrain data in OSL(backend) modified since our rendering?
        private readonly TerrainMeshView _terrainMeshView;

        //reference to the view-layer mesh object. as it is unity API, modify this only on main thread.
        public Mesh mesh_ref;

        public TerrainMeshController(TerrainMeshView terrainMeshView, Mesh terrainMeshRef)
        {
            mesh_ref = terrainMeshRef;
            this._terrainMeshView = terrainMeshView;
            Client.Terrain.LandPatchReceived += new EventHandler<LandPatchReceivedEventArgs>(Terrain_LandPatchReceived);
            Client.Network.SimChanged += NetworkOnSimChanged;
        }

        // if we are too far away from the sim, and for more than 3 seconds, we will kill ourselves.
        private void NetworkOnSimChanged(object sender, SimChangedEventArgs e)
        {
            //todo: update current sim.
            try
            {
                thisSim = instance.Client.Network.CurrentSim;
            }
            catch (Exception exp)
            {
                Logger.Log(exp.ToString(), Helpers.LogLevel.Warning);
            }
            
            
            // var istoofar = CheckIfTooFar();
            // if (istoofar)
            // {
            //     DeleteMe();
            // }
            //
            // bool CheckIfTooFar()
            // {
            //     ulong avi_sSim = instance.Client.Network.CurrentSim.Handle;
            //     if (avi_sSim != thisSim.Handle) //different sim now.
            //     {
            //         var mySim_v3 = MapSpaceConverters.Handle2MapSpace(thisSim.Handle, 0);
            //         var avatar_s_Sim_v3 = MapSpaceConverters.Handle2MapSpace(avi_sSim, 0);
            //
            //         return (Vector3.Distance(mySim_v3, avatar_s_Sim_v3) > deleteDistanceThreshold);
            //
            //         // ResetTerrainTex();
            //     }
            //
            //     return false;
            // }
        }

        void Terrain_LandPatchReceived(object sender, LandPatchReceivedEventArgs e)
        {
            //backend does not know current sim
            if (thisSim == null)
            {
                return;
            }
            
            //filter for packets from current sim only.
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
        public void Render(float timeSinceLastRenderCall)
        {
            terrainTimeSinceUpdate += timeSinceLastRenderCall;

            //1. meshing part.
            // Lock check: if terrainmesher is working, we shall not go further.
            if (terrainMesherIsIdle == false)
            {
                return;
            }
            
            if (sim == null || sim.Terrain == null)
            {
                return;
            }
            
            if (Modified && terrainTimeSinceUpdate > RenderSettings.MinimumTimeBetweenTerrainUpdated)
            {
                Logger.Log("queueing the terrain to be computed! ", Helpers.LogLevel.Info);
                //ResetTerrainTex(/*false*/);
                QueueComputeTerrainMesh();
            }

            
            //2. painting part
            if (terrainTextureNeedsToBeSplatted)
            {
                //PaintTerrain();
            }
            Modified = false;

            //
            // if (terrainIndices == null || terrainVertices == null)
            // {
            //     Logger.Log("terrain indices is null", Helpers.LogLevel.Info);
            //     return;
            // }

            //set texture to mesh
            


            //update / draw new mesh.
            
        }
        
        //queue a job of terrain-computing and splatting
        private void QueueComputeTerrainMesh()
        {
            if (sim == null || sim.Terrain == null)
            {
                Logger.Log("update terrain failed as the sim or terrain is null", Helpers.LogLevel.Info);
                return;
            }

            Logger.Log("putting into threadpool", Helpers.LogLevel.Info);
            // Lock: terrainmesher is working.
            terrainMesherIsIdle = false;

            ThreadPool.QueueUserWorkItem(sync =>
            {
                Logger.Log("QueueUserWorkItem", Helpers.LogLevel.Info);
                
                // 1. generate heightTable from patches in backend.
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
                Logger.Log("finished 1. generate heightTable from patches in backend", Helpers.LogLevel.Info);

                // 2. create mesh-face from heighttable, using the meshmeriser.
                terrainFace = renderer.TerrainMesh(heightTable, 0f, 255f, 0f, 255f); //generate mesh with heights //the result is a huge struct 'Face'
                Logger.Log("finished 2. create mesh-face from heighttable, using the meshmeriser.", Helpers.LogLevel.Info);
                
                // 3. convert omv "face" to unity mesh:
                UniMesher.MeshFromFace(terrainFace, mesh_ref);
                Logger.Log("finished 3. onvert omv face to unity mesh", Helpers.LogLevel.Info);
                
                
                
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


                // Lock: terrainmesher is finished working.
                terrainMesherIsIdle = true;
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
                    terrainTextureNeedsToBeSplatted = false;
                    _terrainMeshView.SetTexture(terrainImage);
                });
            }
        }

        // assign the terrain mesh to a certain simulator.
        public void init(Simulator sim_ref)
        {
            sim = sim_ref;
            
        }
    }
}