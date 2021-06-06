using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catnip.Drawing.Imaging
{
    //sealed=restrict the users from inheriting the class.
    public sealed class BitmapData
    {
        //Gets or sets the address of the first pixel data in the bitmap. This can also be thought of as the first scan line in the bitmap.
        public IntPtr Scan0 { get; set; }
        public int Stride { get; internal set; }
        public int Width { get; internal set; }
        public int Height { get; internal set; }
        public PixelFormat PixelFormat { get; internal set; }

        public BitmapData()
        {

        }

    }

    //https://docs.microsoft.com/en-us/dotnet/api/system.drawing.imaging.pixelformat?view=net-5.0
    public enum PixelFormat 
    {
        Alpha = 262144,
        Canonical= 2097152,
        DontCare =0,
        Format32bppArgb = 2498570,
        Format24bppRgb = 137224,
        Format16bppGrayScale = 2498571,
        Format32bppRgb = 139273,
        PAlpha = 2498572,
        Format32bppPArgb = 2498573,
    }
    public enum ImageLockMode
    {
        ReadOnly=1,
        ReadWrite=3,
        UserInputBuffer=4,
        WriteOnly=2
    }


}
