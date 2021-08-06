using OpenMetaverse;
using OpenMetaverse.Assets;
using OpenMetaverse.Imaging;
using Raindrop.Netcom;
using System;
using System.Collections.Generic;
using UniRx;
using Unity;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Raindrop.Presenters
{
    //map texture module
    // this manages the 2d texture objects in the map texture layer
    //[RequireComponent(typeof(Image))]
    public class MapImageManager:MonoBehaviour
    {
        //[SerializeField]
        //private RawImage Image;        //the unity ui component
        //private Texture2D map_tex;   //the unity engine texture.

        //private Vector2Int mapCoord;

        private Dictionary<Vector2Int, GameObject> map_collection;

        private SimTexture st;

        private OpenMetaverse.Vector2 viewableLow;
        private OpenMetaverse.Vector2 viewableHigh;

        private MapImageCameraPresenter cameraPresenter;

        private RaindropInstance instance { get { return ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>(); } }
        private RaindropNetcom netcom { get { return instance.Netcom; } }
        private GridClient client { get { return instance.Client; } }


        public GameObject Avatar;

        public void Start()
        {
            //reset camera to a sane lcoation
            //cameraPresenter.reset(); 
            cameraPresenter = this.gameObject.AddComponent<MapImageCameraPresenter>();
            cameraPresenter.init(); 

            //set the camera to render to the intended render texture

            instance.Client.Network.SimChanged += Network_OnCurrentSimChanged;

        }

        //x and y in OSL positions
        public void lookAtGridPos(int x, int y)
        {
            cameraPresenter.setPos(x, y);

        }

        //public OpenMetaverse.Vector2 getLookAtGridPos()
        //{
        //    return cameraPresenter.gridPosition;
        //}

         

        private void Network_OnCurrentSimChanged(object sender, SimChangedEventArgs e)
        {

            if (client.Network.Connected) return;

            Debug.Log("Network_OnCurrentSimChanged");

            GridRegion region;
            if (client.Grid.GetGridRegion(client.Network.CurrentSim.Name, GridLayerType.Objects, out region))
            {

                Texture2D _new_MapLayer = null; // LOL! its unity texture btw.

                UUID _MapImageID = region.MapImageID;
                Vector2Int regionPos = new Vector2Int(region.X,region.Y);
                ManagedImage nullImage;

                Debug.Log("requesting map image.");

                client.Assets.RequestImage(_MapImageID, ImageType.Baked,
                    delegate (TextureRequestState state, AssetTexture asset)
                    {
                        if (state == TextureRequestState.Finished)
                        {
                            Debug.Log("minimap texture fetched, decoding it!");
                            OpenJPEG.DecodeToImage(asset.AssetData, out nullImage, out _new_MapLayer); //this call interally calls to another function 'DecodeToImage(byte[] encoded, out ManagedImage managedImage)'

                            UnityMainThreadDispatcher.Instance().Enqueue(() =>
                            {
                                SetMapLayer(_new_MapLayer, regionPos);
                            });
                        }
                        else
                            Debug.LogWarning("minimap failed to DL texture.");
                    });
            }

        }

        //sets a ,map block that is 256pic * 256pic
        private void SetMapLayer(Texture2D new_texture, Vector2Int regionXY)
        {
            Debug.Log("setting the image to the new gameobject");
            if (map_collection.ContainsKey(regionXY))
            {
                //update the region.
                //MonoBehaviour theGO;
                //map_collection.TryGetValue(regionXY, out theGO);

                //Destroy(theGO.map_tex); //delete the tex2d that is no longer (?) used.
            } else
            {
                GameObject mapGO = new GameObject();
                mapGO.transform.SetParent(this.transform);

                var MR = mapGO.AddComponent<MeshRenderer>();
                MR.sharedMaterial = new UnityEngine.Material(Shader.Find("Standard"));
                //MR.
                var MF = mapGO.AddComponent<MeshFilter>();

                //generate the plane.
                Mesh m = new Mesh();
                var width = 1;
                var height = 1;
                m.vertices = new Vector3[]{
                                new Vector3(0, 0, 0),
                                new Vector3(width, 0, 0),
                                new Vector3(width, height, 0),
                                new Vector3(0, height, 0)
                };
                m.uv = new Vector2[]{
                    new Vector2(0, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1),
                    new Vector2(1, 0),
                };
                m.triangles = new int[] { 0, 2, 1, 0, 3, 2 }; //clockwise?
                MF.mesh = m;
                m.RecalculateBounds();
                m.RecalculateNormals();

                MR.material.mainTexture = new_texture;

                map_collection.Add(regionXY, mapGO);

            }


        }



    }
}