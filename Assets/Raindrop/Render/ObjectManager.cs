using OpenMetaverse;
using Raindrop.Rendering;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using OpenMetaverse.Imaging;
using OpenMetaverse.Rendering;
using Raindrop.Render;
using Raindrop.Services.Bootstrap;
using UniRx.Toolkit;
using UE = UnityEngine ;
using UnityEngine ;
using UnityEngine.Serialization;
using Logger = OpenMetaverse.Logger;
using RenderSettings = Raindrop.Rendering.RenderSettings;

namespace Raindrop.Presenters
{
    // subscribes to object-data events and delegates them to the various other rendering managers.
    // - creates objects
    // - gives these objects positions
    // - gives these objects textures.
    public class ObjectManager : MonoBehaviour
    {
        public bool RenderingEnabled { get; set; }

        [FormerlySerializedAs("ObjectPrefab")] public GameObject PrimPrefab;

        private object objectsLock = new object();
        private Dictionary<uint, RenderPrimitive> Prims 
            = new Dictionary<uint, RenderPrimitive>(); //user UUID -> user gameobject 
        List<SceneObject> SortedObjects;
        
         MeshmerizerR renderer => Globals.renderer;

        delegate void GenericTask();
        readonly ConcurrentQueue<GenericTask> PendingTasks = new ConcurrentQueue<GenericTask>();
        private readonly SemaphoreSlim PendingTasksAvailable = new SemaphoreSlim(0);

        private ObjectPool<RenderPrimitive> PrimPool;
        // simple counter to prevent lag from too many meshings in a single frame.
        private int meshingsRequestedThisFrame;
        // a cache for decoded scuplties
        Dictionary<UUID, Texture2D> sculptCache = new Dictionary<UUID, Texture2D>();

        private RaindropInstance instance { get { return ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>(); } }
        private GridClient Client { get { return instance?.Client; } }
        bool Active => instance.Client.Network.Connected;
        
        void Start()
        {
            instance.Client.Objects.ObjectUpdate += ObjectsOnObjectUpdate; //prims, foilage, attachments (for those that are static and we-just-saw-it)
            instance.Client.Objects.TerseObjectUpdate += ObjectsOnTerseObjectUpdate; //prims, avatars (for those that move often and hap-hazardly)
        }

        private void Update()
        {
            //call main render loop
            MainRenderLoop();
        }

        private void MainRenderLoop()
        {
            if (!RenderingEnabled) return;

            MeshThePrimsAndItsChildren();
            
            //what should we do?
            //delegate to the other managers?
            
        }
        
        

        // do prims -> mesh conversion for some of them
        private void MeshThePrimsAndItsChildren()
        {
            lock (Prims)
            {
                foreach (RenderPrimitive obj in Prims.Values)
                {
                    //skip if not root.
                    if (obj.BasePrim.ParentID != 0) continue;

                    DoMeshIfRequired(obj);

                    //calculate and set rotations of children of this root object.
                    foreach (RenderPrimitive child_obj in Prims.Values)
                    {
                        DoMeshIfRequired(child_obj);
                    }
                }
            }

            // generate mesh from the primitive.
            void DoMeshIfRequired(RenderPrimitive obj)
            {
                if (!obj.Initialized) obj.Initialize();

                //do mesh conversions if needed.
                if (!obj.Meshed)
                {
                    if (!obj.Meshing && meshingsRequestedThisFrame < RenderSettings.MeshesPerFrame)
                    {
                        meshingsRequestedThisFrame++;
                        MeshPrim(obj);
                    }
                }

                //early exit
                if (obj.Faces == null) return;

                obj.Attached = false;

                //here is do avatar, but this class don't render avatars.
            }
        }

        // generate the mesh for the prim using its data.
        private void MeshPrim(RenderPrimitive rprim)
        {
            if (rprim.Meshing) return;

            rprim.Meshing = true;
            Primitive prim = rprim.BasePrim;

            // Regular prim can go in main thread....
            if (prim.Sculpt == null || prim.Sculpt.SculptTexture == UUID.Zero)
            {
                GenerateMeshPrim(rprim, prim);
            }
            // ..but mesh and sculptie needs to go in worker-thread
            else
            {
                PendingTasks.Enqueue(GenerateSculptOrMeshPrim(rprim, prim));
                PendingTasksAvailable.Release();
            }
        }

        private void GenerateMeshPrim(RenderPrimitive rprim, Primitive prim)
        {
            DetailLevel detailLevel = RenderSettings.PrimRenderDetail;
            if (RenderSettings.AllowQuickAndDirtyMeshing)
            {
                //Its a box or something else that can use lower meshing
                if (prim.Flexible == null && prim.Type == PrimType.Box &&
                    prim.PrimData.ProfileHollow == 0 &&
                    prim.PrimData.PathTwist == 0 &&
                    prim.PrimData.PathTaperX == 0 &&
                    prim.PrimData.PathTaperY == 0 &&
                    prim.PrimData.PathSkew == 0 &&
                    prim.PrimData.PathShearX == 0 &&
                    prim.PrimData.PathShearY == 0 &&
                    prim.PrimData.PathRevolutions == 1 &&
                    prim.PrimData.PathRadiusOffset == 0)
                    detailLevel = DetailLevel.Low;
            }

            FacetedMesh mesh = renderer.GenerateFacetedMesh(prim, detailLevel);
            rprim.Faces = mesh.Faces;
            PrimGen.CalculateBoundingBox(rprim);
            rprim.Meshing = false;
            rprim.Meshed = true;
        }


        // an sync way to generate mesh. used for more complex prims and all sculpties.
        private GenericTask GenerateSculptOrMeshPrim(RenderPrimitive rprim, Primitive prim)
        {
            return () =>
            {
                FacetedMesh mesh = null;

                try
                {
                    if (prim.Sculpt.Type != SculptType.Mesh)
                    { // Regular sculptie ...
                        Texture2D img = null;

                        lock (sculptCache)
                        {
                            if (sculptCache.ContainsKey(prim.Sculpt.SculptTexture))
                            {
                                img = sculptCache[prim.Sculpt.SculptTexture];
                            }
                        }

                        if (img == null)
                        {
                            if (LoadTexture(prim.Sculpt.SculptTexture, ref img, true))
                            {
                                sculptCache[prim.Sculpt.SculptTexture] = (Texture2D)img;
                            }
                            else
                            {
                                return;
                            }
                        }

                        mesh = renderer.GenerateFacetedSculptMesh(prim, (Texture2D)img, RenderSettings.SculptRenderDetail);
                    }
                    else
                    { // ... or Mesh
                        AutoResetEvent gotMesh = new AutoResetEvent(false);

                        Client.Assets.RequestMesh(prim.Sculpt.SculptTexture, (success, meshAsset) =>
                        {
                            if (!success || !FacetedMesh.TryDecodeFromAsset(prim, meshAsset, RenderSettings.MeshRenderDetail, out mesh))
                            {
                                Logger.Log("Failed to fetch or decode the mesh asset", Helpers.LogLevel.Warning, Client);
                            }
                            gotMesh.Set();
                        });

                        gotMesh.WaitOne(20 * 1000, false);
                    }
                }
                catch
                { }

                if (mesh != null)
                {
                    rprim.Faces = mesh.Faces;
                    PrimGen.CalculateBoundingBox(rprim);
                    rprim.Meshing = false;
                    rprim.Meshed = true;
                }
                else
                {
                    lock (Prims)
                    {
                        Prims.Remove(rprim.BasePrim.LocalID);
                    }
                }
            };
        }

        private void ObjectsOnTerseObjectUpdate(object sender, TerseObjectUpdateEventArgs e)
        {
            if (e.Simulator != instance.Client.Network.CurrentSim)
                return;
            
            //ignore avatars.
            if (e.Prim.PrimData.PCode == PCode.Avatar)
                return;

            UpdatePrim_QueueToMainThread(e.Prim);
        }
        
        private void ObjectsOnObjectUpdate(object sender, PrimEventArgs e)
        {
            if (e.Simulator.Handle != instance.Client.Network.CurrentSim.Handle)
                return;
            if (e.IsAttachment)
                return;
            
            UpdatePrim_QueueToMainThread(e.Prim);
            
        }

        // using the event's prim data, update the prims involved.
        // queued on the main thread, as we will be using Unity APIs
        public void UpdatePrim_QueueToMainThread(Primitive prim)
        {
            if (!RenderingEnabled) return;

            if (Globals.isOnMainThread())
            {
                //work.
                UpdatePrim(prim);

            } else
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    UpdatePrim_QueueToMainThread(prim);
                });
            }
            
            void UpdatePrim(Primitive primitive)
            {
                //create and insert prim's data into the hashtable.
                switch (primitive.PrimData.PCode)
                {
                    case PCode.Avatar:
                        //ignore avatars.
                        return;

                    case PCode.Prim:
                        if (primitive.Textures == null) return;

                        RenderPrimitive rPrim;
                        if (Prims.TryGetValue(primitive.LocalID, out rPrim))
                        {
                            rPrim.AttachedStateKnown = false;
                        }
                        else
                        {
                            rPrim = PrimPool.Rent();
                            //give the prim its default, uninitialised properties
                            rPrim.Meshed = false;
                            rPrim.BoundingVolume = new BoundingVolume();

                            rPrim.BoundingVolume.FromScale(primitive.Scale);
                        }

                        rPrim.BasePrim = primitive;
                        //locking is important, as the current code may be executing on the non-main thread where the network callback occur.
                        lock (Prims) Prims[primitive.LocalID] = rPrim;
                        break;
                    case PCode.ParticleSystem:
                    // todo
                    default:
                        // unimplemented foliage
                        break;
                }

                //render the prims data on the main thread.
                // note: looks like there is no need to run this on main thread, as we can just lock it (exclusive section).
                lock (objectsLock)
                {
                    RenderPrimitive obj;
                    Prims.TryGetValue(primitive.LocalID, out obj);
                    if (obj != null)
                    {
                        SetPrimTransforms_RequiresOnMainThread(primitive, obj.gameObject);
                    }
                    else
                    {
                        RezNewObject_RequiresOnMainThread(primitive, out obj);
                        Prims[primitive.LocalID] = obj;
                        SetPrimTransforms_RequiresOnMainThread(primitive, obj.gameObject);
                    }
                }
            }
        }

        // rez, position and rot is at zero.
        // warn: MUST be run by main thread.
        private void RezNewObject_RequiresOnMainThread(Primitive e, out RenderPrimitive newObj)
        {
            if (Thread.CurrentThread != Globals.GMainThread)
            {
                Debug.LogError("thou must run RezNewObject_RequiresOnMainThread on main thread..");
                newObj = null;
                return;
            }
            
            if (e == null)
            {
                Debug.LogError("null e in RezNewObject_RequiresOnMainThread" + this.ToString());
            }
            
            RezNewObject(e, out newObj);;
            
            // Debug.LogError("fixme");
            // Transform parent = this.gameObject.transform;
            // newObj = Instantiate(PrimPrefab, UE.Vector3.zero, UE.Quaternion.identity, parent);
            
            // GameObject go = new GameObject();
            // if (Globals.isOnMainThread())
            // {
            //      RezNewObject(e, out newObj);
            //      
            // } else
            // {
            //     UnityMainThreadDispatcher.Instance().Enqueue(() =>
            //     {
            //         RezNewObject_RequiresOnMainThread(e, newObj);
            //     });
            // }
            // return;
        }

        //make sure to run this only on Main thread.
        private void RezNewObject(Primitive e, out RenderPrimitive rp)
        {
            if (e.Text != "" || e.Text != null)
            {
                // Debug.Log("prim has no props. " + e.Prim.ID); //this is for most prims.
            }
            else if (e.Properties.Name == null)
            {
                Debug.Log("prim has property, but no name . " + e.ID); //i have never seen this before.
            }

            var go = CreatePrimGameObject(e);
            
            //get the controller class on this obj:
            rp = go.GetComponent<RenderPrimitive>();
            if (rp == null)
            {
                Debug.LogError("wtf why prim prefab has no RenderPrimitive script?");
            }
                
            //objects[e.ID] = primGO;
            //return primGO;
            return;
        }


        // sets object position on the main thread.
        // WARN: MUST be run on main thread
        
        
        private static void SetPrimTransforms_RequiresOnMainThread(Primitive prim, GameObject GO_ToSet)
        {
            //it might be possible that the object is removed at this point. handle it.
            if (GO_ToSet == null)
            {
                return; //object is already removed from scene.
            }
            
            if (Thread.CurrentThread != Globals.GMainThread)
            {
                Debug.LogError("thou shalt not run SetPrimTransforms_RequiresOnMainThread on main thread..");
                return;
            }
    
            // UnityMainThreadDispatcher.Instance().Enqueue(() => {
            UE.Vector3 pos = RHelp.TKVector3(prim.Position);
            GO_ToSet.transform.position = pos;
    
            UE.Quaternion rot = RHelp.TKQuaternion4(prim.Rotation);
            GO_ToSet.transform.rotation = rot;
            // });
        }
        
    
        
        //make the agent:
        // mesh it
        // nametag - todo: remove me.
        // scaling
        // position
        // rotation
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
            
            UUID uuid = ePrim.ID;
            
            GameObject go = Instantiate(PrimPrefab, this.transform);

            UE.Vector3 objScale = RHelp.TKVector3(ePrim.Scale);
            go.transform.localScale = objScale;
            
            UE.Vector3 objPos = RHelp.TKVector3(ePrim.Position);
            go.transform.position = objPos;

            UE.Quaternion objRot = RHelp.TKQuaternion4(ePrim.Rotation);
            go.transform.rotation = objRot;
            
            go.name = uuid.ToString();
            
            return go;
        }
        
        // a function that loads textures from the various possible places it can be from.
        // in order of priority:
        // 1. TGA cache (decoded texture on disk ; if enabled in Rendersettings)
        // 2. Asset cache of OMV 
        // 3. network
        private bool LoadTexture(UUID textureID, ref Texture2D texture, bool removeAlpha)
        {
            ManualResetEvent gotImage = new ManualResetEvent(false);
            Texture2D img = null;

            try
            {
                gotImage.Reset();
                bool hasAlpha, fullAlpha, isMask;
                byte[] tgaData;
                if (RHelp.LoadCachedImage(textureID, out tgaData, out hasAlpha, out fullAlpha, out isMask))
                {
                    img = LoadTGAClass.LoadTGA(new MemoryStream(tgaData));
                }
                else
                {
                    instance.Client.Assets.RequestImage(textureID, (state, assetTexture) =>
                        {
                            if (state == TextureRequestState.Finished)
                            {
                                lock (img)
                                {
                                    img = TexturePool.Get(TextureFormat.RGB24);
                                    T2D.LoadT2DWithoutMipMaps(assetTexture.AssetData, img); //blocking.
                            
                                    try
                                    {
                                        // remove alpha (required for scuplty)
                                        // ManagedImage mi = new ManagedImage(reader.DecodeToBitmap());
                                        // if (removeAlpha)
                                        // {
                                        //     if ((mi.Channels & ManagedImage.ImageChannels.Alpha) != 0)
                                        //     {
                                        //         mi.ConvertChannels(mi.Channels &
                                        //                            ~ManagedImage.ImageChannels.Alpha);
                                        //     }
                                        // }

                                        //todo: caching of decoded texture.
                                        // tgaData = mi.ExportTGA();
                                        // img = LoadTGAClass.LoadTGA(new MemoryStream(tgaData));
                                        // RHelp.SaveCachedImage(tgaData, textureID,
                                        //     (mi.Channels & ManagedImage.ImageChannels.Alpha) != 0, false,
                                        //     false);
                                    }
                                    catch (Exception)
                                    {
                                        Logger.Log("Failed to decode texture " + assetTexture.AssetID,
                                            Helpers.LogLevel.Warning, instance.Client);
                                    }
                           
                                }

                            }
                            gotImage.Set();
                        }
                    );
                    gotImage.WaitOne(30 * 1000, false);
                }
                if (img != null)
                {
                    texture = img;
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Logger.Log(e.Message, Helpers.LogLevel.Error, instance.Client, e);
                return false;
            }
        }
    }
}