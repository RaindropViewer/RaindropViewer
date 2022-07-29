using Raindrop.Netcom;
using System.Collections.Generic;
using UnityEngine;
using Raindrop.Rendering;
using OpenMetaverse;
using Plugins.CommonDependencies;
using Raindrop.Utilities;
using Quaternion = UnityEngine.Quaternion;

namespace Raindrop.Unity3D
{
    // Attach to minimap player to set its location in mapspace.
    // see @ MapCoordinatesConversionTests
    class MapPlayerLocationUpdater : MonoBehaviour
    {
        private RaindropInstance instance { get { return RaindropInstance.GlobalInstance; } }
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

            var position = 
                MapSpaceConverters.GlobalSpaceToMapSpace(instance.Client.Self.GlobalPosition);
            
            // UnityEngine.Quaternion orientation_in3D = RHelp.TKQuaternion4(instance.Client.Self.SimRotation);
            UnityEngine.Quaternion orientation_mapspace = MapSpaceConverters.GlobalRot2MapRot(instance.Client.Self.SimRotation);
            // var headingDeg = - orientation.eulerAngles.y; //invert rotation as y is upward axis.

            MapSpaceSetters.SetMapItemPosition(agent.transform, position);
            MapSpaceSetters.SetMapItemOrientation(agent.transform, orientation_mapspace);
            
        }

    }
}
