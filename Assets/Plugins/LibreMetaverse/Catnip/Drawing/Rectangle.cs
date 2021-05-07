using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;

namespace Catnip.Drawing
{
    public struct Rectangle : IEquatable<Catnip.Drawing.Rectangle>
    {
        private SixLabors.ImageSharp.Rectangle inst;

        public Rectangle(int v1, int v2, int width, int height)
        {

            inst = new SixLabors.ImageSharp.Rectangle(v1,v2,width,height);
            
        }

        public bool Equals(Rectangle other)
        {
            throw new NotImplementedException();
        }
    }
}
