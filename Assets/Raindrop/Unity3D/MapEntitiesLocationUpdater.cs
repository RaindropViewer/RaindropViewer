using Raindrop.Netcom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Raindrop.Rendering;
using OpenMetaverse;

namespace Raindrop.Unity3D
{
    // sets the attached gameobject to the location of the user in the sim.
    
    // the location of the mapEntities in the scene hierachy is defined as 
    // "global" coordinates / 256 - that is, an entity existing in the sim Daboom (1000, 1000)
    // at the sim coordinates 0,0,0, will be shown at the Unityscene position of
    // (1000,1000, mapItemDepthConstant) 
    class MapEntitiesLocationUpdater : MonoBehaviour
    {
        private RaindropInstance instance { get { return ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>(); } }
        private RaindropNetcom netcom { get { return instance.Netcom; } }
        bool Active => instance.Client.Network.Connected;

        List<UUID> agents;
        public GameObject agent;
        //the height at which all map items are at (unity y axis.)
        private readonly float mapItemDepthConstant = 10;

        private void Update()
        {
            if (! Active)
            {
                return;
            }

            //instance.Client.Grid.CoarseLocationUpdate += Grid_CoarseLocationUpdate;

            //UnityEngine.Vector3 agentSimLoc = (UnityEngine.Vector3)RHelp.TKVector3(instance.Client.Self.SimPosition); //convert OMV v3 to unity v3, so that we can move the object to the desired location lol.
            UnityEngine.Vector3 agentGridLoc = (UnityEngine.Vector3)RHelp.TKVector3d(instance.Client.Self.GlobalPosition);
            UnityEngine.Vector2 mapPos = toMapPlaneCoords(agentGridLoc);// + toMapPlaneCoords(agentSimLoc);
            //transform.rotation = (UnityEngine.Vector3)RHelp.TKVector4(instance.Client.Self.SimRotation); 
            SetMapItemPosition(agent.transform , mapPos);

            //set mapItems
        }

        // moves the transform to the 2d map position (x,y).
        private void SetMapItemPosition(Transform entitiyTransform, UnityEngine.Vector2 mapPos)
        {
            UnityEngine.Vector3 newPos = fromMapCoord(mapPos);
            entitiyTransform.position = newPos;
        }

        // translate the vector2 globalMapPositions into scene vector3 transform positions
        private UnityEngine.Vector3 fromMapCoord(UnityEngine.Vector2 mapPos)
        {
            return new UnityEngine.Vector3(mapPos.x, mapItemDepthConstant , mapPos.y);
        }

        private UnityEngine.Vector2 toMapPlaneCoords(UnityEngine.Vector3 V3Offset)
        {
            return new UnityEngine.Vector2(V3Offset.x, V3Offset.z);
        }

        private void Grid_CoarseLocationUpdate(object sender, OpenMetaverse.CoarseLocationUpdateEventArgs e)
        {
            foreach (var newEntry in e.NewEntries)
            {
                agents.Add(newEntry);
            }

            foreach(var removeEntry in e.RemovedEntries)
            {
                agents.Remove(removeEntry);
            }

        }
    }
}
