using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;


//fake image class that actually just calls unity image internally.
namespace Catnip.Drawing
{
    public abstract class Image
    {

        public abstract int Width { get; internal set; }
        public abstract int Height { get; internal set; }

        internal static Bitmap FromFile(string fname)
        {
            throw new NotImplementedException();
        }
    }
}
