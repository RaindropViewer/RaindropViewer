using System.Threading;
using UnityEngine;

namespace Raindrop.Services.Bootstrap
{
    public class Globals : MonoBehaviour
    {
        public static Thread GMainThread;
        
        public static bool isOnMainThread()
        {
            return Globals.GMainThread.Equals(System.Threading.Thread.CurrentThread);
        }

    }
}