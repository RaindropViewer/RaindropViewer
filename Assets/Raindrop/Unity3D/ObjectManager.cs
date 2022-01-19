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
    //contains methods to display, update, de-rez the SLobjects as unity objects.
    public class ObjectManager : MonoBehaviour
    {
        public static Thread mainThread;
        public GameObject ObjectPrefab;

        private object objectsLock = new object();
        private Dictionary<UUID, UnityEngine.GameObject> objects 
            = new Dictionary<UUID, GameObject>(); //user UUID -> user gameobject 

        private RaindropInstance instance { get { return ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>(); } }
        bool Active => instance.Client.Network.Connected;

        
        void Start()
        {
            mainThread = System.Threading.Thread.CurrentThread;
            //instance.Client.Objects.ObjectUpdate += ObjectsOnObjectUpdate; //prims, foilage, attachments (for those that are static and we-just-saw-it)
            //instance.Client.Objects.TerseObjectUpdate += ObjectsOnTerseObjectUpdate; //prims, avatars (for those that move often and hap-hazardly)
        }

        private void ObjectsOnTerseObjectUpdate(object sender, TerseObjectUpdateEventArgs e)
        {
            // if (isOnMainThread())
            // {
                lock (objectsLock)
                {
                    GameObject obj;
                    objects.TryGetValue(e.Prim.ID, out obj);
                    if (obj != null)
                    {
                        SetPrimTransformsOnMainThread(e.Prim, obj);
                    }
                    else
                    {
                        RezNewObjectOnMainThread(e.Prim);
                        objects[e.Prim.ID] = obj;
                    }
                }  
            // } else
            // {
            //     UnityMainThreadDispatcher.Instance().Enqueue(() =>
            //     {
            //         ObjectsOnTerseObjectUpdate(sender, e);
            //     });
            // }
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


        private static bool isOnMainThread()
        {
            return mainThread.Equals(System.Threading.Thread.CurrentThread);
        }

 

        //  1. create prim if not exists yet.
        //  2. move prim if already exist
        private void updatePrim(PrimEventArgs e)
        {
            lock (objectsLock)
            {
                GameObject primGO = null;
                objects.TryGetValue(e.Prim.ID, out primGO); 

                if (primGO == null)
                {
                    RezNewObjectOnMainThread(e.Prim);
                }
                else
                {
                    //Debug.Log("updating obj position. " + e.Prim.ID);
                }

                SetPrimTransformsOnMainThread(e.Prim, primGO);
            }
        }


        private void RezNewObjectOnMainThread(Primitive e)
        {
            
            Debug.LogError("fixme");
            // GameObject go = new GameObject();
            // if (isOnMainThread())
            // {
            //      RezNewObject(e, out go);
            //      
            // } else
            // {
            //     UnityMainThreadDispatcher.Instance().Enqueue(() =>
            //     {
            //         RezNewObjectOnMainThread(e, out goo);
            //     });
            // }
            // goo = go;
        }

        //make sure to run this only on Main thread.
        private void RezNewObject(Primitive e, out GameObject go)
        {
            if (e.Text != "" || e.Text != null)
            {
                // Debug.Log("prim has no props. " + e.Prim.ID); //this is for most prims.
            }
            else if (e.Properties.Name == null)
            {
                Debug.Log("prim has property, but no name . " + e.ID); //i have never seen this before.
            }

            go = CreatePrimGameObject(e);
            //objects[e.ID] = primGO;
            //return primGO;
            return;
        }


        // sets object position on the main thread.
        private static void SetPrimTransformsOnMainThread(Primitive e, GameObject primGO)
        {
            //it might be possible that the object is removed at this point. handle it.
            if (primGO == null)
            {
                return; //object is already removed from scene.
            }

            if (isOnMainThread())
            {
                SetPrimTransformsOnMainThread(e, primGO);
            } else
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() => {
                    
                    UE.Vector3 pos = RHelp.TKVector3(e.Position);
                    primGO.transform.position = pos;
            
                    UE.Quaternion rot = RHelp.TKQuaternion4(e.Rotation);
                    primGO.transform.rotation = rot;
                    
                });
            }
            
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