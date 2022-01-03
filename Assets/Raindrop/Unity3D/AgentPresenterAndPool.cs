using OpenMetaverse;
using Raindrop.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UniRx.Triggers;
using UE = UnityEngine ;
using UnityEngine ;

namespace Raindrop.Presenters
{
    //presents agents in the sim as boxes
    public class AgentPresenterAndPool : MonoBehaviour
    {
        public GameObject AgentPrefab;
        public GameObject MainAgent;

        private object avatarsDictLock = new object();
        private Dictionary<UUID, UnityEngine.GameObject> avatarsDict 
            = new Dictionary<UUID, GameObject>(); //user UUID -> user gameobject 

        private RaindropInstance instance { get { return ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>(); } }
        //private RaindropNetcom netcom { get { return instance.Netcom; } }
        bool Active => instance.Client.Network.Connected;

        
        void Start()
        {
            mainThread = System.Threading.Thread.CurrentThread;
            instance.Client.Objects.AvatarUpdate += new EventHandler<AvatarUpdateEventArgs>(Objects_AvatarUpdate);
        }

        
        private bool isOnMainThread()
        {
            return mainThread.Equals(System.Threading.Thread.CurrentThread);
        }

        public Thread mainThread;

        private void Objects_AvatarUpdate(object sender, AvatarUpdateEventArgs e)
        {
            if (e.Simulator != instance.Client.Network.CurrentSim)
                return;
            
            if (isOnMainThread())
            {
                updateAvatar(e);
            } else
            {

                UnityMainThreadDispatcher.Instance().Enqueue(() => {
                    Debug.Log(" dispatching of update-avatar to main thread");
                    updateAvatar(e);
                });
            }
            
        }

        //the routine that updates the position of the avatar or creates newly seen avatar
        private void updateAvatar(AvatarUpdateEventArgs e)
        {
            //as avatar tracking is enabled -> thus all of this event is a new avatar that we never seen before 

            if (e.Avatar.Name == instance.Client.Self.Name)
            {
                UE.Vector3 pos = RHelp.TKVector3(e.Avatar.Position);
                MainAgent.transform.position = pos;
                return;
            }

            lock (avatarsDictLock)
            {
                GameObject aviGO = null;
                avatarsDict.TryGetValue(e.Avatar.ID, out aviGO);

                if (aviGO == null)
                {
                    Debug.Log("newly seen avi. rezzing " + e.Avatar.Name);
                    aviGO = CreateAgentGameObject(e.Avatar.Name);

                    avatarsDict[e.Avatar.ID] = aviGO;
                }
                else
                {
                    Debug.Log("updating known-avi position. " + e.Avatar.Name);
                }

                UE.Vector3 pos = RHelp.TKVector3(e.Avatar.Position);
                aviGO.transform.position = pos;

                //
                // if (avatarsGO.ContainsKey(e.))
                // {
                //     //update existing avatar
                //     GameObject theavi = avatarsGO[e.Avatar.LocalID];
                //     theavi.transform.position = RHelp.TKVector3(e.Avatar.Position);
                // }
                // else
                // {
                //     //make new avatar out of this information and add to dict.
                //     var newavi = new GameObject();
                //     newavi.transform.position = RHelp.TKVector3(e.Avatar.Position);
                //     avatarsGO.Add(e.Avatar.LocalID, newavi);
                //
                //
                // }
            }
        }


        //make the agent:
        // mesh
        // nametag
        // and more ???
        private GameObject CreateAgentGameObject(string aviname)
        {
            GameObject avi = Instantiate(AgentPrefab);
            
            var nametag = avi.GetComponent<nameTagPresenter>();
            nametag.setName(aviname);
            
            return avi;
        }
        
        
        
        
    }

}