using Raindrop.Services.Bootstrap;
using UnityEngine;

namespace Raindrop.Unity
{
    // fixme:
    // this is kind of like a builder to generate a simple scene with bootstrapper.
    // I don't think we should use something so complicated
    public class SceneBootstrapperGenerator
    {
        public static GameObject mtd;
        public static void Init()
        {
            mtd = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mtd.AddComponent<RaindropBootstrapper>();

        }
        public static void AddMainThreadDispatcher()
        {
            mtd.AddComponent<UnityMainThreadDispatcher>();
        }

    }
}