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
using Unity.Collections;
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


    public sealed class Bitmap 
    {
        private Texture2D tex;
        public TextureFormat Format => tex.format;

        //Initializes a new instance of the Bitmap class with the specified size and format.
        public Bitmap(int width, int height, TextureFormat format)
        {
            Width = width;
            Height = height;
            tex = new Texture2D(width,height,format,false);
        }



        public Bitmap(int width, int height)
        {
            Width = width;
            Height = height;
            tex = new Texture2D(width,height);
            
        }
        public Bitmap(Texture2D tex)
        {
            Width = tex.width;
            Height = tex.height;
            this.tex = tex;


        }


        public NativeArray<Color32> getAsNativeArray()
        {
            return tex.GetRawTextureData<Color32>();
        }

        public Bitmap FromFile(string fileName)
        {
            var myreader = new BMPLoader();
            BMPImage myimg = myreader.LoadBMP(fileName);
            Texture2D tex = myimg.ToTexture2D();
            Bitmap fakebmp = new Bitmap(tex);

            return fakebmp;
        }
        public UnityEngine.Color32 GetPixel(int x, int y)
        {
            return tex.GetPixel(x, y);
        }

        public void resize(int w, int h)
        {
            tex.Resize(w, h);
        }

        public void delete()
        {
            GameObject.Destroy(tex);
        }

        public /*override*/ int Width { get; internal set; }
        public /*override*/ int Height { get; internal set; }  

        public BitmapData LockBits(Rectangle rectangle, object readOnly, PixelFormat format32bppArgb)
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

    }
}
