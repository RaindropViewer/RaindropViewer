using OpenMetaverse;
using Raindrop.Rendering;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using OpenMetaverse.Imaging;
using OpenMetaverse.Rendering;
using Plugins.CommonDependencies;
using Plugins.ObjectPool;
using Raindrop.Bootstrap;
using Raindrop.Render;
using UniRx.Toolkit;
using UE = UnityEngine ;
using UnityEngine ;
using UnityEngine.Serialization;
using Logger = OpenMetaverse.Logger;
using RenderSettings = Raindrop.Rendering.RenderSettings;

namespace Raindrop.Presenters
{
    // on every update, do a scan 
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
        
         MeshmerizerR renderer => Globals.renderer;

        delegate void GenericTask();
        readonly ConcurrentQueue<GenericTask> PendingTasks = new ConcurrentQueue<GenericTask>();
        private readonly SemaphoreSlim PendingTasksAvailable = new SemaphoreSlim(0);

        // private ObjectPool<RenderPrimitive> PrimPool;
        // simple counter to prevent lag from too many meshings in a single frame.
        private int meshingsRequestedThisFrame;
        // a cache for decoded scuplties
        Dictionary<UUID, Texture2D> sculptCache = new Dictionary<UUID, Texture2D>();

        private RaindropInstance instance { get { return ServiceLocator.Instance.Get<RaindropInstance>(); } }
        private GridClient Client { get { return instance?.Client; } }
        bool Active => instance.Client.Network.Connected;
        
        void Start()
        {
            // instance.Client.Objects.ObjectUpdate += ObjectsOnObjectUpdate; //prims, foilage, attachments (for those that are static and we-just-saw-it)
            // instance.Client.Objects.TerseObjectUpdate += ObjectsOnTerseObjectUpdate; //prims, avatars (for those that move often and hap-hazardly)
            
            instance.Client.Network.SimConnected += NetworkOnSimConnected;

            // PrimPool = ObjectPool<RenderPrimitive>
        }

        private void NetworkOnSimConnected(object sender, SimConnectedEventArgs e)
        {
            if (e.Simulator == instance.Client.Network.CurrentSim)
            {
                RenderingEnabled = true;
            }
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

            if (UnityMainThreadDispatcher.isOnMainThread())
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
                RenderPrimitive rPrim;
                
                //create and insert prim's data into the hashtable.
                switch (primitive.PrimData.PCode)
                {
                    case PCode.Avatar:
                        //ignore avatars.
                        return;

                    case PCode.Prim:
                        if (primitive.Textures == null) return;

                        //check if the prim is already in the scene dictionary
                        if (Prims.TryGetValue(primitive.LocalID, out rPrim))
                        {
                            SetPrimTransforms_RequiresOnMainThread(primitive, rPrim.gameObject);
                            rPrim.AttachedStateKnown = false;
                        }
                        else
                        {
                            //looks like the prim is new to us.
                            
                            //give the prim its default, uninitialised properties
                            rPrim.Meshed = false;
                            rPrim.BoundingVolume = new BoundingVolume();

                            //mesh it
                            rPrim.BoundingVolume.FromScale(primitive.Scale);
                            RezNewObject_RequiresOnMainThread(primitive, out rPrim);
                            Prims[primitive.LocalID] = rPrim;
                            SetPrimTransforms_RequiresOnMainThread(primitive, rPrim.gameObject);
                            
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

            }
        }

        // rez, position and rot is at zero.
        // warn: MUST be run by main thread.
        private void RezNewObject_RequiresOnMainThread(Primitive e, out RenderPrimitive newObj)
        {
            if (! UnityMainThreadDispatcher.isOnMainThread())
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
            
        }

        //make sure to run this only on Main thread.
        private void RezNewObject(Primitive e, out RenderPrimitive rp)
        {
            var go = CreatePrimGameObject(e);
            
            //get the controller class on this obj:
            rp = go.GetComponent<RenderPrimitive>();
            if (rp == null)
            {
                Debug.LogError("wtf why prim prefab has no RenderPrimitive script?");
            }
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
            
            if (! UnityMainThreadDispatcher.isOnMainThread())
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
        
    
        
        //make the Gameobject that represents the primitive:
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
            
            // UUID uuid = ePrim.ID;
            string name = ePrim.Properties.Name;
            string desc = ePrim.Properties.Description;
            string sitString = ePrim.Properties.SitName;
            string touchString = ePrim.Properties.TouchName;
            
            GameObject go = Instantiate(PrimPrefab, this.transform);

            UE.Vector3 objScale = RHelp.TKVector3(ePrim.Scale);
            go.transform.localScale = objScale;
            
            UE.Vector3 objPos = RHelp.TKVector3(ePrim.Position);
            go.transform.position = objPos;

            UE.Quaternion objRot = RHelp.TKQuaternion4(ePrim.Rotation);
            go.transform.rotation = objRot;

            go.name = name;
            
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
                                    img = TexturePoolSelfImpl.GetInstance().GetFromPool(TextureFormat.RGB24);
                                    T2D_JP2.LoadT2DWithoutMipMaps(assetTexture.AssetData, img); //blocking.
                            
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