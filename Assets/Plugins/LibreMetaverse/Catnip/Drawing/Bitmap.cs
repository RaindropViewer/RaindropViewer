using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Catnip.Drawing;
using Catnip.Drawing.Imaging;
using SixLabors.ImageSharp;
using UnityEngine;


//fake image class that actually just calls unity image internally.
namespace Catnip.Drawing
{
    //use this as the template for call to texture.GetRawTextureData<T>()
    //https://forum.unity.com/threads/2019-2-dots-image-processing-performance.729314/
    [StructLayout(LayoutKind.Explicit)]
    public struct Pixel
    {

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
    }


    public sealed class Bitmap //: Catnip.Drawing.Image
    {
        private Texture2D tex;
        private TextureFormat texformat;

        //Initializes a new instance of the Bitmap class with the specified size and format.
        public Bitmap(int width, int height, PixelFormat format32bppArgb)
        {
            Width = width;
            Height = height;
            if (format32bppArgb == PixelFormat.Format32bppArgb)
            {
                texformat = TextureFormat.ARGB32;
            }
        }

        public Bitmap(int width, int height)
        {
            Width = width;
            Height = height;


        }

        public /*override*/ int Width { get; internal set; }
        public /*override*/ int Height { get; internal set; }
        public Catnip.Drawing.Imaging.PixelFormat PixelFormat { get; internal set; }

        public BitmapData LockBits(Rectangle rectangle, object readOnly, PixelFormat format32bppArgb)
        {

            throw new NotImplementedException();
        }


        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void UnlockBits(BitmapData outputData)
        {
            throw new NotImplementedException();
        }

        public void RotateFlip(object rotate270FlipNone)
        {
            //rotate the texture.
            //tex.GetRawTextureData<Pixel>()

            throw new NotImplementedException();
        }

        public void fillColor(Catnip.Drawing.Color color)
        {
            var fillColor  = color;
            var fillColorArray = tex.GetPixels();

            for (var i = 0; i < fillColorArray.Length; ++i)
            {
                fillColorArray[i] = fillColor;
            }

            tex.SetPixels(fillColorArray);
            //applies the setPixel changes.
            tex.Apply();
        }

        public static Bitmap FromFile(string fname)
        {
            var fileData = File.ReadAllBytes(fname);
            var bmp = new Bitmap(5,5);
            bmp.tex.LoadImage(fileData);

            return bmp;
        }
    }
}
