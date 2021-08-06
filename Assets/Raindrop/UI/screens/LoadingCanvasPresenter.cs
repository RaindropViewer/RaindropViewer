using Raindrop.Netcom;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Raindrop.Presenters
{
    public class LoadingCanvasPresenter : MonoBehaviour
    {
        private RaindropInstance instance { get { return ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>(); } }
        private RaindropNetcom netcom { get { return instance.Netcom; } }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
