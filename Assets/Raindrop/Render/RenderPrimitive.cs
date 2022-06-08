using System;
using System.Collections.Generic;
//using OpenTK.Graphics.OpenGL;
using OpenMetaverse;
using OpenMetaverse.Rendering;

namespace Raindrop.Rendering
{

    /// <summary>
    /// Class that handle rendering of objects: simple primitives, sculpties, and meshes
    /// </summary>
    public class RenderPrimitive : SceneObject
    {
        #region Public fields
        /// <summary>Base simulator object</summary>
        public Primitive Prim;
        public List<Face> Faces;
        /// <summary>Is this object attached to an avatar</summary>
        public bool Attached;
        /// <summary>Do we know if object is attached</summary>
        public bool AttachedStateKnown;
        /// <summary>Are meshes constructed and ready for this prim</summary>
        public bool Meshed;
        /// <summary>Process of creating a mesh is underway</summary>
        public bool Meshing;
        #endregion Public fields

        #region Private fields
        int prevTEHash;
        int prevSculptHash;
        int prevShapeHash;
        #endregion Private fields

        /// Default constructor
        private void Awake()
        {
            Type = SceneObjectType.Primitive;
        }

        /// <summary>
        /// Remove any GL resource we may still have in use
        /// </summary>
        public override void Dispose()
        {
            if (Faces != null)
            {
                foreach (Face f in Faces)
                {
                    if (f.UserData is FaceData data)
                    {
                        data.Dispose();
                        data = null;
                    }
                }
                Faces = null;
            }
            base.Dispose();
        }

        /// <summary>
        /// Simulator object that is basis for this SceneObject
        /// </summary>
        public override Primitive BasePrim
        {
            get => Prim;
            set
            {
                Prim = value;

                int TEHash = Prim.Textures?.GetHashCode() ?? 0;

                if (Meshed)
                {
                    if (Prim.Type == PrimType.Sculpt || Prim.Type == PrimType.Mesh)
                    {
                        var sculptHash = Prim.Sculpt.GetHashCode();
                        if (sculptHash != prevSculptHash || TEHash != prevTEHash)
                        {
                            Meshed = false;
                        }
                        prevSculptHash = sculptHash;
                    }
                    else
                    {
                        var shapeHash = Prim.PrimData.GetHashCode();
                        if (shapeHash != prevShapeHash)
                        {
                            Meshed = false;
                        }
                        else if (TEHash != prevTEHash)
                        {
                            Meshed = false;
                        }
                        prevShapeHash = shapeHash;
                    }
                }
                prevTEHash = TEHash;
            }
        }

        /// <summary>
        /// Set initial state of the object
        /// </summary>
        public override void Initialize()
        {
            AttachedStateKnown = false;
            base.Initialize();
        }

        /// Render Primitive
        /// update the state in this view.
        public void Render()
        {
            //filter out attached objects if not rendering people
            if (!RenderSettings.AvatarRenderingEnabled && Attached) return;
            
            // Prim rotation and position and scale
            RenderPrimitive_SetPosition(Prim.Scale, RenderPosition);
            
            // // Do we have animated texture on this face
            // bool animatedTexture = false;
            //
            // // Initialise flags tracking what type of faces this prim has
            // if (pass == RenderPass.Simple)
            // {
            //     HasSimpleFaces = false;
            // }
            // else if (pass == RenderPass.Alpha)
            // {
            //     HasAlphaFaces = false;
            //     
            //     
            // }
            // else if (pass == RenderPass.Invisible)
            // {
            //     HasInvisibleFaces = false;
            //     
            //     
            //     if (!data.TextureInfo.IsInvisible) return animatedTexture;
            //     HasInvisibleFaces = true;
            //
            // }
            //
            // // Draw the prim faces
            // for (int j = 0; j < Faces.Count; j++)
            // {
            //     animatedTexture = DrawPrim(j);
            // }
            //
            // RHelp.ResetMaterial();
            //
            //
            // unsafe bool DrawPrim(int j)
            // {
            //     Primitive.TextureEntryFace teFace = Prim.Textures.GetFace((uint) j);
            //     Face face = Faces[j];
            //     FaceData data = (FaceData) face.UserData;
            //
            //     if (data == null)
            //         return animatedTexture;
            //
            //     if (teFace == null)
            //         return animatedTexture;
            //
            //     // Don't render transparent faces
            //     Color4 RGBA = teFace.RGBA;
            //
            //     if (data.TextureInfo.FullAlpha || RGBA.A <= 0.01f) return animatedTexture;
            //
            //     if
            //     {
            //         if (data.TextureInfo.IsInvisible) return animatedTexture;
            //         bool belongToAlphaPass = (RGBA.A < 0.99f) || (data.TextureInfo.HasAlpha && !data.TextureInfo.IsMask);
            //
            //         if (belongToAlphaPass && pass != RenderPass.Alpha) return animatedTexture;
            //         if (!belongToAlphaPass && pass == RenderPass.Alpha) return animatedTexture;
            //
            //         if (pass == RenderPass.Simple)
            //         {
            //             HasSimpleFaces = true;
            //         }
            //         else if (pass == RenderPass.Alpha)
            //         {
            //             HasAlphaFaces = true;
            //         }
            //
            //
            //         float shiny = 0f;
            //         switch (teFace.Shiny)
            //         {
            //             case Shininess.High:
            //                 shiny = 0.96f;
            //                 break;
            //
            //             case Shininess.Medium:
            //                 shiny = 0.64f;
            //                 break;
            //
            //             case Shininess.Low:
            //                 shiny = 0.24f;
            //                 break;
            //         }
            //
            //         if (shiny > 0f)
            //         {
            //             scene.StartShiny();
            //         }
            //
            //         GL.Material(MaterialFace.Front, MaterialParameter.Shininess, shiny);
            //         var faceColor = new float[] {RGBA.R, RGBA.G, RGBA.B, RGBA.A};
            //         GL.Color4(faceColor);
            //
            //         GL.Material(MaterialFace.Front, MaterialParameter.Specular, new float[] {0.5f, 0.5f, 0.5f, 1f});
            //
            //         if (data.TextureInfo.TexturePointer == 0)
            //         {
            //             TextureInfo teInfo;
            //             if (scene.TryGetTextureInfo(teFace.TextureID, out teInfo))
            //             {
            //                 data.TextureInfo = teInfo;
            //             }
            //         }
            //
            //         if (data.TextureInfo.TexturePointer == 0)
            //         {
            //             
            //             if (!data.TextureInfo.FetchFailed)
            //             {
            //                 scene.DownloadTexture(new TextureLoadItem()
            //                 {
            //                     Prim = Prim,
            //                     TeFace = teFace,
            //                     Data = data
            //                 }, false);
            //             }
            //         }
            //         else
            //         {
            //             // Is this face using texture animation
            //             if ((Prim.TextureAnim.Flags & Primitive.TextureAnimMode.ANIM_ON) != 0
            //                 && (Prim.TextureAnim.Face == j || Prim.TextureAnim.Face == 255))
            //             {
            //                 if (data.AnimInfo == null)
            //                 {
            //                     data.AnimInfo = new TextureAnimationInfo();
            //                 }
            //
            //                 data.AnimInfo.PrimAnimInfo = Prim.TextureAnim;
            //                 data.AnimInfo.Step(time);
            //                 animatedTexture = true;
            //             }
            //
            //             data.AnimInfo = null;
            //
            //             GL.Enable(EnableCap.Texture2D);
            //             GL.BindTexture(TextureTarget.Texture2D, data.TextureInfo.TexturePointer);
            //         }
            //     }
            //
            //     if (!RenderSettings.UseVBO || data.VBOFailed)
            //     {
            //         Vertex[] verts = face.Vertices.ToArray();
            //         ushort[] indices = face.Indices.ToArray();
            //
            //         unsafe
            //         {
            //             fixed (float* normalPtr = &verts[0].Normal.X)
            //             fixed (float* texPtr = &verts[0].TexCoord.X)
            //             {
            //                 GL.NormalPointer(NormalPointerType.Float, FaceData.VertexSize, (IntPtr) normalPtr);
            //                 GL.TexCoordPointer(2, TexCoordPointerType.Float, FaceData.VertexSize, (IntPtr) texPtr);
            //                 GL.VertexPointer(3, VertexPointerType.Float, FaceData.VertexSize, verts);
            //                 GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedShort, indices);
            //             }
            //         }
            //     }
            //     else
            //     {
            //         if (data.CheckVBO(face))
            //         {
            //             Compat.BindBuffer(BufferTarget.ArrayBuffer, data.VertexVBO);
            //             Compat.BindBuffer(BufferTarget.ElementArrayBuffer, data.IndexVBO);
            //             GL.NormalPointer(NormalPointerType.Float, FaceData.VertexSize, (IntPtr) 12);
            //             GL.TexCoordPointer(2, TexCoordPointerType.Float, FaceData.VertexSize, (IntPtr) (24));
            //             GL.VertexPointer(3, VertexPointerType.Float, FaceData.VertexSize, (IntPtr) (0));
            //
            //             GL.DrawElements(PrimitiveType.Triangles, face.Indices.Count, DrawElementsType.UnsignedShort, IntPtr.Zero);
            //         }
            //
            //         Compat.BindBuffer(BufferTarget.ArrayBuffer, 0);
            //         Compat.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            //     }
            //
            //     return animatedTexture;
            // }
        }

        //set my position.
        private void RenderPrimitive_SetPosition(Vector3 primScale, Vector3 renderPosition)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// String representation of the object
        /// </summary>
        /// <returns>String containing local ID of the object and it's distance from the camera</returns>
        public override string ToString()
        {
            uint id = Prim == null ? 0 : Prim.LocalID;
            float distance = (float)Math.Sqrt(DistanceSquared);
            return $"LocalID: {id}, distance {distance:0.00}";
        }
    }
    

    /// <summary>
    /// Contains per primitive face data
    /// </summary>
    public class FaceData : IDisposable
    {
        public float[] Vertices;
        public ushort[] Indices;
        public float[] TexCoords;
        public float[] Normals;
        public int PickingID = -1;
        public int VertexVBO = -1;
        public int IndexVBO = -1;
        public TextureInfo TextureInfo = new TextureInfo();
        public BoundingVolume BoundingVolume = new BoundingVolume();
        public static int VertexSize = 32; // sizeof (vertex), 2  x vector3 + 1 x vector2 = 8 floats x 4 bytes = 32 bytes 
        public TextureAnimationInfo AnimInfo;
        public int QueryID = 0;
        public bool VBOFailed = false;

        /// <summary>
        /// Dispose VBOs if we have them in graphics card memory
        /// </summary>
        public void Dispose()
        {
        }

    }

    /// <summary>
    /// Class handling texture animations
    /// </summary>
    public class TextureAnimationInfo
    {
        public Primitive.TextureAnimation PrimAnimInfo;
        public float CurrentFrame;
        public float CurrentTime;
        public bool PingPong;
        float LastTime = 0f;
        float TotalTime = 0f;

        /// <summary>
        /// Perform texture manipulation to implement texture animations
        /// </summary>
        /// <param name="lastFrameTime">Time passed since the last run (in seconds)</param>
        public void Step(float lastFrameTime)
        {
            //todo: animation of textures.
            
            
        }

    }
    
}
