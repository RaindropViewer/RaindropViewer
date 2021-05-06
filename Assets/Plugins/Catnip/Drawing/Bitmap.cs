using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catnip.Drawing;
using Catnip.Drawing.Imaging;
using SixLabors.ImageSharp;


//fake image class that actually just calls unity image internally.
namespace Catnip.Drawing
{
    public sealed class Bitmap : Catnip.Drawing.Image
    {
        //Initializes a new instance of the Bitmap class with the specified size and format.
        public Bitmap(int width, int height, PixelFormat format32bppArgb)
        {
            Width = width;
            Height = height;
        }

        public override int Width { get; internal set; }
        public override int Height { get; internal set; }
        public Catnip.Drawing.Imaging.PixelFormat PixelFormat { get; internal set; }

        public BitmapData LockBits(Rectangle rectangle, object readOnly, PixelFormat format32bppArgb)
        {
            throw new NotImplementedException();
        }

        internal void UnlockBits(BitmapData bd)
        {
            throw new NotImplementedException();
        }

        internal void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
