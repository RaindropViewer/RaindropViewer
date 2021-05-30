// 
// Radegast Metaverse Client
// Copyright (c) 2009-2014, Radegast Development Team
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
//     * Redistributions of source code must retain the above copyright notice,
//       this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the application "Radegast", nor the names of its
//       contributors may be used to endorse or promote products derived from
//       this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
// $Id$
//
using OpenMetaverse;
using System.Collections.Generic;
using UnityEngine;
using Raindrop.Rendering;
using Raindrop.Render;
using System.Threading;
using OpenMetaverse.Rendering;

namespace Raindrop
{
    //I render stuff into the unity scene!
    public class RaindropUnitySceneRenderer: MonoBehaviour
    {
        RaindropInstance Instance;
        GridClient Client => Instance.Client;
        Simulator sim => Instance.Client.Network.CurrentSim;


        public bool Modified = true;
        float[,] heightTable = new float[256, 256];
        Face terrainFace;
        uint[] terrainIndices;
        ColorVertex[] terrainVertices;

        bool terrainInProgress = false;
        bool terrainTextureNeedsUpdate = false;
        float terrainTimeSinceUpdate = Rendering.RenderSettings.MinimumTimeBetweenTerrainUpdated + 1f; // Update terrain om first run

        bool fetchingTerrainTexture = false;

        Texture2D terrainImage = null;
        MeshmerizerR meshrenderer;


        private GameObject land;
        private GameObject Avatar;
        private List<GameObject> SimAvatarList;

        public RaindropUnitySceneRenderer()
        {
            //terrainImage = new Texture2D(1024,1024);
            //terrainImage.hideFlags = HideFlags.HideAndDontSave;

            terrainImage = TempImageManager.CreateTexture2D(1024, 1024);

        }


        public void ResetTerrain()
        {
            ResetTerrain(true);
        }

        public void ResetTerrain(bool removeImage)
        {
            if (terrainImage != null)
            {
                TempImageManager.Destroy(terrainImage);
                //RenderedTextureManager.dispose();
                //terrainImage.Dispose();
                terrainImage = null;
            }

            

            fetchingTerrainTexture = false;
            Modified = true;
        }



        private void UpdateTerrain()
        {
            if (sim == null || sim.Terrain == null) return;

            ThreadPool.QueueUserWorkItem(sync =>
            {
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

                terrainFace = meshrenderer.TerrainMesh(heightTable, 0f, 255f, 0f, 255f);
                terrainVertices = new ColorVertex[terrainFace.Vertices.Count];
                for (int i = 0; i < terrainFace.Vertices.Count; i++)
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
            });
        }


        void UpdateTerrainTexture()
        {
            if (!fetchingTerrainTexture)
            {
                fetchingTerrainTexture = true;
                ThreadPool.QueueUserWorkItem(sync =>
                {
                    Simulator sim = Client.Network.CurrentSim;
                    terrainImage = TerrainSplat.Splat(Instance, heightTable,
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