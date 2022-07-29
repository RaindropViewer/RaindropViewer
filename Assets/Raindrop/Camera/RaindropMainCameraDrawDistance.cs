using OpenMetaverse;
using Plugins.CommonDependencies;
using UnityEngine;

namespace Raindrop.Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class RaindropMainCameraDrawDistance : MonoBehaviour
    {
        public float DrawDistance;
        // private UIService ui => ServiceLocator.Instance.Get<UIService>();
        private RaindropInstance Instance => RaindropInstance.GlobalInstance;
        private GridClient Client => Instance.Client;

        //private bool Active => ui.ScreensManager.TopCanvas.canvasType == CanvasType.Game;

        // Start is called before the first frame update
        void Start()
        {
            if (!Instance.GlobalSettings.ContainsKey("draw_distance"))
            {
                Instance.GlobalSettings["draw_distance"] = DrawDistance;
            }

            this.GetComponent<UnityEngine.Camera>().farClipPlane = DrawDistance;
        }

        void Update()
        {
            //if (!Active)
            //{
            //    return;
            //}

        }
    }
}
