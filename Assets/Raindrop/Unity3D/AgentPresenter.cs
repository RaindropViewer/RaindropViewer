using OpenMetaverse;
using Raindrop.Rendering;
using System.Collections.Generic;
using System.Threading;
using Plugins.CommonDependencies;
using UE = UnityEngine ;
using UnityEngine ;

namespace Raindrop.Presenters
{
    //presents agents in the sim as boxes
    // also update the position of minimap avatars.
    public class AgentPresenter : MonoBehaviour
    {
        public GameObject AgentPrefab;
        public GameObject MinimapAgentPrefab;
        public Transform MinimapRoot;
        public uint z_MinimapAgents;

        private object avatarsDictLock = new object();
        private Dictionary<UUID, UnityEngine.GameObject> avatarsDict 
            = new Dictionary<UUID, GameObject>(); //user UUID -> user gameobject 
        private Dictionary<UUID, UnityEngine.GameObject> avatarsDictMinimap
            = new Dictionary<UUID, GameObject>(); //user UUID -> user gameobject in minimap
        public GameObject agentReference; //reference to the agent, if rezzed - it should.

        private RaindropInstance instance { get { return ServiceLocator.Instance.Get<RaindropInstance>(); } }
        //private RaindropNetcom netcom { get { return instance.Netcom; } }
        bool Active => instance.Client.Network.Connected;

        void Start()
        {
            //this seems to happen on new avatars.
            instance.Client.Objects.AvatarUpdate +=  Objects_AvatarUpdate;
            //this seems to happen on avatar movements.
            instance.Client.Objects.TerseObjectUpdate += ObjectsOnTerseObjectUpdate;
        }

        
        private void ObjectsOnTerseObjectUpdate(object sender, TerseObjectUpdateEventArgs e)
        {
            if (e.Simulator != instance.Client.Network.CurrentSim)
                return;
            
            if (UnityMainThreadDispatcher.isOnMainThread())
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
        private void Objects_AvatarUpdate(object sender, AvatarUpdateEventArgs e)
        {
            if (e.Simulator != instance.Client.Network.CurrentSim)
                return;
            
            if (UnityMainThreadDispatcher.isOnMainThread())
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
                GameObject mapGO = null;
                avatarsDict.TryGetValue(e.Avatar.ID, out aviGO);
                avatarsDictMinimap.TryGetValue(e.Avatar.ID, out mapGO);

                if (aviGO == null) //is new avatar.
                {
                    Debug.Log("rezzing " + e.Avatar.Name);
                    aviGO = CreateAgentGameObject(e.Avatar.Name);
                    mapGO = CreateMinimapGameObject(e.Avatar.Name);

                    avatarsDict[e.Avatar.ID] = aviGO;
                    avatarsDictMinimap[e.Avatar.ID] = mapGO;
                    if (e.Avatar.Name == instance.Client.Self.Name)
                    {
                        agentReference = aviGO;
                        //cameraTrackAgent(aviGO);
                    }
                    
                }

                UpdateAvatarTransforms(e.Avatar, aviGO);
                UpdateMinimapTransforms(e.Avatar.RegionHandle, e.Avatar.Position, mapGO);
            }
        }

        private static void UpdateAvatarTransforms(Primitive e, GameObject aviGO)
        {
            UE.Vector3 pos = RHelp.TKVector3(e.Position);
            // UE.Quaternion rot = RHelp.TKQuaternion4(e.Rotation);
            aviGO.transform.position = pos;
            // aviGO.transform.rotation = rot;
        }

        private void UpdateMinimapTransforms(ulong regionHandle, OpenMetaverse.Vector3 position, GameObject aviGO)
        {
            var region = Utilities.MapSpaceConverters.Handle2MapSpace(regionHandle, z_MinimapAgents);
            var ObjectCoordinatesWithinSim = RHelp.TKVector3(position);
            var ObjectOffsetWithinSim_InRegionCoordinates = ObjectCoordinatesWithinSim / 256.0f;
            aviGO.transform.position = region + ObjectOffsetWithinSim_InRegionCoordinates;
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

        private GameObject CreateMinimapGameObject(string name)
        {
            GameObject avi = Instantiate(MinimapAgentPrefab, MinimapRoot);

            return avi;
        }

    }

}