using OpenMetaverse;
using UnityEngine;

namespace Raindrop.Map.Model
{
    /// <summary>
    /// A DS that represents a maptile in the scene.
    /// The implementation assumes 1 sim per tile; 256*256 resolution per tile. Clearly, at further zoom levels we want to squeeze more sims into a single tile.
    /// </summary>
    
    public class MapTile
    {
        // it seems that a tile from the http is ~ 17KB in JPEG format; making it kind of spammable at the highest zoom level of 4.
        private int size; //in sims.
        private Texture2D texture;

        private ulong gridHandle;

        public bool isReady = false; 

        private static Texture2D emptyTexture = Texture2D.redTexture;

        public MapTile(int width, int height)
        {
            clearTex();
        }

        /// <summary>
        /// get the texture in the tile.
        /// </summary>
        /// <returns></returns>
        public Texture2D getTex()
        {
            return texture;
        }


        //clear the internal texture and location of the maptile.
        public void clearTex()
        {
            texture = emptyTexture; //TODO : have not called Destroy(texture) to free memory.
            size = 0;
            gridHandle = 0;
        }


        /// <summary>
        /// Set the maptile's handle and texture
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="handle"></param>
        private void setTex(Texture2D tex, ulong handle)
        {
            setTex(tex);
            setLoc(handle);
        }

        //set the internal texture of the maptile.
        private void setTex(Texture2D tex)
        {
            texture = tex;
            size = 1;
        }

        //set the location of sim that the maptile represents.
        private void setLoc(ulong handle)
        {
            gridHandle = handle;
        }

        //set the location of sim that the maptile represents.
        public ulong getLoc()
        {
            return gridHandle;
        }

    }


}