/*
 * Copyright (c) 2006-2016, openmetaverse.co
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
using Plugins.CommonDependencies;
using Unity.Collections;
//using System.Drawing;
//using System.Drawing.Imaging;
//using Catnip.Drawing.Imaging;
using UnityEngine;

namespace OpenMetaverse.Imaging
{
    public class ManagedImage
    {
        [Flags]
        public enum ImageChannels
        {
            Gray = 1,
            Color = 2,
            Alpha = 4,
            Bump = 8
        };

        public enum ImageResizeAlgorithm
        {
            NearestNeighbor
        }

        /// <summary>
        /// Image width
        /// </summary>
        public int Width;

        /// <summary>
        /// Image height
        /// </summary>
        public int Height;
        
        /// <summary>
        /// Image channel flags
        /// </summary>
        public ImageChannels Channels;

        /// <summary>
        /// Red channel data
        /// </summary>
        public byte[] Red;
        
        /// <summary>
        /// Green channel data
        /// </summary>
        public byte[] Green;
        
        /// <summary>
        /// Blue channel data
        /// </summary>
        public byte[] Blue;

        /// <summary>
        /// Alpha channel data
        /// </summary>
        public byte[] Alpha;
        
        /// <summary>
        /// Bump channel data
        /// </summary>
        public byte[] Bump;

        /// <summary>
        /// Create a new blank image
        /// </summary>
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        /// <param name="channels">channel flags</param>
        public ManagedImage(int width, int height, ImageChannels channels)
        {
            Width = width;
            Height = height;
            Channels = channels;

            int n = width * height;

            if ((channels & ImageChannels.Gray) != 0)
            {
                Red = new byte[n];
            }
            else if ((channels & ImageChannels.Color) != 0)
            {
                Red = new byte[n];
                Green = new byte[n];
                Blue = new byte[n];
            }

            if ((channels & ImageChannels.Alpha) != 0)
                Alpha = new byte[n];

            if ((channels & ImageChannels.Bump) != 0)
                Bump = new byte[n];
        }

#if !NO_UNSAFE

        public ManagedImage(Color32[] bitmap, int height, int width)
        {
            Width = width;
            Height = height;
            int pixelCount = Width * Height;

            //todo: by default, we assume A,r,g,b,channels are all present
            Channels = ImageChannels.Alpha | ImageChannels.Color;
            Red = new byte[pixelCount];
            Green = new byte[pixelCount];
            Blue = new byte[pixelCount];
            Alpha = new byte[pixelCount];

            for (int i = 0; i < pixelCount; i++)
            {
                Color32 bit = bitmap[i]; // tex.GetPixel(i%Width , i / Width);
                Blue[i] = bit.b;
                Green[i] = bit.g;
                Red[i] = bit.r;
                Alpha[i] = bit.a;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// 
        //apparently this method only loads the image (.tga) as a bitmap, then returns it as a managed image. the bitmap is not used. perhaps we can skip this intermediary?
        // warn: Must be run in main thread.
        public ManagedImage(Texture2D tex)
        {
            Width = tex.width;
            Height = tex.height;

            int pixelCount = Width * Height;

            if (tex.format == TextureFormat.RGBA32)   //PixelFormat.Format32bppArgb  --- 32 bits per pixel; 8 bits each are used for the alpha, red, green, and blue 
            {
                Channels = ImageChannels.Alpha | ImageChannels.Color;
                Red = new byte[pixelCount];
                Green = new byte[pixelCount];
                Blue = new byte[pixelCount];
                Alpha = new byte[pixelCount];

                //BitmapData bd = bitmap.LockBits(new Rectangle(0, 0, Width, Height),
                //    ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);				
				/*
                unsafe
                {
                    byte* pixel = (byte*)bd.Scan0;

                    for (int i = 0; i < pixelCount; i++)
                    {
                        // GDI+ gives us BGRA and we need to turn that in to RGBA
                        Blue[i] = *(pixel++);
                        Green[i] = *(pixel++);
                        Red[i] = *(pixel++);
                        Alpha[i] = *(pixel++);
                    }
                }
				
				*/
			
                for (int i = 0; i < pixelCount; i++)
                {
                    Color32 bit = tex.GetPixel(i%Width , i / Width);
                    Blue[i] = bit.b;
                    Green[i] = bit.g;
                    Red[i] = bit.r;
                    Alpha[i] = bit.a;

                }
                //bitmap.UnlockBits(bd);
            }
			// this 16bit grayscale image format is commented-out for Raindrop and we need to investigate what its used for; can we just ignore it?
			// unity does not seem to support 16bit grayscale
            //else if (tex.format == TextureFormat.Alpha8)  // PixelFormat.Format16bppGrayScale --- 16 bits per pixel. The color information specifies 65536 shades of gray.
            //{
            //    Channels = ImageChannels.Gray;
            //    Red = new byte[pixelCount];

            //    throw new NotImplementedException("16bpp grayscale image support is incomplete");
            //}
            else if (tex.format == TextureFormat.RGB24) //== PixelFormat.Format24bppRgb)
            {
                Channels = ImageChannels.Color;
                Red = new byte[pixelCount];
                Green = new byte[pixelCount];
                Blue = new byte[pixelCount];

                //BitmapData bd = bitmap.LockBits(new Rectangle(0, 0, Width, Height),
                //        ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                //unsafe
                //{
                //    byte* pixel = (byte*)bd.Scan0;

                //    for (int i = 0; i < pixelCount; i++)
                //    {
                //        // GDI+ gives us BGR and we need to turn that in to RGB
                //        Blue[i] = *(pixel++);
                //        Green[i] = *(pixel++);
                //        Red[i] = *(pixel++);
                //    }
                //}

                //bitmap.UnlockBits(bd);

                for (int i = 0; i < pixelCount; i++)
                {
                    //int _x = i % Width;
                    //int _y = i / Width;
                    Color32 bit = tex.GetPixel(i % Width, i / Width);
                    Blue[i] = bit.b;
                    Green[i] = bit.g;
                    Red[i] = bit.r;

                }
            }
			// can we remove the following? this Format32bppRgb is a ridiculous format.
			else if (tex.format == TextureFormat.RGB24) // PixelFormat.Format32bppRgb) --- The remaining 8 bits are not used.
            {
				Channels = ImageChannels.Color;
				Red = new byte[pixelCount];
				Green = new byte[pixelCount];
				Blue = new byte[pixelCount];

                //BitmapData bd = bitmap.LockBits(new Rectangle(0, 0, Width, Height),
                //		ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);

                //NativeArray<Color32> texture = tex.GetRawTextureData<Color32>();

    //            unsafe
				//{
                    
                    //byte* pixel = (byte*)bd.Scan0;

                    for (int i = 0; i < pixelCount; i++)
					{
                    // GDI+ gives us BGR and we need to turn that in to RGB
                        int _x = i % Width;
                        int _y = i / Width;
                        Color32 bit = tex.GetPixel(i % Width, i / Width);
                        Blue[i] = bit.b;
                        Green[i] = bit.g;
						Red[i] = bit.r;
						//Blue[i] = *(pixel++);
						//Green[i] = *(pixel++);
						//Red[i] = *(pixel++);
						//pixel++;	// Skip over the empty byte where the Alpha info would normally be
					}
				//}

				//bitmap.UnlockBits(bd);
			}
			else
            {
                throw new NotSupportedException("Unrecognized pixel format: " + tex.format.ToString());
            }
        }
#endif

        /// <summary>
        /// Convert the channels in the image. Channels are created or destroyed as required.
        /// </summary>
        /// <param name="channels">new channel flags</param>
        [Obsolete] // do not wish to use managed image when there are better alternatives for this raindrop.
        public void ConvertChannels(ImageChannels channels)
        {
            if (Channels == channels)
                return;

            int n = Width * Height;
            ImageChannels add = Channels ^ channels & channels;
            ImageChannels del = Channels ^ channels & Channels;

            if ((add & ImageChannels.Color) != 0)
            {
                Red = new byte[n];
                Green = new byte[n];
                Blue = new byte[n];
            }
            else if ((del & ImageChannels.Color) != 0)
            {
                Red = null;
                Green = null;
                Blue = null;
            }

            if ((add & ImageChannels.Alpha) != 0)
            {
                Alpha = new byte[n];
                FillArray(Alpha, 255);
            }
            else if ((del & ImageChannels.Alpha) != 0)
                Alpha = null;

            if ((add & ImageChannels.Bump) != 0)
                Bump = new byte[n];
            else if ((del & ImageChannels.Bump) != 0)
                Bump = null;

            Channels = channels;
        }

        /// <summary>
        /// Resize or stretch the image using nearest neighbor (ugly) resampling
        /// </summary>
        /// <param name="width">new width</param>
        /// <param name="height">new height</param>
        public void ResizeNearestNeighbor(int width, int height)
        {
            if (width == Width && height == Height)
                return;

            byte[]
                red = null, 
                green = null, 
                blue = null, 
                alpha = null, 
                bump = null;
            int n = width * height;
            int di = 0, si;

            if (Red != null) red = new byte[n];
            if (Green != null) green = new byte[n];
            if (Blue != null) blue = new byte[n];
            if (Alpha != null) alpha = new byte[n];
            if (Bump != null) bump = new byte[n];
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    si = (y * Height / height) * Width + (x * Width / width);
                    if (Red != null) red[di] = Red[si];
                    if (Green != null) green[di] = Green[si];
                    if (Blue != null) blue[di] = Blue[si];
                    if (Alpha != null) alpha[di] = Alpha[si];
                    if (Bump != null) bump[di] = Bump[si];
                    di++;
                }
            }

            Width = width;
            Height = height;
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
            Bump = bump;
        }

        /// <summary>
        /// Create a byte array containing 32-bit RGBA data with a bottom-left
        /// origin, suitable for feeding directly into OpenGL
        /// </summary>
        /// <returns>A byte array containing raw texture data</returns>
        public byte[] ExportRaw()
        {
            byte[] raw = new byte[Width * Height * 4];

            if ((Channels & ImageChannels.Alpha) != 0)
            {
                if ((Channels & ImageChannels.Color) != 0)
                {
                    // RGBA
                    for (int h = 0; h < Height; h++)
                    {
                        for (int w = 0; w < Width; w++)
                        {
                            int pos = (Height - 1 - h) * Width + w;
                            int srcPos = h * Width + w;

                            raw[pos * 4 + 0] = Red[srcPos];
                            raw[pos * 4 + 1] = Green[srcPos];
                            raw[pos * 4 + 2] = Blue[srcPos];
                            raw[pos * 4 + 3] = Alpha[srcPos];
                        }
                    }
                }
                else
                {
                    // Alpha only
                    for (int h = 0; h < Height; h++)
                    {
                        for (int w = 0; w < Width; w++)
                        {
                            int pos = (Height - 1 - h) * Width + w;
                            int srcPos = h * Width + w;

                            raw[pos * 4 + 0] = Alpha[srcPos];
                            raw[pos * 4 + 1] = Alpha[srcPos];
                            raw[pos * 4 + 2] = Alpha[srcPos];
                            raw[pos * 4 + 3] = Byte.MaxValue;
                        }
                    }
                }
            }
            else
            {
                // RGB
                for (int h = 0; h < Height; h++)
                {
                    for (int w = 0; w < Width; w++)
                    {
                        int pos = (Height - 1 - h) * Width + w;
                        int srcPos = h * Width + w;

                        raw[pos * 4 + 0] = Red[srcPos];
                        raw[pos * 4 + 1] = Green[srcPos];
                        raw[pos * 4 + 2] = Blue[srcPos];
                        raw[pos * 4 + 3] = Byte.MaxValue;
                    }
                }
            }

            return raw;
        }

        /// <summary>
        /// Create a byte array containing 32-bit RGBA data with a bottom-left
        /// origin, suitable for feeding directly into OpenGL
        /// </summary>
        /// <returns>A byte array containing raw texture data</returns>
        public void ExportTex2D(Texture2D reference)
        {
            //check for bad thread.
            if (!UnityMainThreadDispatcher.isOnMainThread())
            {
                throw new WrongThreadException();
            }
            
            int byteCount = Width * Height * 4;
            byte[] raw = new byte[byteCount];

            if ((Channels & ImageChannels.Alpha) != 0)
            {
                if ((Channels & ImageChannels.Color) != 0)
                {
                    // RGBA
                    for (int pos = 0; pos < Height * Width; pos++)
                    {
                        raw[pos * 4 + 0] = Blue[pos];
                        raw[pos * 4 + 1] = Green[pos];
                        raw[pos * 4 + 2] = Red[pos];
                        raw[pos * 4 + 3] = Alpha[pos];
                    }
                }
                else
                {
                    // Alpha only
                    for (int pos = 0; pos < Height * Width; pos++)
                    {
                        raw[pos * 4 + 0] = Alpha[pos];
                        raw[pos * 4 + 1] = Alpha[pos];
                        raw[pos * 4 + 2] = Alpha[pos];
                        raw[pos * 4 + 3] = Byte.MaxValue;
                    }
                }
            }
            else
            {
                // RGB
                for (int pos = 0; pos < Height * Width; pos++)
                {
                    raw[pos * 4 + 0] = Blue[pos];
                    raw[pos * 4 + 1] = Green[pos];
                    raw[pos * 4 + 2] = Red[pos];
                    raw[pos * 4 + 3] = Byte.MaxValue;
                }
            }

            // take the texture that the caller passed in...
            Texture2D b = reference;
            b.hideFlags = HideFlags.HideAndDontSave; //this helps us delete this texture later?

            //do a resize if required.
            if (b.height != Height || b.width != Width)
            {
                //do resize
                b.Resize(Width, Height);
            }
            
            //do a quick sanity check that the sizes are exact same.
            var mip0Data = b.GetPixelData<Color32>(0);
            var mip0ByteCount = b.format == TextureFormat.RGBA32 ?  mip0Data.Length * 4 : mip0Data.Length * 3; //4 bytes per texel.
            if (mip0ByteCount != byteCount) //LHS is mip texel count. //RHS is byte array texel count 
            {
                Debug.LogError("mip0 data size (of texture) not match the data size of array! "
                               + mip0ByteCount.ToString() + " vs "+ (byteCount).ToString()
                               );
            } 

            //copy managedinage bytes into the nativearray
            for (int i = 0; i < mip0Data.Length; i++)
            {
                var blue = raw[i * 4 + 0];
                var green = raw[i * 4 + 1];
                var red = raw[i * 4 + 2];
                var alpha = raw[i * 4 + 3];
                mip0Data[i] = new Color32(red, green, blue, alpha);
            }

            b.LoadRawTextureData(mip0Data);
            b.Apply(false);
            // return b;
        }

        public byte[] ExportTGA()
        {
            byte[] tga = new byte[Width * Height * ((Channels & ImageChannels.Alpha) == 0 ? 3 : 4) + 32];
            int di = 0;
            tga[di++] = 0; // idlength
            tga[di++] = 0; // colormaptype = 0: no colormap
            tga[di++] = 2; // image type = 2: uncompressed RGB
            tga[di++] = 0; // color map spec is five zeroes for no color map
            tga[di++] = 0; // color map spec is five zeroes for no color map
            tga[di++] = 0; // color map spec is five zeroes for no color map
            tga[di++] = 0; // color map spec is five zeroes for no color map
            tga[di++] = 0; // color map spec is five zeroes for no color map
            tga[di++] = 0; // x origin = two bytes
            tga[di++] = 0; // x origin = two bytes
            tga[di++] = 0; // y origin = two bytes
            tga[di++] = 0; // y origin = two bytes
            tga[di++] = (byte)(Width & 0xFF); // width - low byte
            tga[di++] = (byte)(Width >> 8); // width - hi byte
            tga[di++] = (byte)(Height & 0xFF); // height - low byte
            tga[di++] = (byte)(Height >> 8); // height - hi byte
            tga[di++] = (byte)((Channels & ImageChannels.Alpha) == 0 ? 24 : 32); // 24/32 bits per pixel
            tga[di++] = (byte)((Channels & ImageChannels.Alpha) == 0 ? 32 : 40); // image descriptor byte

            int n = Width * Height;

            if ((Channels & ImageChannels.Alpha) != 0)
            {
                if ((Channels & ImageChannels.Color) != 0)
                {
                    // RGBA
                    for (int i = 0; i < n; i++)
                    {
                        tga[di++] = Blue[i];
                        tga[di++] = Green[i];
                        tga[di++] = Red[i];
                        tga[di++] = Alpha[i];
                    }
                }
                else
                {
                    // Alpha only
                    for (int i = 0; i < n; i++)
                    {
                        tga[di++] = Alpha[i];
                        tga[di++] = Alpha[i];
                        tga[di++] = Alpha[i];
                        tga[di++] = Byte.MaxValue;
                    }
                }
            }
            else
            {
                // RGB
                for (int i = 0; i < n; i++)
                {
                    tga[di++] = Blue[i];
                    tga[di++] = Green[i];
                    tga[di++] = Red[i];
                }
            }

            return tga;
        }

        private static void FillArray(byte[] array, byte value)
        {
            if (array != null)
            {
                for (int i = 0; i < array.Length; i++)
                    array[i] = value;
            }
        }

        public void Clear()
        {
            FillArray(Red, 0);
            FillArray(Green, 0);
            FillArray(Blue, 0);
            FillArray(Alpha, 0);
            FillArray(Bump, 0);
        }

        public ManagedImage Clone()
        {
            ManagedImage image = new ManagedImage(Width, Height, Channels);
            if (Red != null) image.Red = (byte[])Red.Clone();
            if (Green != null) image.Green = (byte[])Green.Clone();
            if (Blue != null) image.Blue = (byte[])Blue.Clone();
            if (Alpha != null) image.Alpha = (byte[])Alpha.Clone();
            if (Bump != null) image.Bump = (byte[])Bump.Clone();
            return image;
        }
    }
}
