using OpenMetaverse;
using Raindrop;
using Raindrop.Netcom;
using Raindrop.Rendering;
using System;
using Plugins.CommonDependencies;
using UnityEngine;
using Vector3 = OpenMetaverse.Vector3;
using OMV = OpenMetaverse;

// send current player's heading to the backend. We are authoritative on this, as the viewer.
// we can send it in a lerp-y way, as we slowly turn in the scene.
public class PlayerFacingInput : MonoBehaviour
{
    private RaindropInstance instance { get { return RaindropInstance.GlobalInstance; } }
    private RaindropNetcom netcom { get { return instance.Netcom; } }
    private GridClient client { get { return instance.Client; } }
    bool Active => instance.Client.Network.Connected;

    //left handed; starting from forward in world space - clockwise from top-down 
    public void OnHeadingSet(float heading_lefthanded)
    {
        float heading_righthanded = -heading_lefthanded;
        instance.Movement.SetHeading(heading_righthanded);
    }
}