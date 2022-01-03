using OpenMetaverse;
using Raindrop.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UniRx.Triggers;
using UE = UnityEngine ;
using UnityEngine ;
using Vector3 = OpenMetaverse.Vector3;

namespace Raindrop.Presenters
{
    //presents objects in the sim as boxes
    public class ObjectPresenter : MonoBehaviour
    {
        public Thread mainThread;
        public GameObject ObjectPrefab;

        private object objectsLock = new object();
        private Dictionary<UUID, UnityEngine.GameObject> objects 
            = new Dictionary<UUID, GameObject>(); //user UUID -> user gameobject 

        private RaindropInstance instance { get { return ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>(); } }
        //private RaindropNetcom netcom { get { return instance.Netcom; } }
        bool Active => instance.Client.Network.Connected;

        
        void Start()
        {
            mainThread = System.Threading.Thread.CurrentThread;
            instance.Client.Objects.ObjectUpdate += ObjectsOnObjectUpdate;
            instance.Client.Objects.TerseObjectUpdate += ObjectsOnTerseObjectUpdate;
        }

        private void ObjectsOnTerseObjectUpdate(object sender, TerseObjectUpdateEventArgs e)
        {
            GameObject res;
            objects.TryGetValue(e.Prim.ID, out res);
            if (res != null)
            {
                Debug.Log("object " + e.Prim.ID.ToString() + "has moved");
                Debug.LogWarning("object move to be implemented");
            }
        }
        
        private void ObjectsOnObjectUpdate(object sender, PrimEventArgs e)
        {
            if (e.Simulator != instance.Client.Network.CurrentSim)
                return;
            if (e.IsAttachment)
                return;
            
            if (isOnMainThread())
            {
                updatePrim(e);
            } else
            {

                UnityMainThreadDispatcher.Instance().Enqueue(() => {
                    // Debug.Log(" dispatching of update-obj to main thread"); //wow mega spam!
                    updatePrim(e);
                });
            }

        }


        private bool isOnMainThread()
        {
            return mainThread.Equals(System.Threading.Thread.CurrentThread);
        }

 

        //the routine that updates the position of the avatar or creates newly seen avatar
        private void updatePrim(PrimEventArgs e)
        {
            lock (objectsLock)
            {
                GameObject primGO = null;
                objects.TryGetValue(e.Prim.ID, out primGO);

                if (primGO == null)
                {
                    // Debug.Log("newly seen obj. rezzing " + e.Prim.ID);
                    if (e.Prim.Text != "" || e.Prim.Text != null )
                    {
                        // Debug.Log("prim has no props. " + e.Prim.ID); //this is for most prims.
                    }
                    else if (e.Prim.Properties.Name == null)
                    {
                        Debug.Log("prim has property, but no name . " + e.Prim.ID); //i have never seen this before.
                    }
                    primGO = CreatePrimGameObject(e.Prim);
                    objects[e.Prim.ID] = primGO;
                }
                else
                {
                    Debug.Log("updating obj position. " + e.Prim.ID);
                }

                SetPrimTransforms(e, primGO);
            }
        }


        private static void SetPrimTransforms(PrimEventArgs e, GameObject primGO)
        {
            UE.Vector3 pos = RHelp.TKVector3(e.Prim.Position);
            primGO.transform.position = pos;
            
            UE.Quaternion rot = RHelp.TKQuaternion4(e.Prim.Rotation);
            primGO.transform.rotation = rot;
        }
        
        //make the agent:
        // mesh
        // nametag
        // scaling
        // and more ???
        private GameObject CreatePrimGameObject(Primitive ePrim)
        {
            string objname;
            if (ePrim.Properties == null)
            {
                objname = "";
            }
            else
            {
                objname = ePrim.Properties.Name;
            }
            
            UE.Vector3 objScale = RHelp.TKVector3(ePrim.Scale);
            UUID uuid = ePrim.ID;
            
            GameObject avi = Instantiate(ObjectPrefab, this.transform);
            avi.transform.localScale = objScale;
            
            var nametag = avi.GetComponent<nameTagPresenter>();
            nametag.setName(objname);
            avi.name = uuid.ToString();
            
            return avi;
        }
    }
}