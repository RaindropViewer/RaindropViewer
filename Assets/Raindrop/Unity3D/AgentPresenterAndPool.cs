using OpenMetaverse;
using Raindrop.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raindrop.Presenters
{

    //presents agents in the sim as boxes
    public class AgentPresenterAndPool : MonoBehaviour
    {
        private Dictionary<uint, UnityEngine.GameObject> avatarsGO;

        private RaindropInstance instance { get { return RaindropInstance.GlobalInstance; } }
        //private RaindropNetcom netcom { get { return instance.Netcom; } }
        bool Active => instance.Client.Network.Connected;

        // Start is called before the first frame update
        void Start()
        {
            instance.Client.Objects.AvatarUpdate += new EventHandler<AvatarUpdateEventArgs>(Objects_AvatarUpdate);
        }

        private void Objects_AvatarUpdate(object sender, AvatarUpdateEventArgs e)
        {
            if (e.Simulator != instance.Client.Network.CurrentSim)
                return;


            lock (avatarsGO)
            {
                if (avatarsGO.ContainsKey(e.Avatar.LocalID))
                {
                    //update existing avatar
                    GameObject theavi = avatarsGO[e.Avatar.LocalID];
                    theavi.transform.position = RHelp.TKVector3(e.Avatar.Position);
                }
                else
                {
                    //make new avatar out of this information and add to dict.
                    var newavi = new GameObject();
                    newavi.transform.position = RHelp.TKVector3(e.Avatar.Position);
                    avatarsGO.Add(e.Avatar.LocalID, newavi);


                }
            }
        }

    }

}