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
using System.IO;
//using System.Drawing;
//using System.Drawing.Imaging;
//using Catnip.Drawing;
//using Catnip.Drawing.Imaging;
using Unity.Collections;
using UnityEngine;
//using UnityEngine;

namespace OpenMetaverse.Imaging
{
#if !NO_UNSAFE

    /// <summary>
    /// Capability to load TGAs to Bitmap 
    /// </summary>
    public class LoadTGAClass
    {
        struct tgaColorMap
        {
            public ushort FirstEntryIndex;
            public ushort Length;
            public byte EntrySize;

            public void Read(System.IO.BinaryReader br)
            {
                FirstEntryIndex = br.ReadUInt16();
                Length = br.ReadUInt16();
                EntrySize = br.ReadByte();
            }
        }

        struct tgaImageSpec
        {
            public ushort XOrigin;
            public ushort YOrigin;
            public ushort Width;
            public ushort Height;
            public byte PixelDepth;
            public byte Descriptor;

            public void Read(System.IO.BinaryReader br)
            {
                XOrigin = br.ReadUInt16();
                YOrigin = br.ReadUInt16();
                Width = br.ReadUInt16();
                Height = br.ReadUInt16();
                PixelDepth = br.ReadByte();
                Descriptor = br.ReadByte();
            }

            public byte AlphaBits
            {
                get
                {
                    return (byte)(Descriptor & 0xF);
                }
                set
                {
                    Descriptor = (byte)((Descriptor & ~0xF) | (value & 0xF));
                }
            }

            public bool BottomUp
            {
                get
                {
                    return (Descriptor & 0x20) == 0x20;
                }
                set
                {
                    Descriptor = (byte)((Descriptor & ~0x20) | (value ? 0x20 : 0));
                }
            }
        }

        struct tgaHeader
        {
            public byte IdLength;
            public byte ColorMapType;
            public byte ImageType;

            public tgaColorMap ColorMap;
            public tgaImageSpec ImageSpec;

            public void Read(System.IO.BinaryReader br)
            {
                this.IdLength = br.ReadByte();
                this.ColorMapType = br.ReadByte();
                this.ImageType = br.ReadByte();
                this.ColorMap = new tgaColorMap();
                this.ImageSpec = new tgaImageSpec();
                this.ColorMap.Read(br);
                this.ImageSpec.Read(br);
            }

            public bool RleEncoded
            {
                get
                {
                    return ImageType >= 9;
                }
            }
        }
        
        struct tgaCD
        {
            public uint RMask, GMask, BMask, AMask;
            public byte RShift, GShift, BShift, AShift;
            public uint FinalOr;
            public bool NeedNoConvert;
        }


        static uint UnpackColor(
           uint sourceColor, ref tgaCD cd)
        {
            if (cd.RMask == 0xFF && cd.GMask == 0xFF && cd.BMask == 0xFF)
            {
                // Special case to deal with 8-bit TGA files that we treat as alpha masks
                return sourceColor << 24;
            }
            else
            {
                uint rpermute = (sourceColor << cd.RShift) | (sourceColor >> (32 - cd.RShift));
                uint gpermute = (sourceColor << cd.GShift) | (sourceColor >> (32 - cd.GShift));
                uint bpermute = (sourceColor << cd.BShift) | (sourceColor >> (32 - cd.BShift));
                uint apermute = (sourceColor << cd.AShift) | (sourceColor >> (32 - cd.AShift));
                uint result =
                    (rpermute & cd.RMask) | (gpermute & cd.GMask)
                    | (bpermute & cd.BMask) | (apermute & cd.AMask) | cd.FinalOr;

                return result;
            }
        }

        static unsafe void decodeLine(
          Color32[] b, int texWidth, int texHeight,
          int line,
          int byp,
          byte[] data,
          ref tgaCD cd)
        {
            if (cd.NeedNoConvert)
            {
                // fast copy
                uint offset_colorArray_scanline = (uint)(line * texWidth + 0); //should be large enough?
                //uint* linep = (uint*)((byte*)b.Scan0.ToPointer() + line * b.Stride);
                fixed (byte* ptr = data)
                {
                    uint* sptr = (uint*)ptr;
                    for (int i = 0; i < texWidth; ++i)
                    {
                        b[i + offset_colorArray_scanline] = makeColor32fromUInt(sptr[i]);
                    }
                }
            }
            else
            {
                //byte* linep = (byte*)b.Scan0.ToPointer() + line * b.Stride;
                uint offset_colorArray_scanline = (uint)(line * texWidth + 0);

                //uint* up = (uint*)linep;

                int rdi = 0;

                fixed (byte* ptr = data)
                {
                    for (int i = 0; i < texWidth; ++i)
                    {
                        uint x = 0;
                        for (int j = 0; j < byp; ++j)
                        {
                            x |= ((uint)ptr[rdi]) << (j << 3);  //load all bytes that represent the pixel's color into register x.
                            ++rdi;
                        }
                        uint unpackedColoByte = UnpackColor(x, ref cd);
                        b[i+line*texWidth] = makeColor32fromUInt(unpackedColoByte);
                    }
                }
            }
        }


        static void decodeRle(
               Color32[] b, int texWidth, int texHeight,
               int byp, tgaCD cd, System.IO.BinaryReader br, bool bottomUp)
        {
            try
            {
                int w = texWidth;
                // make buffer larger, so in case of emergency I can decode 
                // over line ends.
                byte[] linebuffer = new byte[(w + 128) * byp];
                int maxindex = w * byp;
                int index = 0;

                for (int j = 0; j < texHeight; ++j)
                {
                    while (index < maxindex)
                    {
                        byte blocktype = br.ReadByte(); //MSB of blocktype: 1 means Raw packet(non-RLE), 0 means RLE packet

                        int bytestoread;
                        int bytestocopy;

                        if (blocktype >= 0x80)  // run-length packet. - read 1 color and replicate them by bytestocopy
                        {
                            int pixel_count_minus_1 = (blocktype - 0x80);
                            bytestoread = byp;  // pixel data
                            bytestocopy = byp * pixel_count_minus_1; 
                        }
                        else //raw packet (non run-lenght encoding.)
                        {
                            bytestoread = byp * (blocktype + 1);
                            bytestocopy = 0;
                        }

                        //if (index + bytestoread > maxindex)
                        //	throw new System.ArgumentException ("Corrupt TGA");

                        br.Read(linebuffer, index, bytestoread);
                        index += bytestoread;

                        for (int i = 0; i != bytestocopy; ++i)
                        {
                            linebuffer[index + i] = linebuffer[index + i - bytestoread];
                        }
                        index += bytestocopy;
                    }
					// todo: did i wrongly flip the orientation here? is the original actually correct?
                    if (!bottomUp)
                        decodeLine(b, texWidth, texHeight, j, byp, linebuffer, ref cd);
                    else
                        decodeLine(b, texWidth, texHeight, texHeight - j - 1, byp, linebuffer, ref cd);

                    if (index > maxindex)
                    {
                        Array.Copy(linebuffer, maxindex, linebuffer, 0, index - maxindex);
                        index -= maxindex;
                    }
                    else
                        index = 0;

                }
            }
            catch (System.IO.EndOfStreamException)
            {
            }
        }



        /// <summary>
        ///  decodes the non-RLE version of tga files. 
        /// </summary>
        /// <param name="b">output reference to texture/bmp</param>
        /// <param name="byp">bytes-per-pixel</param>
        /// <param name="cd">???</param>
        /// <param name="br">binary reader that is reading the data file </param>
        /// <param name="bottomUp">if the image sh. </param>
        static void decodePlain(
                Color32[] b, int texWidth, int texHeight,
                int byp, tgaCD cd, System.IO.BinaryReader br, bool bottomUp)
        {
            int w = texWidth;
            byte[] linebuffer = new byte[w * byp];

            for (int j = 0; j < texHeight; ++j)
            {
                br.Read(linebuffer, 0, w * byp);
				
				// todo: did i wrongly flip the orientation here? is the original actually correct?    
                if (!bottomUp)
                    decodeLine(b, texWidth, texHeight, j, byp, linebuffer, ref cd);
                else
                    decodeLine(b, texWidth, texHeight, texHeight - j - 1, byp, linebuffer, ref cd);
            }
        }
		

        static void decodeStandard8(
           Color32[] b, int texWidth, int texHeight,
           tgaHeader hdr,
           System.IO.BinaryReader br)
        {
            tgaCD cd = new tgaCD();
            cd.RMask = 0x000000ff;
            cd.GMask = 0x000000ff;
            cd.BMask = 0x000000ff;
            cd.AMask = 0x000000ff;
            cd.RShift = 0;
            cd.GShift = 0;
            cd.BShift = 0;
            cd.AShift = 0;
            cd.FinalOr = 0x00000000;
            if (hdr.RleEncoded)
                decodeRle(b, texWidth, texHeight, 1, cd, br, hdr.ImageSpec.BottomUp);
            else
                decodePlain(b, texWidth, texHeight, 1, cd, br, hdr.ImageSpec.BottomUp);
        }

        static void decodeSpecial16(
            Color32[] b, int texWidth, int texHeight, tgaHeader hdr, System.IO.BinaryReader br)
        {
            // i must convert the input stream to a sequence of uint values
            // which I then unpack.
            tgaCD cd = new tgaCD();
            cd.RMask = 0x00f00000;
            cd.GMask = 0x0000f000;
            cd.BMask = 0x000000f0;
            cd.AMask = 0xf0000000;
            cd.RShift = 12;
            cd.GShift = 8;
            cd.BShift = 4;
            cd.AShift = 16;
            cd.FinalOr = 0;

            if (hdr.RleEncoded)
                decodeRle(b, texWidth, texHeight, 2, cd, br, hdr.ImageSpec.BottomUp);
            else
                decodePlain(b, texWidth, texHeight, 2, cd, br, hdr.ImageSpec.BottomUp);
        }

        static void decodeStandard16(
            Color32[] b, int texWidth, int texHeight,
            tgaHeader hdr,
            System.IO.BinaryReader br)
        {
            // i must convert the input stream to a sequence of uint values
            // which I then unpack.
            tgaCD cd = new tgaCD();
            cd.RMask = 0x00f80000;	// from 0xF800
            cd.GMask = 0x0000fc00;	// from 0x07E0
            cd.BMask = 0x000000f8;  // from 0x001F
            cd.AMask = 0x00000000;
            cd.RShift = 8;
            cd.GShift = 5;
            cd.BShift = 3;
            cd.AShift = 0;
            cd.FinalOr = 0xff000000;

            if (hdr.RleEncoded)
                decodeRle(b, texWidth, texHeight, 2, cd, br, hdr.ImageSpec.BottomUp);
            else
                decodePlain(b, texWidth, texHeight, 2, cd, br, hdr.ImageSpec.BottomUp);
        }


        static void decodeSpecial24(Color32[] b, int texWidth, int texHeight,
            tgaHeader hdr, System.IO.BinaryReader br)
        {
            // i must convert the input stream to a sequence of uint values
            // which I then unpack.
            tgaCD cd = new tgaCD();
            cd.RMask = 0x00f80000;
            cd.GMask = 0x0000fc00;
            cd.BMask = 0x000000f8;
            cd.AMask = 0xff000000;
            cd.RShift = 8;
            cd.GShift = 5;
            cd.BShift = 3;
            cd.AShift = 8;
            cd.FinalOr = 0;

            if (hdr.RleEncoded)
                decodeRle(b, texWidth, texHeight, 3, cd, br, hdr.ImageSpec.BottomUp);
            else
                decodePlain(b, texWidth, texHeight, 3, cd, br, hdr.ImageSpec.BottomUp);
        }

        static void decodeStandard24(Color32[] b, int texWidth, int texHeight,
            tgaHeader hdr, System.IO.BinaryReader br)
        {
            // i must convert the input stream to a sequence of uint values
            // which I then unpack.
            tgaCD cd = new tgaCD();
            cd.RMask = 0x00ff0000;
            cd.GMask = 0x0000ff00;
            cd.BMask = 0x000000ff;
            cd.AMask = 0x00000000;
            cd.RShift = 0;
            cd.GShift = 0;
            cd.BShift = 0;
            cd.AShift = 0;
            cd.FinalOr = 0xff000000;

            if (hdr.RleEncoded)
                decodeRle(b, texWidth, texHeight, 3, cd, br, hdr.ImageSpec.BottomUp);
            else
                decodePlain(b, texWidth, texHeight, 3, cd, br, hdr.ImageSpec.BottomUp);
        }

        static void decodeStandard32(Color32[] b, int texWidth, int texHeight,
            tgaHeader hdr, System.IO.BinaryReader br)
        {
            // i must convert the input stream to a sequence of uint values
            // which I then unpack.
            tgaCD cd = new tgaCD();
            cd.RMask = 0x00ff0000;
            cd.GMask = 0x0000ff00;
            cd.BMask = 0x000000ff;
            cd.AMask = 0xff000000;
            cd.RShift = 0;
            cd.GShift = 0;
            cd.BShift = 0;
            cd.AShift = 0;
            cd.FinalOr = 0x00000000;
            cd.NeedNoConvert = true;

            if (hdr.RleEncoded)
                decodeRle(b, texWidth, texHeight, 4, cd, br, hdr.ImageSpec.BottomUp);
            else
                decodePlain(b, texWidth, texHeight, 4, cd, br, hdr.ImageSpec.BottomUp);
        }

        //load the tga and get a texture using a TGA byte array.
        public static Texture2D LoadTGA(byte[] source)
        {

            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            stream.Write(source, 0, source.Length);
            stream.Seek(0, SeekOrigin.Begin); //FUCKKK

            return LoadTGA(stream); // better refactor this mess later

        }

        //Decode TGA file given in stream form.
        public static Texture2D LoadTGA(System.IO.Stream source)
        {
            //byte[] buffer = new byte[source.Length];
            //int _readbytes = source.Read(buffer, 0, buffer.Length);

            //System.IO.MemoryStream ms = new System.IO.MemoryStream(buffer);

            using (System.IO.BinaryReader br = new System.IO.BinaryReader(source))
            {
                tgaHeader header = new tgaHeader();
                header.Read(br);

                if (header.ImageSpec.PixelDepth != 8 &&
                    header.ImageSpec.PixelDepth != 16 &&
                    header.ImageSpec.PixelDepth != 24 &&
                    header.ImageSpec.PixelDepth != 32)
                    throw new ArgumentException("Not a supported tga file.");

                //if (header.ImageSpec.PixelDepth == 8)
                //    throw new ArgumentException("TGA texture had 8 bit depth.");
                if (header.ImageSpec.PixelDepth == 16)
                    throw new ArgumentException("TGA texture had 16 bit depth.");

                if (header.ImageSpec.AlphaBits > 8)
                    throw new ArgumentException("Not a supported tga file: too many Alpha bits");

                if (header.ImageSpec.Width > 4096 ||
                    header.ImageSpec.Height > 4096)
                    throw new ArgumentException("Image too large.");


                //long len = br.BaseStream.Length;
                //if (header.ImageSpec.PixelDepth == 24)
                //{
                //    long expectedBytes = header.ImageSpec.Width * header.ImageSpec.Height * 3;
                //    if (len < expectedBytes)
                //    {
                //        //throw new ArgumentException("TGA texture has 24 bit depth, height and width of " +header.ImageSpec.Width+ " " + header.ImageSpec.Height+ " but the number of bytes in file is " + len);
                //        throw new ArgumentException("the 24bit TGA file is smaller than expected");
                //    }

                //}
                //if (header.ImageSpec.PixelDepth == 32)
                //{
                //    long expectedBytes = header.ImageSpec.Width * header.ImageSpec.Height * 4;
                //    if (len < expectedBytes)
                //    {
                //        //throw new ArgumentException("TGA texture has 32 bit depth, height and width of " +header.ImageSpec.Width+ " " + header.ImageSpec.Height+ " but the number of bytes in file is " + len);
                //        throw new ArgumentException("the 32bit TGA file is smaller than expected");
                //    }

                //}
                //if (header.ImageSpec.PixelDepth == 8)
                //{
                //    long expectedBytes = header.ImageSpec.Width * header.ImageSpec.Height * 1;
                //    if (len < expectedBytes)
                //    {
                //        //throw new ArgumentException("TGA texture has 32 bit depth, height and width of " +header.ImageSpec.Width+ " " + header.ImageSpec.Height+ " but the number of bytes in file is " + len);
                //        throw new ArgumentException("the 8bit TGA file is smaller than expected");
                //    }

                //}


                Texture2D b;
                int width = header.ImageSpec.Width;
                int height = header.ImageSpec.Height;
                Color32[] pulledColors = new Color32[width * height];

                //should support compressed texture too!

                // Create a bitmap for the image.
                // Only include an alpha layer when the image requires one.
                if (header.ImageSpec.AlphaBits > 0 ||
                    header.ImageSpec.PixelDepth == 8 || // Assume  8 bit images are alpha only
                    header.ImageSpec.PixelDepth == 32)	// Assume 32 bit images are ARGB
                {   // Image needs an alpha layer
                    b = new Texture2D(
                        header.ImageSpec.Width,
                        header.ImageSpec.Height,
                        TextureFormat.ARGB32,
                        false
                        /*PixelFormat.Format32bppArgb*/); 

                    //Debug.Log("Debug the color: " + pulledColors[5]);  //ok we know these are correct.
                    //Debug.Log("Debug the color: " + pulledColors[500]);
                    //Debug.Log("Debug the color: " + pulledColors[50000]);
                }
                else
                {   // Image does not need an alpha layer, so do not include one.
                    b = new Texture2D(
                        header.ImageSpec.Width,
                        header.ImageSpec.Height,
                        TextureFormat.RGB24,
                        false
                        /*PixelFormat.Format32bppRgb*/);

                }

                switch (header.ImageSpec.PixelDepth)
                {
                    case 8:
                        decodeStandard8(pulledColors, width,height, header, br);
                        break;
                    case 16:
                        if (header.ImageSpec.AlphaBits > 0)
                            decodeSpecial16(pulledColors, width, height, header, br);
                        else
                            decodeStandard16(pulledColors, width, height, header, br);
                        break;
                    case 24:
                        if (header.ImageSpec.AlphaBits > 0)
                            decodeSpecial24(pulledColors, width, height, header, br);
                        else
                            decodeStandard24(pulledColors, width, height, header, br);
                        break;
                    case 32:
                        decodeStandard32(pulledColors, width, height, header, br);
                        break;
                    default:
                        //b.UnlockBits(bd);
                        //b.Dispose();
                        return null;
                }

                b.SetPixels32(pulledColors);
                b.Apply();

                return b;
            }
        }

        public static Texture2D LoadTGA(string filename)
        {
            try
            {
                using (System.IO.FileStream f = System.IO.File.OpenRead(filename))
                {
                    if (f != null)
                    {
                        return LoadTGA(f);
                    }
                }
                return null; // file stream error
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                return null;	// file not found
            }
            catch (System.IO.FileNotFoundException)
            {
                return null; // file not found
            }
        }



        static public Color32 makeColor32fromUInt(uint x)
        {
            return new Color32( (byte)( (x & 0x00FF0000) >> 16),
                                (byte)( (x & 0x0000FF00) >> 8),
                                (byte)(( x & 0x000000FF) >> 0),
                                (byte)((x & 0xFF000000) >> 24)); //alpha
        }

    }

#endif
}


//using System;
//using System.IO;
//using UnityEngine;
//public static class TGALoader
//{
//    // Loads 32-bit (RGBA) uncompressed TGA. Actually, due to TARGA file structure, BGRA32 is good option...
//    // Disabled mipmaps. Disabled read/write option, to release texture memory copy.
//    public static Texture2D LoadTGA(string fileName)
//    {
//        try
//        {
//            BinaryReader reader = new BinaryReader(File.OpenRead(fileName));
//            reader.BaseStream.Seek(12, SeekOrigin.Begin);
//            short width = reader.ReadInt16();
//            short height = reader.ReadInt16();
//            reader.BaseStream.Seek(2, SeekOrigin.Current);
//            byte[] source = reader.ReadBytes(width * height * 4);
//            reader.Close();
//            Texture2D texture = new Texture2D(width, height, TextureFormat.BGRA32, false);
//            texture.LoadRawTextureData(source);
//            texture.name = Path.GetFileName(fileName);
//            texture.Apply(false, true);
//            return texture;
//        }
//        catch (Exception)
//        {
//            return Texture2D.blackTexture;
//        }
//    }


//    public static Texture2D LoadTGA(Stream TGAStream)
//    {

//        using (BinaryReader r = new BinaryReader(TGAStream))
//        {
//            // Skip some header info we don't care about.
//            // Even if we did care, we have to move the stream seek point to the beginning,
//            // as the previous method in the workflow left it at the end.
//            r.BaseStream.Seek(12, SeekOrigin.Begin);

//            short width = r.ReadInt16();
//            short height = r.ReadInt16();
//            int bitDepth = r.ReadByte();

//            // Skip a byte of header information we don't care about.
//            r.BaseStream.Seek(1, SeekOrigin.Current);

//            Texture2D tex = new Texture2D(width, height);
//            Color32[] pulledColors = new Color32[width * height];

//            if (bitDepth == 32)
//            {
//                for (int i = 0; i < width * height; i++)
//                {
//                    byte red = r.ReadByte();
//                    byte green = r.ReadByte();
//                    byte blue = r.ReadByte();
//                    byte alpha = r.ReadByte();

//                    pulledColors[i] = new Color32(blue, green, red, alpha);
//                }
//            }
//            else if (bitDepth == 24)
//            {
//                for (int i = 0; i < width * height; i++)
//                {
//                    byte red = r.ReadByte();
//                    byte green = r.ReadByte();
//                    byte blue = r.ReadByte();

//                    pulledColors[i] = new Color32(blue, green, red, 1);
//                }
//            }
//            else
//            {
//                throw new Exception("TGA texture had non 32/24 bit depth.");
//            }

//            tex.SetPixels32(pulledColors);
//            tex.Apply();
//            return tex;

//        }
//    }

//}
