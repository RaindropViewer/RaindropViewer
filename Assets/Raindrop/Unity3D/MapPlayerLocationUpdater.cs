using Raindrop.Netcom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Raindrop.Rendering;
using OpenMetaverse;
using Raindrop.Utilities;
using UnityEngine.Assertions;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = OpenMetaverse.Vector3;

namespace Raindrop.Unity3D
{
    // Attach to minimap player to set its location in mapspace.
    // see @ MapCoordinatesConversionTests
    class MapPlayerLocationUpdater : MonoBehaviour
    {
        private RaindropInstance instance { get { return ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>(); } }
        private RaindropNetcom netcom { get { return instance.Netcom; } }
        bool Active => instance.Client.Network.Connected;

        //List<UUID> agents;
        public GameObject agentPrefab;
        Dictionary<UUID, GameObject> agents;
        public GameObject agent;
        //the height at which all map items are at (unity y axis.)
        private readonly float mapItemDepthConstant = 10;

        private void Update()
        {
            if (! Active)
            {
                return;
            }

            var mapSpace = 
                MapSpaceConverters.GlobalSpaceToMapSpace(instance.Client.Self.GlobalPosition);
            
            UnityEngine.Quaternion rotation_InScene = RHelp.TKQuaternion4(instance.Client.Self.SimRotation);
            var heading_UE = rotation_InScene.eulerAngles.y;
            UnityEngine.Vector3 orientationInMapSpace =
                Quaternion.Euler(0, 0, heading_UE) * UnityEngine.Vector3.up;

            MapSpaceSetters.SetMapItemPosition(agent.transform, mapSpace);
            MapSpaceSetters.SetMapItemRotation(agent.transform, orientationInMapSpace);
            
        }

    }
}
