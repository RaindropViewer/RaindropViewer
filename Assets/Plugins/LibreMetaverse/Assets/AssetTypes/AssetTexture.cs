/*
 * Copyright (c) 2006-2016, openmetaverse.co
 * Copyright (c) 2021-2022, Sjofn LLC.
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.co nor the names
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using OpenMetaverse.Imaging;
using Plugins.ObjectPool;
using UnityEngine;

namespace OpenMetaverse.Assets
{
    /// <summary>
    /// Represents a texture
    /// </summary>
    public class AssetTexture : Asset
    {
        /// <summary>Override the base classes AssetType</summary>
        public override AssetType AssetType { get { return AssetType.Texture; } }

        /// <summary>A <seealso cref="ManagedImage"/> object containing image data</summary>
        public ManagedImage Image;

        /// <summary></summary>
        public int Components;

        /// <summary>Initializes a new instance of an AssetTexture object</summary>
        public AssetTexture() { }

        /// <summary>
        /// Initializes a new instance of an AssetTexture object
        /// </summary>
        /// <param name="assetID">A unique <see cref="UUID"/> specific to this asset</param>
        /// <param name="assetData">A byte array containing the raw asset data</param>
        public AssetTexture(UUID assetID, byte[] assetData) : base(assetID, assetData) { }

        /// <summary>
        /// Initializes a new instance of an AssetTexture object
        /// </summary>
        /// <param name="image">A <seealso cref="ManagedImage"/> object containing texture data</param>
        public AssetTexture(ManagedImage image)
        {
            Image = image;
            Components = 0;
            if ((Image.Channels & ManagedImage.ImageChannels.Color) != 0)
                Components += 3;
            if ((Image.Channels & ManagedImage.ImageChannels.Gray) != 0)
                ++Components;
            if ((Image.Channels & ManagedImage.ImageChannels.Bump) != 0)
                ++Components;
            if ((Image.Channels & ManagedImage.ImageChannels.Alpha) != 0)
                ++Components;
        }

        /// <summary>
        /// Populates the <seealso cref="AssetData"/> byte array with a JPEG2000
        /// encoded image created from the data in <seealso cref="Image"/>
        /// </summary>
        public override void Encode()
        {
            // image bitmap -> jp2 byte array
            Texture2D t2d_bitmap = TexturePoolSelfImpl.GetInstance().GetFromPool(TextureFormat.ARGB32);
            Image.ExportTex2D(t2d_bitmap);
            byte[] jp2 = Array.Empty<byte>();
            T2D_JP2.Convert_TextureBitmap_To_JP2Bytes_NoMipMap(t2d_bitmap, ref jp2);

            AssetData = jp2;

            TexturePoolSelfImpl.GetInstance().ReturnToPool(t2d_bitmap);

            // using (var writer = new OpenJpegDotNet.IO.Writer(Image.ExportTex2D()))
            // {
            //     AssetData = writer.Encode();
            // }
        }

        /// <summary>
        /// Decodes the JPEG2000 data in <code>AssetData</code> to the
        /// <seealso cref="ManagedImage"/> object <seealso cref="Image"/>
        /// </summary>
        /// <returns>True if the decoding was successful, otherwise false</returns>
        /// Warn: only run on main thread, due to call to T2D.LoadT2DWithoutMipMaps(AssetData, texture);
        public override bool Decode()
        {
            if (AssetData == null || AssetData.Length <= 0) { return false; }

            
            Components = 0;

            // 1. get a texture and give it to the decoder 
            Texture2D texture = TexturePoolSelfImpl.GetInstance().GetFromPool(TextureFormat.RGBA32);

            T2D_JP2.LoadT2DWithoutMipMaps(AssetData, texture); //blocking.
            
            // 2. when decoder is done writing to texture, we can give it the ManagedImage constructor. (oh wtf)
            Image = new ManagedImage(texture); //blocking.

            if ((Image.Channels & ManagedImage.ImageChannels.Color) != 0)
                Components += 3;
            if ((Image.Channels & ManagedImage.ImageChannels.Gray) != 0)
                ++Components;
            if ((Image.Channels & ManagedImage.ImageChannels.Bump) != 0)
                ++Components;
            if ((Image.Channels & ManagedImage.ImageChannels.Alpha) != 0)
                ++Components;

            return true;
        }
    }
}
