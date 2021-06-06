using Raindrop.Netcom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Raindrop.Rendering;

namespace Raindrop.Unity3D
{
    //sets the attached gameobject to the location of the user in the sim.
    class AgentLocationUpdater : MonoBehaviour
    {
        private RaindropInstance instance { get { return RaindropInstance.GlobalInstance; } }
        private RaindropNetcom netcom { get { return instance.Netcom; } }
        bool Active => instance.Client.Network.Connected;

        private void Awake()
        {
            
        }


        private void Update()
        {
            if (! Active)
            {
                return;
            }


            transform.position = (UnityEngine.Vector3)RHelp.TKVector3(instance.Client.Self.SimPosition); //convert OMV v3 to unity v3, so that we can move the object to the desired location lol.
            //transform.rotation = (UnityEngine.Vector3)RHelp.TKVector4(instance.Client.Self.SimRotation); 

        }

    }
}
