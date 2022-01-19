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
    public class AgentPresenter : MonoBehaviour
    {
        public GameObject AgentPrefab;
        //public GameObject MainAgent;

        private object avatarsDictLock = new object();
        private Dictionary<UUID, UnityEngine.GameObject> avatarsDict 
            = new Dictionary<UUID, GameObject>(); //user UUID -> user gameobject 

        private RaindropInstance instance { get { return ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>(); } }
        //private RaindropNetcom netcom { get { return instance.Netcom; } }
        bool Active => instance.Client.Network.Connected;

        void Start()
        {
            mainThread = System.Threading.Thread.CurrentThread;
            //this seems to happen on new avatars.
            instance.Client.Objects.AvatarUpdate +=  Objects_AvatarUpdate;
            //this seems to happen on avatar movements.
            instance.Client.Objects.TerseObjectUpdate += ObjectsOnTerseObjectUpdate;
        }

        
        private void ObjectsOnTerseObjectUpdate(object sender, TerseObjectUpdateEventArgs e)
        {
            if (e.Simulator != instance.Client.Network.CurrentSim)
                return;
            
            if (isOnMainThread())
            {
                updateAvatarPosition(e);
            } else
            {

                UnityMainThreadDispatcher.Instance().Enqueue(() => {
                    updateAvatarPosition(e);
                });
            }
            
        }

        private void updateAvatarPosition(TerseObjectUpdateEventArgs e)
        {
            lock (avatarsDictLock)
            {
                GameObject agent;
                avatarsDict.TryGetValue(e.Prim.ID, out agent);
                if (agent != null)
                {
                    UpdateAvatarTransforms(e.Prim, agent);
                }
            }
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
            

            lock (avatarsDictLock)
            {
                GameObject aviGO = null;
                avatarsDict.TryGetValue(e.Avatar.ID, out aviGO);

                if (aviGO == null)
                {
                    Debug.Log("newly seen avi. rezzing " + e.Avatar.Name);
                    aviGO = CreateAgentGameObject(e.Avatar.Name);

                    avatarsDict[e.Avatar.ID] = aviGO;
                    if (e.Avatar.Name == instance.Client.Self.Name)
                    {
                        cameraTrackAgent(aviGO);
                    }
                    
                }
                else
                {
                    Debug.Log("updating known-avi position. " + e.Avatar.Name);
                }

                UpdateAvatarTransforms(e.Avatar, aviGO);
            }
        }

        private static void UpdateAvatarTransforms(Primitive e, GameObject aviGO)
        {
            UE.Vector3 pos = RHelp.TKVector3(e.Position);
            UE.Quaternion rot = RHelp.TKQuaternion4(e.Rotation);
            aviGO.transform.position = pos;
            aviGO.transform.rotation = rot;
        }

        //give the object for the main camera to track.
        private void cameraTrackAgent(GameObject aviGo)
        {
            var _ = UE.Camera.main.gameObject.GetComponent<simpleFollow>();
            _.target = aviGo.transform;
        }


        //make the agent:
        // mesh
        // nametag
        // and more ???
        private GameObject CreateAgentGameObject(string aviname)
        {
            GameObject avi = Instantiate(AgentPrefab, this.transform);

            var nametag = avi.GetComponent<nameTagPresenter>();
            nametag.setName(aviname);
            
            return avi;
        }
    }
}