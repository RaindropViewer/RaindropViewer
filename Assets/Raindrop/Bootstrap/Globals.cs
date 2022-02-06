using System.Threading;

namespace Raindrop.Services.Bootstrap
{
    public class Globals
    {
        public static Thread GMainThread;
        
        public static bool isOnMainThread()
        {
            return Globals.GMainThread.Equals(System.Threading.Thread.CurrentThread);
        }
    }
}