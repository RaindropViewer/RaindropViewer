using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catnip.Drawing
{
    public sealed class Graphics : MarshalByRefObject, IDisposable, Catnip.Drawing.IDeviceContext
    {
        public object SmoothingMode { get; internal set; }
        public object InterpolationMode { get; internal set; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public static Graphics FromImage(Bitmap resized)
        {
            throw new NotImplementedException();
        }

        public void DrawImage(Bitmap bitmap, int v1, int v2, int width, int height)
        {
            throw new NotImplementedException();
        }
    }

    internal interface IDeviceContext
    {
    }
}
