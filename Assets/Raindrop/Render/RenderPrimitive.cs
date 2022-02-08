/**
 * Radegast Metaverse Client
 * Copyright(c) 2009-2014, Radegast Development Team
 * Copyright(c) 2016-2020, Sjofn, LLC
 * All rights reserved.
 *  
 * Radegast is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program.If not, see<https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
//using OpenTK.Graphics.OpenGL;
using OpenMetaverse;
using OpenMetaverse.Rendering;

namespace Raindrop.Rendering
{

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
        //public void Step(float lastFrameTime)
        //{
        //    float numFrames = 1f;
        //    float fullLength = 1f;

        //    if (PrimAnimInfo.Length > 0)
        //    {
        //        numFrames = PrimAnimInfo.Length;
        //    }
        //    else
        //    {
        //        numFrames = Math.Max(1f, (float)(PrimAnimInfo.SizeX * PrimAnimInfo.SizeY));
        //    }

        //    if ((PrimAnimInfo.Flags & Primitive.TextureAnimMode.PING_PONG) != 0)
        //    {
        //        if ((PrimAnimInfo.Flags & Primitive.TextureAnimMode.SMOOTH) != 0)
        //        {
        //            fullLength = 2f * numFrames;
        //        }
        //        else if ((PrimAnimInfo.Flags & Primitive.TextureAnimMode.LOOP) != 0)
        //        {
        //            fullLength = 2f * numFrames - 2f;
        //            fullLength = Math.Max(1f, fullLength);
        //        }
        //        else
        //        {
        //            fullLength = 2f * numFrames - 1f;
        //            fullLength = Math.Max(1f, fullLength);
        //        }
        //    }
        //    else
        //    {
        //        fullLength = numFrames;
        //    }

        //    float frameCounter;
        //    if ((PrimAnimInfo.Flags & Primitive.TextureAnimMode.SMOOTH) != 0)
        //    {
        //        frameCounter = lastFrameTime * PrimAnimInfo.Rate + LastTime;
        //    }
        //    else
        //    {
        //        TotalTime += lastFrameTime;
        //        frameCounter = TotalTime * PrimAnimInfo.Rate;
        //    }
        //    LastTime = frameCounter;

        //    if ((PrimAnimInfo.Flags & Primitive.TextureAnimMode.LOOP) != 0)
        //    {
        //        frameCounter %= fullLength;
        //    }
        //    else
        //    {
        //        frameCounter = Math.Min(fullLength - 1f, frameCounter);
        //    }

        //    if ((PrimAnimInfo.Flags & Primitive.TextureAnimMode.SMOOTH) == 0)
        //    {
        //        frameCounter = (float)Math.Floor(frameCounter + 0.01f);
        //    }

        //    if ((PrimAnimInfo.Flags & Primitive.TextureAnimMode.PING_PONG) != 0)
        //    {
        //        if (frameCounter > numFrames)
        //        {
        //            if ((PrimAnimInfo.Flags & Primitive.TextureAnimMode.SMOOTH) != 0)
        //            {
        //                frameCounter = numFrames - (frameCounter - numFrames);
        //            }
        //            else
        //            {
        //                frameCounter = (numFrames - 1.99f) - (frameCounter - numFrames);
        //            }
        //        }
        //    }

        //    if ((PrimAnimInfo.Flags & Primitive.TextureAnimMode.REVERSE) != 0)
        //    {
        //        if ((PrimAnimInfo.Flags & Primitive.TextureAnimMode.SMOOTH) != 0)
        //        {
        //            frameCounter = numFrames - frameCounter;
        //        }
        //        else
        //        {
        //            frameCounter = (numFrames - 0.99f) - frameCounter;
        //        }
        //    }

        //    frameCounter += PrimAnimInfo.Start;

        //    if ((PrimAnimInfo.Flags & Primitive.TextureAnimMode.SMOOTH) == 0)
        //    {
        //        frameCounter = (float)Math.Round(frameCounter);
        //    }


        //    GL.MatrixMode(MatrixMode.Texture);
        //    GL.LoadIdentity();

        //    if ((PrimAnimInfo.Flags & Primitive.TextureAnimMode.ROTATE) != 0)
        //    {
        //        //GL.Translate(0.5f, 0.5f, 0f);
        //        GL.Rotate(Utils.RAD_TO_DEG * frameCounter, OpenTK.Vector3d.UnitZ);
        //        //GL.Translate(-0.5f, -0.5f, 0f);
        //    }
        //    else if ((PrimAnimInfo.Flags & Primitive.TextureAnimMode.SCALE) != 0)
        //    {
        //        GL.Scale(frameCounter, frameCounter, 0);
        //    }
        //    else // Translate
        //    {
        //        float sizeX = Math.Max(1f, (float)PrimAnimInfo.SizeX);
        //        float sizeY = Math.Max(1f, (float)PrimAnimInfo.SizeY);

        //        GL.Scale(1f / sizeX, 1f / sizeY, 0);
        //        GL.Translate(frameCounter % sizeX, Math.Floor(frameCounter / sizeY), 0);
        //    }

        //    GL.MatrixMode(MatrixMode.Modelview);
        //}

        //[Obsolete("Use Step() instead")]
        //public void ExperimentalStep(float time)
        //{
        //    int reverseFactor = 1;
        //    float rate = PrimAnimInfo.Rate;

        //    if (rate < 0)
        //    {
        //        rate = -rate;
        //        reverseFactor = -reverseFactor;
        //    }

        //    if ((PrimAnimInfo.Flags & Primitive.TextureAnimMode.REVERSE) != 0)
        //    {
        //        reverseFactor = -reverseFactor;
        //    }

        //    CurrentTime += time;
        //    float totalTime = 1 / rate;

        //    uint x = Math.Max(1, PrimAnimInfo.SizeX);
        //    uint y = Math.Max(1, PrimAnimInfo.SizeY);
        //    uint nrFrames = x * y;

        //    if (PrimAnimInfo.Length > 0 && PrimAnimInfo.Length < nrFrames)
        //    {
        //        nrFrames = (uint)PrimAnimInfo.Length;
        //    }

        //    GL.MatrixMode(MatrixMode.Texture);
        //    GL.LoadIdentity();

        //    if (CurrentTime >= totalTime)
        //    {
        //        CurrentTime = 0;
        //        CurrentFrame++;
        //        if (CurrentFrame > nrFrames) CurrentFrame = (uint)PrimAnimInfo.Start;
        //        if ((PrimAnimInfo.Flags & Primitive.TextureAnimMode.PING_PONG) != 0)
        //        {
        //            PingPong = !PingPong;
        //        }
        //    }

        //    float smoothOffset = 0f;

        //    if ((PrimAnimInfo.Flags & Primitive.TextureAnimMode.SMOOTH) != 0)
        //    {
        //        smoothOffset = (CurrentTime / totalTime) * reverseFactor;
        //    }

        //    float f = CurrentFrame;
        //    if (reverseFactor < 0)
        //    {
        //        f = nrFrames - CurrentFrame;
        //    }

        //    if ((PrimAnimInfo.Flags & Primitive.TextureAnimMode.ROTATE) == 0) // not rotating
        //    {
        //        GL.Scale(1f / x, 1f / y, 0f);
        //        GL.Translate((f % x) + smoothOffset, f / y, 0);
        //    }
        //    else
        //    {
        //        smoothOffset = (CurrentTime * PrimAnimInfo.Rate);
        //        float startAngle = PrimAnimInfo.Start;
        //        float endAngle = PrimAnimInfo.Length;
        //        float angle = startAngle + (endAngle - startAngle) * smoothOffset;
        //        GL.Translate(0.5f, 0.5f, 0f);
        //        GL.Rotate(Utils.RAD_TO_DEG * angle, OpenTK.Vector3d.UnitZ);
        //        GL.Translate(-0.5f, -0.5f, 0f);
        //    }

        //    GL.MatrixMode(MatrixMode.Modelview);
        //}

    }

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

        /// <summary>
        /// Default constructor
        /// </summary>
        public RenderPrimitive()
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
                        if (Prim.Sculpt.GetHashCode() != prevSculptHash || TEHash != prevTEHash)
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
}
