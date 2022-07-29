using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean;
using Raindrop;
using OpenMetaverse;
using Raindrop.Netcom;
using Lean.Gui;
using Plugins.CommonDependencies;
using Vector2 = UnityEngine.Vector2;

//update the backend on user's (u,d,l,r)
public class UpdateMovementBackend : MonoBehaviour
{
    private RaindropInstance instance { get { return RaindropInstance.GlobalInstance; } }
    private RaindropNetcom netcom { get { return instance.Netcom; } }
    private GridClient client { get { return instance.Client; } }
    bool Active => instance.Client.Network.Connected;

   // Use this to update the up, down, left, right movements.
    public void OnWASDSet(
            bool up,
            bool down,
            bool left,
            bool right)
    {
        instance.Movement.SetWasdInput(up,down,left,right);
    } 

}
