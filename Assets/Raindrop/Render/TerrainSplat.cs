/**
 * Raindrop Metaverse Client
 * Copyright(c) 2009-2014, Raindrop Development Team
 * Copyright(c) 2016-2020, Sjofn, LLC
 * All rights reserved.
 *  
 * Raindrop is free software: you can redistribute it and/or modify
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




//To do the terrain splatting you build an array the size of the heightmap with values [0-3] that map to the four terrain textures. Floating point is used so you can blend between the textures when creating the final output. The array is a combination of the actual height (scaled down to 0-3) and some perlin noise. Heres clean room documentation of the noise generation:

//vec = global_position * 0.20319f;
//low_freq = perlin_noise2(vec.X * 0.222222, vec.Y * 0.222222) * 6.5;
//high_freq = perlin_turbulence2(vec.X, vec.Y, 2) * 2.25;
//noise = (low_freq + high_freq) * 2;

//To build the final values in the array the start height and height range need to be used by bilinearly interpolating between the four corners of each with the current x/y position in the array. It all comes together like:

//value = (height + noise - interpolated_start_height) * 4 / interpolated_height_range;

//That 's all there is to it basically. The rest is an exercise in texture compositing and interpolation. 

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
//using System.Drawing;
//using System.Drawing.Imaging;
//using Catnip.Drawing;
//using Catnip.Drawing.Imaging;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Imaging;
using Unity.Collections;
using UnityEngine;
//using Color = Catnip.Drawing.Color;
using Debug = System.Diagnostics.Debug;
using Vector3 = OpenMetaverse.Vector3;

namespace Raindrop.Rendering
{

    [StructLayout(LayoutKind.Explicit)]
    public struct Pixel
    {

        #region Data
        [FieldOffset(0)]
        public int rgba;

        [FieldOffset(0)]
        public byte r;

        [FieldOffset(1)]
        public byte g;

        [FieldOffset(2)]
        public byte b;

        [FieldOffset(3)]
        public byte a;
        #endregion
    }


    public static class TerrainSplat
    {
        #region Constants

        private static readonly UUID DIRT_DETAIL = new UUID("0bc58228-74a0-7e83-89bc-5c23464bcec5");
        private static readonly UUID GRASS_DETAIL = new UUID("63338ede-0037-c4fd-855b-015d77112fc8");
        private static readonly UUID MOUNTAIN_DETAIL = new UUID("303cd381-8560-7579-23f1-f0a880799740");
        private static readonly UUID ROCK_DETAIL = new UUID("53a2f406-4895-1d13-d541-d2e3b86bc19c");
        private static readonly int RegionSize = 256;

        private static readonly UUID[] DEFAULT_TERRAIN_DETAIL = new UUID[]
        {
            DIRT_DETAIL,
            GRASS_DETAIL,
            MOUNTAIN_DETAIL,
            ROCK_DETAIL
        };

        private static readonly Color32[] DEFAULT_TERRAIN_COLOR = new Color32[]
        {
            new Color32(164, 136, 117, 255),
            new Color32(65, 87, 47, 255),
            new Color32(157, 145, 131, 255),
            new Color32(125, 128, 130, 255)
        };

        private static readonly UUID TERRAIN_CACHE_MAGIC = new UUID("2c0c7ef2-56be-4eb8-aacb-76712c535b4b");

        #endregion Constants

        /// <summary>
        /// Builds a composited terrain texture given the region texture
        /// and heightmap settings
        /// </summary>
        /// <param name="instance">Raindrop Instance</param>
        /// <param name="heightmap">Terrain heightmap</param>
        /// <param name="textureIDs"></param>
        /// <param name="startHeights"></param>
        /// <param name="heightRanges"></param>
        /// <returns>A composited 256x256 RGB texture ready for rendering</returns>
        /// <remarks>Based on the algorithm described at http://opensimulator.org/wiki/Terrain_Splatting
        /// </remarks>
        public static Texture2D Splat(RaindropInstance instance, float[,] heightmap, UUID[] textureIDs, float[] startHeights, float[] heightRanges)
        {
            Debug.Assert(textureIDs.Length == 4);
            Debug.Assert(startHeights.Length == 4);
            Debug.Assert(heightRanges.Length == 4);
            int outputSize = 2048;

            Texture2D[] detailTexture = new Texture2D[4];

            // Swap empty terrain textureIDs with default IDs
            for (int i = 0; i < textureIDs.Length; i++)
            {
                if (textureIDs[i] == UUID.Zero)
                    textureIDs[i] = DEFAULT_TERRAIN_DETAIL[i];
            }

            #region Texture Fetching
            for (int i = 0; i < 4; i++)
            {
                AutoResetEvent textureDone = new AutoResetEvent(false);
                UUID textureID = textureIDs[i];

                instance.Client.Assets.RequestImage(textureID, TextureDownloadCallback(detailTexture, i, textureDone));

                textureDone.WaitOne(60 * 1000, false);
            }

            #endregion Texture Fetching

            // Fill in any missing textures with a solid color
            for (int i = 0; i < 4; i++)
            {
                if (detailTexture[i] == null)
                {
                    // Create a solid color texture for this layer
                    detailTexture[i] = new Texture2D(outputSize, outputSize);

                    fillcolor(detailTexture[i], DEFAULT_TERRAIN_COLOR[i]);
                    //detailTexture[i].fillColor(DEFAULT_TERRAIN_COLOR[i]);
                    //using (Graphics gfx = Graphics.FromImage(detailTexture[i]))
                    //{
                    //    using (SolidBrush brush = new SolidBrush(DEFAULT_TERRAIN_COLOR[i]))
                    //        gfx.FillRectangle(brush, 0, 0, outputSize, outputSize);
                    //}
                }
                else if (detailTexture[i].width != outputSize || detailTexture[i].height != outputSize)
                {
                    detailTexture[i] = ResizeTexture2D(detailTexture[i], 256, 256);
                }
            }

            #region Layer Map

            int diff = heightmap.GetLength(0) / RegionSize;
            float[] layermap = new float[RegionSize * RegionSize];

            for (int y = 0; y < heightmap.GetLength(0); y += diff)
            {
                for (int x = 0; x < heightmap.GetLength(1); x += diff)
                {
                    int newX = x / diff;
                    int newY = y / diff;
                    float height = heightmap[newX, newY];

                    float pctX = (float)newX / 255f;
                    float pctY = (float)newY / 255f;

                    // Use bilinear interpolation between the four corners of start height and
                    // height range to select the current values at this position
                    float startHeight = ImageUtils.Bilinear(
                        startHeights[0],
                        startHeights[2],
                        startHeights[1],
                        startHeights[3],
                        pctX, pctY);
                    startHeight = Utils.Clamp(startHeight, 0f, 255f);

                    float heightRange = ImageUtils.Bilinear(
                        heightRanges[0],
                        heightRanges[2],
                        heightRanges[1],
                        heightRanges[3],
                        pctX, pctY);
                    heightRange = Utils.Clamp(heightRange, 0f, 255f);

                    // Generate two frequencies of perlin noise based on our global position
                    // The magic values were taken from http://opensimulator.org/wiki/Terrain_Splatting
                    Vector3 vec = new Vector3
                    (
                        newX * 0.20319f,
                        newY * 0.20319f,
                        height * 0.25f
                    );

                    float lowFreq = Perlin.noise2(vec.X * 0.222222f, vec.Y * 0.222222f) * 6.5f;
                    float highFreq = Perlin.turbulence2(vec.X, vec.Y, 2f) * 2.25f;
                    float noise = (lowFreq + highFreq) * 2f;

                    // Combine the current height, generated noise, start height, and height range parameters, then scale all of it
                    float layer = ((height + noise - startHeight) / heightRange) * 4f;
                    if (Single.IsNaN(layer))
                        layer = 0f;
                    layermap[newY * RegionSize + newX] = Utils.Clamp(layer, 0f, 3f);
                }
            }

            #endregion Layer Map

            #region Texture Compositing
            //Texture2D output = new Texture2D(outputSize, outputSize, PixelFormat.Format24bppRgb);
            Texture2D output = new Texture2D(outputSize, outputSize);
            //Texture2DData outputData = output.LockBits(new Rectangle(0, 0, outputSize, outputSize), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            NativeArray<Pixel> outputData = output.GetRawTextureData<Pixel>();
            Pixel outputDataScan0 = outputData[0];
            unsafe
            {
                // Get handles to all of the texture data arrays
                //Texture2DData[] datas = new Texture2DData[]
                NativeArray<Pixel>[] datas = new NativeArray<Pixel>[]
                {
                    detailTexture[0].GetRawTextureData<Pixel>(),
                    detailTexture[1].GetRawTextureData<Pixel>(),
                    detailTexture[2].GetRawTextureData<Pixel>(),
                    detailTexture[3].GetRawTextureData<Pixel>(),

                    //detailTexture[0].LockBits(new Rectangle(0, 0, 256, 256), ImageLockMode.ReadOnly, detailTexture[0].PixelFormat),
                    //    detailTexture[1].LockBits(new Rectangle(0, 0, 256, 256), ImageLockMode.ReadOnly, detailTexture[1].PixelFormat),
                    //    detailTexture[2].LockBits(new Rectangle(0, 0, 256, 256), ImageLockMode.ReadOnly, detailTexture[2].PixelFormat),
                    //    detailTexture[3].LockBits(new Rectangle(0, 0, 256, 256), ImageLockMode.ReadOnly, detailTexture[3].PixelFormat)
                };

                int[] comps = new int[]
                {
                    4,
                    4,
                    4,
                    4 //lmao wtf 

                    //(datas[0].PixelFormat == PixelFormat.Format32bppArgb) ? 4 : 3,
                    //(datas[1].PixelFormat == PixelFormat.Format32bppArgb) ? 4 : 3,
                    //(datas[2].PixelFormat == PixelFormat.Format32bppArgb) ? 4 : 3,
                    //(datas[3].PixelFormat == PixelFormat.Format32bppArgb) ? 4 : 3
                };

                //int[] strides = new int[] //stride, aka scan-width (in bytes)
                //{
                //    datas[0].Stride,
                //    datas[1].Stride,
                //    datas[2].Stride,
                //    datas[3].Stride
                //};

                //IntPtr[] scans = new IntPtr[] //memoryAddr of the BMPs
                //{
                //    datas[0].Scan0,
                //    datas[1].Scan0,
                //    datas[2].Scan0,
                //    datas[3].Scan0
                //};

                int ratio = outputSize / RegionSize;

                //TODO terrain texture interpolating
                for (int y = 0; y < outputSize; y++)
                {
                    for (int x = 0; x < outputSize; x++)
                    {
                        float layer = layermap[(y / ratio) * RegionSize + x / ratio]; //grabs layermap value at x,y on the 2dgrid
                        float layerx = layermap[(y / ratio) * RegionSize + Math.Min(outputSize - 1, (x + 1)) / ratio]; //at x+1,y
                        float layerxx = layermap[(y / ratio) * RegionSize + Math.Max(0, (x - 1)) / ratio]; //at x-1,y
                        float layery = layermap[Math.Min(outputSize - 1, (y + 1)) / ratio * RegionSize + x / ratio]; //at x,y+1
                        float layeryy = layermap[(Math.Max(0, (y - 1)) / ratio) * RegionSize + x / ratio]; //at x,y-1

                        // Select two textures
                        int l0 = (int)Math.Floor(layer); //lowest texture-layer for the current point in layermap
                        int l1 = Math.Min(l0 + 1, 3);  //next-lowest texture-layer for the current point in layermap

                        //todo: placeholder here.
                        outputData[(y / ratio) * RegionSize + x / ratio] = datas[l0][(y / ratio) * RegionSize + x / ratio];

                        //        Pixel* ptrA = (Pixel*)scans[l0] + (y % 256) * strides[l0] + (x % 256) * comps[l0];
                        //        Pixel* ptrB = (Pixel*)scans[l1] + (y % 256) * strides[l1] + (x % 256) * comps[l1];
                        //        Pixel* ptrO = (Pixel*)outputData.Scan0 + y * outputData.Stride + x * 3;

                        //        byte aB = *byte (ptrA + 0);
                        //        float aG = *(ptrA + 1);
                        //        float aR = *(ptrA + 2);

                        //        int lX = (int)Math.Floor(layerx);
                        //        byte* ptrX = (byte*)scans[lX] + (y % 256) * strides[lX] + (x % 256) * comps[lX];
                        //        int lXX = (int)Math.Floor(layerxx);
                        //        byte* ptrXX = (byte*)scans[lXX] + (y % 256) * strides[lXX] + (x % 256) * comps[lXX];
                        //        int lY = (int)Math.Floor(layery);
                        //        byte* ptrY = (byte*)scans[lY] + (y % 256) * strides[lY] + (x % 256) * comps[lY];
                        //        int lYY = (int)Math.Floor(layeryy);
                        //        byte* ptrYY = (byte*)scans[lYY] + (y % 256) * strides[lYY] + (x % 256) * comps[lYY];

                        //        float bB = *(ptrB + 0);
                        //        float bG = *(ptrB + 1);
                        //        float bR = *(ptrB + 2);

                        //        float layerDiff = layer - l0;
                        //        float xlayerDiff = layerx - layer;
                        //        float xxlayerDiff = layerxx - layer;
                        //        float ylayerDiff = layery - layer;
                        //        float yylayerDiff = layeryy - layer;
                        //        // Interpolate between the two selected textures
                        //        *(ptrO + 0) = (byte)Math.Floor(aB + layerDiff * (bB - aB) +
                        //            xlayerDiff * (*ptrX - aB) +
                        //            xxlayerDiff * (*(ptrXX) - aB) +
                        //            ylayerDiff * (*ptrY - aB) +
                        //            yylayerDiff * (*(ptrYY) - aB));
                        //        *(ptrO + 1) = (byte)Math.Floor(aG + layerDiff * (bG - aG) +
                        //            xlayerDiff * (*(ptrX + 1) - aG) +
                        //            xxlayerDiff * (*(ptrXX + 1) - aG) +
                        //            ylayerDiff * (*(ptrY + 1) - aG) +
                        //            yylayerDiff * (*(ptrYY + 1) - aG));
                        //        *(ptrO + 2) = (byte)Math.Floor(aR + layerDiff * (bR - aR) +
                        //            xlayerDiff * (*(ptrX + 2) - aR) +
                        //            xxlayerDiff * (*(ptrXX + 2) - aR) +
                        //            ylayerDiff * (*(ptrY + 2) - aR) +
                        //            yylayerDiff * (*(ptrYY + 2) - aR));
                    }
                }

                    for (int i = 0; i < 4; i++)
                {
                    //seems like we dont need to dispose mem
                    //https://docs.unity3d.com/ScriptReference/Texture2D.GetRawTextureData.html
                    //detailTexture[i].UnlockBits(datas[i]);
                    //detailTexture[i].Dispose();
                }
            }

            layermap = null;
            //output.UnlockBits(outputData);

            //output.RotateFlip(RotateFlipType.Rotate270FlipNone); //why rotate?

            #endregion Texture Compositing
            //output = outputData;
            output.LoadRawTextureData(outputData);
            return output;
        }

        private static void fillcolor(Texture2D tex2, Color32 color)
        {
            //var fillColor : Color = Color(1, 0.0, 0.0);
            var fillColorArray = tex2.GetPixels();

            for (var i = 0; i < fillColorArray.Length; ++i)
            {
                fillColorArray[i] = color;
            }

            tex2.SetPixels(fillColorArray);

            tex2.Apply();

        }

        private static TextureDownloadCallback TextureDownloadCallback(Texture2D[] detailTexture, int i, AutoResetEvent textureDone)
        {
            return (state, assetTexture) =>
            {
                if (state == TextureRequestState.Finished && assetTexture?.AssetData != null)
                {
                    Texture2D img;
                    img = Imaging.LoadT2DWithoutMipMaps(assetTexture.AssetData);
                    detailTexture[i] = (Texture2D)img;
                }
                textureDone.Set();
            };
        }

        public static Texture2D ResizeTexture2D(Texture2D b, int nWidth, int nHeight)
        {
            //Texture2D newTex = new Texture2D(nWidth, nHeight);

            //Texture2D result = new Texture2D(nWidth, nHeight);
            //using (Graphics g = Graphics.FromImage((Image)result))
            //{
            //    g.DrawImage(b, 0, 0, nWidth, nHeight);
            //}
            //b.Dispose();
            b.Resize(nWidth,nHeight);
            //UnityEngine.Object.Destroy(b);
            return b;
        }

        //public static Texture2D TileTexture2D(Texture2D b, int tiles)
        //{
        //    Texture2D result = new Texture2D(b.Width * tiles, b.Width * tiles);
        //    using (Graphics g = Graphics.FromImage((Image)result))
        //    {
        //        for (int x = 0; x < tiles; x++)
        //        {
        //            for (int y = 0; y < tiles; y++)
        //            {
        //                g.DrawImage(b, x * 256, y * 256, x * 256 + 256, y * 256 + 256);
        //            }
        //        }
        //    }
        //    b.Dispose();
        //    return result;
        //}

        //public static Texture2D SplatSimple(float[,] heightmap)
        //{
        //    const float BASE_HSV_H = 93f / 360f;
        //    const float BASE_HSV_S = 44f / 100f;
        //    const float BASE_HSV_V = 34f / 100f;

        //    Texture2D img = new Texture2D(256, 256);
        //    Texture2DData texture2DData = img.LockBits(new Rectangle(0, 0, 256, 256), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

        //    unsafe
        //    {
        //        for (int y = 255; y >= 0; y--)
        //        {
        //            for (int x = 0; x < 256; x++)
        //            {
        //                float normHeight = heightmap[x, y] / 255f;
        //                normHeight = Utils.Clamp(normHeight, BASE_HSV_V, 1.0f);

        //                Color4 color = Color4.FromHSV(BASE_HSV_H, BASE_HSV_S, normHeight);

        //                byte* ptr = (byte*)texture2DData.Scan0 + y * texture2DData.Stride + x * 3;
        //                *(ptr + 0) = (byte)(color.B * 255f);
        //                *(ptr + 1) = (byte)(color.G * 255f);
        //                *(ptr + 2) = (byte)(color.R * 255f);
        //            }
        //        }
        //    }

        //    img.UnlockBits(Texture2DData);
        //    return img;
        //}
    }
}
