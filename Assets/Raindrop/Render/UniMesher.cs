using System.Collections.Generic;
using OpenMetaverse.Rendering;
using Raindrop.Rendering;
using UnityEngine;
using UnityEngine.Assertions;
using Mesh = UnityEngine.Mesh;

namespace Raindrop.Render
{
    //mesher functions for unity.
    public static class UniMesher
    {
        //generate a generic cube.
        public static void Mesh_Cube(uint width, ref Mesh mesh)
        {
            //1. size.
            float length = width;
            //float _width = width;
            float height = width;
            
            //2. const declaration of the verts.
            Vector3[] c = new Vector3[8];

            c[0] = new Vector3(-length * .5f, -width * .5f, height * .5f);
            c[1] = new Vector3(length * .5f, -width * .5f, height * .5f);
            c[2] = new Vector3(length * .5f, -width * .5f, -height * .5f);
            c[3] = new Vector3(-length * .5f, -width * .5f, -height * .5f);

            c[4] = new Vector3(-length * .5f, width * .5f, height * .5f);
            c[5] = new Vector3(length * .5f, width * .5f, height * .5f);
            c[6] = new Vector3(length * .5f, width * .5f, -height * .5f);
            c[7] = new Vector3(-length * .5f, width * .5f, -height * .5f);
            
            //3. ordering of vertices.
            // 1 way: use 
            //I have used 16 vertices (4 vertices per side). 
            //This is because I want the vertices of each side to have separate normals.
            //(so the object renders light/shade correctly) 
            Vector3[] vertices = new Vector3[]
            {
                c[0], c[1], c[2], c[3], // Bottom
                c[7], c[4], c[0], c[3], // Left
                c[4], c[5], c[1], c[0], // Front
                c[6], c[7], c[3], c[2], // Back
                c[5], c[6], c[2], c[1], // Right
                c[7], c[6], c[5], c[4]  // Top
            };
            
            //5) Define each vertex's Normal
            Vector3 up = Vector3.up;
            Vector3 down = Vector3.down;
            Vector3 forward = Vector3.forward;
            Vector3 back = Vector3.back;
            Vector3 left = Vector3.left;
            Vector3 right = Vector3.right;
            Vector3[] normals = new Vector3[]
            {
                down, down, down, down,             // Bottom
                left, left, left, left,             // Left
                forward, forward, forward, forward,	// Front
                back, back, back, back,             // Back
                right, right, right, right,         // Right
                up, up, up, up	                    // Top
            };
            
            //6) Define each vertex's UV co-ordinates
            Vector2 uv00 = new Vector2(0f, 0f);
            Vector2 uv10 = new Vector2(1f, 0f);
            Vector2 uv01 = new Vector2(0f, 1f);
            Vector2 uv11 = new Vector2(1f, 1f);

            Vector2[] uvs = new Vector2[]
            {
                uv11, uv01, uv00, uv10, // Bottom
                uv11, uv01, uv00, uv10, // Left
                uv11, uv01, uv00, uv10, // Front
                uv11, uv01, uv00, uv10, // Back	        
                uv11, uv01, uv00, uv10, // Right 
                uv11, uv01, uv00, uv10  // Top
            };

            
            //7) Define the Polygons (triangles) that make up the our Mesh (cube)
            //IMPORTANT: Unity uses a 'Clockwise Winding Order' for determining front-facing polygons.
            //This means that a polygon's vertices must be defined in 
            //a clockwise order (relative to the camera) in order to be rendered/visible.
            int[] triangles = new int[]
            {
                3, 1, 0,        3, 2, 1,        // Bottom	
                7, 5, 4,        7, 6, 5,        // Left
                11, 9, 8,       11, 10, 9,      // Front
                15, 13, 12,     15, 14, 13,     // Back
                19, 17, 16,     19, 18, 17,	    // Right
                23, 21, 20,     23, 22, 21,	    // Top
            };
            
            //8) Build the Mesh
            lock (mesh)
            {
                mesh.Clear();
                mesh.vertices = vertices;
                mesh.triangles = triangles;
                mesh.normals = normals;
                mesh.uv = uvs;
                mesh.Optimize();
                //mesh.RecalculateNormals();
            }

            
            //9) Give it a Material
            // Material cubeMaterial = new Material(Shader.Find("Standard"));
            // cubeMaterial.SetColor("_Color", new Color(0f, 0.7f, 0f)); //green main color
            // _cube.GetComponent<Renderer>().material = cubeMaterial;
            
            //9 return
        }        //generate a generic cube.
        
        
        //give OMV.rendering.face, return unity mesh.
        public static void MeshFromFace(Face face, Mesh mesh)
        {
            if (mesh == null)
            {
                Debug.LogError("given a null mesh to generate into! wtf? ");
            }
            mesh.Clear();
            
            //2. copy the verts as-is.
            // consider if we want to re-use verts to achive a blended look.
            var verticesCount = face.Vertices.Count;
            Vector3[] vertices = new Vector3[verticesCount];
            for (int i = 0; i < verticesCount; i++)
            {
                var OMVPos = face.Vertices[i].Position;
                var UnityPos = RHelp.TKVector3(OMVPos);
                vertices[i] = UnityPos;
            }
            
            //5) Define each vertex's Normal
            // Vector3 up = Vector3.up;
            // Vector3 down = Vector3.down;
            // Vector3 forward = Vector3.forward;
            // Vector3 back = Vector3.back;
            // Vector3 left = Vector3.left;
            // Vector3 right = Vector3.right;
            // Vector3[] normals = new Vector3[]
            // {
            //     down, down, down, down,             // Bottom
            //     left, left, left, left,             // Left
            //     forward, forward, forward, forward,	// Front
            //     back, back, back, back,             // Back
            //     right, right, right, right,         // Right
            //     up, up, up, up	                    // Top
            // };
            Vector3[] normals = new Vector3[verticesCount];
            for (int i = 0; i < verticesCount; i++)
            {
                var OMVPos = face.Vertices[i].Normal;
                var UnityPos = RHelp.TKVector3(OMVPos);
                normals[i] = UnityPos;
            }
            
            
            //6) Define each vertex's UV co-ordinates
            // todo: I do not know how the UV is defined as in the prim's face.
            Vector2[] uvs = new Vector2[verticesCount];
            for (int i = 0; i < verticesCount; i++)
            {
                //no need to transform any axis, since 2d-wise x and y are same in OMV and unity.
                uvs[i].x = face.Vertices[i].TexCoord.X;
                uvs[i].y = face.Vertices[i].TexCoord.Y;
            }

            
            //7) Define the Polygons (triangles) that make up the our Mesh (cube)
            //IMPORTANT: Unity uses a 'Clockwise Winding Order' for determining front-facing polygons.
            //This means that a polygon's vertices must be defined in 
            //a clockwise order (relative to the camera) in order to be rendered/visible.
            // int[] triangles = new int[]
            // {
            //     3, 1, 0,        3, 2, 1,        // Bottom	
            //     7, 5, 4,        7, 6, 5,        // Left
            //     11, 9, 8,       11, 10, 9,      // Front
            //     15, 13, 12,     15, 14, 13,     // Back
            //     19, 17, 16,     19, 18, 17,	    // Right
            //     23, 21, 20,     23, 22, 21,	    // Top
            // };
            List<ushort> OMV_ACW_triangleIndex = face.Indices;
            FlipIndicesOrdering(ref OMV_ACW_triangleIndex);
            
            int[] triangles = new int[OMV_ACW_triangleIndex.Count]; //warn: if data from library is bad, we are not checking its integrity...
            // Assert.IsTrue(verticesCount * 3 == OMV_ACW_triangleIndex.Count, 
            //     "wrong count of indices for defining triangles of your mesh: " + 
            //     "expected: " + verticesCount*3 + 
            //     "actual provided data: " + OMV_ACW_triangleIndex.Count
            //     );
            for (int i = 0; i < triangles.Length; i++)
            {
                triangles[i] = OMV_ACW_triangleIndex[i];
            }

            //8) Build the Mesh
            lock (mesh)
            {
                mesh.Clear();
                mesh.vertices = vertices;
                mesh.triangles = triangles;
                mesh.normals = normals;
                mesh.uv = uvs;
                mesh.Optimize();
                //mesh.RecalculateNormals();
            }

            
            //9) Give it a Material
            // Material cubeMaterial = new Material(Shader.Find("Standard"));
            // cubeMaterial.SetColor("_Color", new Color(0f, 0.7f, 0f)); //green main color
            // _cube.GetComponent<Renderer>().material = cubeMaterial;
            
            //9 return
        }

        //flip the Ordering of each group of 3 indexes.
        // exmaple:
        // visially the face looks like:        2 3
        //                                      0 1 
        // input indices: <0 | 3 | 2> , <0 | 1 | 3> (ACW; OMV style.)
        // output indices : <0 | 2 | 3> , <0 | 3 | 1> (CW; Unity style.)
        private static void FlipIndicesOrdering(ref List<ushort> omvAcwTriangleIndex)
        {
            int IndexCount = omvAcwTriangleIndex.Count;
            Assert.IsTrue(IndexCount % 3 == 0, "indices should be divible by 3");
            for (int i = 0; i < IndexCount; i += 3)
            {
                //swap.
                (omvAcwTriangleIndex[i + 1], omvAcwTriangleIndex[i + 2]) =
                    (omvAcwTriangleIndex[i + 2], omvAcwTriangleIndex[i + 1]);
            }
            
        }
    }
}