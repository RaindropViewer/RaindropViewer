using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpenMetaverse.Rendering;
using Raindrop.Render;
using UnityEngine;
using Mesh = UnityEngine.Mesh;

namespace Tests.Raindrop.MeshingTests
{
    public class Terrain
    {
        [Test]
        public void OMVTerrainMesher_SingleQuad()
        {
            float[,] zMap = new float[,] {{0, 1},{0,0}};
            float xBegin = 0f;
            float xEnd = 1f;
            float yBegin = 0f;
            float yEnd = 1f;

            var mesher = new MeshmerizerR();
                
            var face = mesher.TerrainMesh(zMap,xBegin,xEnd,yBegin,yEnd);
            
            Assert.True(face.Vertices.Count == 4);
            Assert.True(heightOf1_1ishigherthanneightbours());
            PrintFaceInfo(face);

            bool heightOf1_1ishigherthanneightbours()
            {
                var posExpectedHigherThanNeighbours = face.Vertices[1];
                var neighbours = new List<Vertex>
                {
                    face.Vertices[0],
                    face.Vertices[1],
                    face.Vertices[2]
                };
                bool res = true;
                foreach (var neighbour in neighbours)
                {
                    if (posExpectedHigherThanNeighbours.Position.Z < neighbour.Position.Z)
                    {
                        res = false;
                    }
                }
                return res;
            }
        }

        // test: able to mesh 1x1 heightmap into a mesh of size 1x1
        // obviously, there is unlikely ever a 1x1 terrain. just for sanity check..
        [Test]
        public void UnityTerrainMesher_SingleQuad()
        {
            //omv part terrain mesh.
            Face face = OMVSingleQuad_TerrainMesh();

            //unity part terrain mesh.
            var mesh = new Mesh();
            UniMesher.Mesh_Face(face, ref mesh);
            Assert.True(mesh.vertexCount == face.Vertices.Count);
            
            Face OMVSingleQuad_TerrainMesh()
            {
                float[,] zMap = new float[,] {{0, 1}, {0, 0}};
                float xBegin = 0f;
                float xEnd = 1f;
                float yBegin = 0f;
                float yEnd = 1f;

                var mesher = new MeshmerizerR();

                var face = mesher.TerrainMesh(zMap, xBegin, xEnd, yBegin, yEnd);
                return face;
            }
        }

        // test: able to mesh 256^2 heightmap into a mesh of size 64^2
        [Test]
        public void OMVTerrainMesher_256ZMap_64Mesh()
        {
            uint xDesired = 256;
            uint yDesired = 256;
            float[,] zMap = new float[xDesired, yDesired];

            TerrainTestFunctions.GenerateNoiseZMap(xDesired, yDesired, ref zMap , 10f, 10f);

            // mesh gen parameters: 256*256 heightmap input is applied to a mesh of size 64*64.
            float xBegin = 0f;
            float xEnd = 64f;
            float yBegin = 0f;
            float yEnd = 64f;

            var mesher = new MeshmerizerR();
                
            Face face = mesher.TerrainMesh(zMap,xBegin,xEnd,yBegin,yEnd);
            
            Assert.True(face.Vertices.Count == 256*256);
            PrintFaceInfo(face);
            
        }

        private void PrintFaceInfo(Face face)
        {
            
            Debug.Log("ID" + face.ID + "\n" +
                      "Center" + face.Center);
            //"min" + face.MinExtent + "\n" + //unused
            //"max" + face.MaxExtent + "\n" + //unused
            Debug.Log("verts " + String.Join(" | ", face.Vertices.GetRange(0,Mathf.Min(face.Vertices.Count,10))));
            Debug.Log("indices " + String.Join(" | ", face.Indices.GetRange(0,Mathf.Min(face.Vertices.Count,30))));
            Debug.Log("edges " + (face.Edge != null ? 
                String.Join(" | " , face.Edge) : 
                "null" ));
            Debug.Log("facemask" + face.Mask.ToString());
            Debug.Log("textureface.ID" + 
                      (face.TextureFace != null ?
                          face.TextureFace.TextureID.ToString() : 
                          "null"));
        }
    }
}