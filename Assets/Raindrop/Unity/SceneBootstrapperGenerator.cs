using Raindrop.Services.Bootstrap;
using UnityEngine;

namespace Raindrop.Unity
{
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