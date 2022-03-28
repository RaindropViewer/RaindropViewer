using OpenMetaverse;
using OpenMetaverse.Rendering;
using Raindrop.Rendering;

namespace Raindrop.Render
{
    public class PrimGen
    {
        
        static MeshmerizerR renderer;
        
        public static void CalculateBoundingBox(RenderPrimitive rprim)
        {
            Primitive prim = rprim.BasePrim;

            // Calculate bounding volumes for each prim and adjust textures
            rprim.BoundingVolume = new BoundingVolume();
            for (int j = 0; j < rprim.Faces.Count; j++)
            {
                Primitive.TextureEntryFace teFace = prim.Textures.GetFace((uint)j);
                if (teFace == null) continue;

                Face face = rprim.Faces[j];
                FaceData data = new FaceData();

                data.BoundingVolume.CreateBoundingVolume(face, prim.Scale);
                rprim.BoundingVolume.AddVolume(data.BoundingVolume, prim.Scale);

                // With linear texture animation in effect, texture repeats and offset are ignored
                if ((prim.TextureAnim.Flags & Primitive.TextureAnimMode.ANIM_ON) != 0
                    && (prim.TextureAnim.Flags & Primitive.TextureAnimMode.ROTATE) == 0
                    && (prim.TextureAnim.Face == 255 || prim.TextureAnim.Face == j))
                {
                    teFace.RepeatU = 1;
                    teFace.RepeatV = 1;
                    teFace.OffsetU = 0;
                    teFace.OffsetV = 0;
                }

                // Sculpt UV vertically flipped compared to prims. Flip back
                if (prim.Sculpt != null && prim.Sculpt.SculptTexture != UUID.Zero && prim.Sculpt.Type != SculptType.Mesh)
                {
                    teFace = (Primitive.TextureEntryFace)teFace.Clone();
                    teFace.RepeatV *= -1;
                }

                // Texture transform for this face
                renderer.TransformTexCoords(face.Vertices, face.Center, teFace, prim.Scale);

                // Set the UserData for this face to our FaceData struct
                face.UserData = data;
                rprim.Faces[j] = face;
            }
        }
    }
}