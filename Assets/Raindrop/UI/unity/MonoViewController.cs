using UnityEngine;
using Raindrop.Netcom;

namespace Raindrop
{
    public class MonoViewController : MonoBehaviour
    {
        private RaindropNetcom netcom { get { return instance.Netcom; } }
        private RaindropInstance instance;

        //this guy flips pages.
        public CanvasManager CanvasManagerRef { get; private set; }
        
        //this is a page.
        private LoginVM LoginVMRef;


        void Awake()

        {
            //find canvasmanager in child.
            CanvasManagerRef = FindObjectOfType<CanvasManager>();

            //find all viewmodels in children.
            LoginVMRef = FindObjectOfType<LoginVM>();
            //GameVMRef = FindObjectOfType<GameVM>();

        }

    }
}