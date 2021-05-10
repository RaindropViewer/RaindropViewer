using UnityEngine;
using Raindrop.Netcom;

namespace Raindrop
{
    public class RaindropViewControl:MonoBehaviour
    {
        private RaindropNetcom netcom { get { return instance.Netcom; } }
        private RaindropInstance instance;
        public CanvasManager CanvasManagerRef { get; private set; }

        private LoginVM LoginVMRef;

        public RaindropViewControl()
        {
            //find canvasmanager in child.
            CanvasManagerRef = FindObjectOfType<CanvasManager>();

            //find all viewmodels in children.
            LoginVMRef = FindObjectOfType<LoginVM>();
            //GameVMRef = FindObjectOfType<GameVM>();
        }

    }
}