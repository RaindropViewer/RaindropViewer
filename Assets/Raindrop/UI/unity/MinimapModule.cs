using OpenMetaverse;
using OpenMetaverse.Assets;
using OpenMetaverse.Imaging;
using Raindrop.Netcom;
using System;
using UniRx;
using Unity;
using UnityEngine;
using UnityEngine.UI;

namespace Raindrop.Presenters
{
    //i think the minimap-module should its own monobehavior, present in the canvas ui,
    // it will only work when you inject the instance and the client into it.
    // it is as if it is its own master; only its on-and-off is controlled by other entities
    // it is updated by events of the like 'avatar has moved' and 'camera has moved' and 'avatar has moved sims'
    public class MinimapModule:MonoBehaviour
    {
        [SerializeField]
        private Image Image;        //the unity ui component
        private Texture2D map_tex;   //the unity engine texture.

        private RaindropInstance instance { get { return RaindropInstance.GlobalInstance; } }
        private RaindropNetcom netcom { get { return instance.Netcom; } }
        private GridClient client { get { return instance.Client; } }


        //private UnityEngine.Vector3 referenceToCameraPosition;
        private UnityEngine.Vector3 referenceToAvatarPosition;
        //private UnityEngine.Vector3 referenceToSimPositionGlobal;
        public Button MiniMapButton;

        MinimapModule()
        {
            MiniMapButton.onClick.AsObservable().Subscribe(_ => OnMinimapClick()); //when clicked, runs this method.



            instance.Client.Network.SimChanged += Network_OnCurrentSimChanged;


        }
        private void Update()
        {
            //set the camera to follow the avi.


        }


        private void OnMinimapClick()
        {
            //what happend when minimap is clicked?
            instance.MainCanvas.canvasManager.pushCanvas(CanvasType.Map);
            Debug.Log("clicked minimap");
        }

        private void Network_OnCurrentSimChanged(object sender, SimChangedEventArgs e)
        {
            if (client.Network.Connected) return;

            GridRegion region;
            if (client.Grid.GetGridRegion(client.Network.CurrentSim.Name, GridLayerType.Objects, out region))
            {
                Texture2D _new_MapLayer = null; // LOL! its unity texture btw.

                UUID _MapImageID = region.MapImageID;
                ManagedImage nullImage;

                client.Assets.RequestImage(_MapImageID, ImageType.Baked,
                    delegate (TextureRequestState state, AssetTexture asset)
                    {
                        if (state == TextureRequestState.Finished)
                        {
                            OpenJPEG.DecodeToImage(asset.AssetData, out nullImage, out _new_MapLayer); //this call interally calls to another function 'DecodeToImage(byte[] encoded, out ManagedImage managedImage)'

                            SetMapLayer(_new_MapLayer );
                        }
                        else
                            Debug.LogWarning("minimap failed to DL texture.");
                    });
            }

        }

        private void SetMapLayer(Texture2D new_texture )
        {
            Destroy(map_tex); //delete the tex2d that is no longer (?) used.

            map_tex = new_texture;
            //throw new NotImplementedException();
        }



    }
}