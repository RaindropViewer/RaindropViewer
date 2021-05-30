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
        private Image Image;
        private Texture2D my_img;

        private RaindropInstance instance { get { return RaindropInstance.GlobalInstance; } }
        private RaindropNetcom netcom { get { return instance.Netcom; } }
        private GridClient client { get { return instance.Client; } }

        private bool isWorking = false;

        private UnityEngine.Vector3 referenceToCameraPosition;
        private UnityEngine.Vector3 referenceToAvatarPosition;
        private UnityEngine.Vector3 referenceToSimPositionGlobal;
        public Button MiniMapButton;
        private readonly int DEFAULT_RES = 500;

        MinimapModule()
        {
            MiniMapButton.onClick.AsObservable().Subscribe(_ => OnMinimapClick()); //when clicked, runs this method.



            instance.Client.Network.SimChanged += Network_OnCurrentSimChanged;


        }

        private void OnMinimapClick()
        {
            //what happend when minimap is clicked?
            instance.MainCanvas.canvasManager.pushCanvas(CanvasType.Map);
            Debug.Log("clicked minimap");
        }

        private void Network_OnCurrentSimChanged(object sender, SimChangedEventArgs e)
        {
            isWorking = true;

            if (client.Network.Connected) return;

            GridRegion region;
            if (client.Grid.GetGridRegion(client.Network.CurrentSim.Name, GridLayerType.Objects, out region))
            {
                SetMapLayer(my_img);

                var _MapImageID = region.MapImageID;
                ManagedImage nullImage;

                client.Assets.RequestImage(_MapImageID, ImageType.Baked,
                    delegate (TextureRequestState state, AssetTexture asset)
                    {
                        if (state == TextureRequestState.Finished)
                            OpenJPEG.DecodeToImage(asset.AssetData, out nullImage, out _MapLayer);
                        else
                            Debug.LogWarning("minimap failed to DL texture.");
                    });
            }

        }

        private void SetMapLayer(Texture2D my_img)
        {
            my_img = new Texture2D(DEFAULT_RES, DEFAULT_RES);
            //throw new NotImplementedException();
        }


    }
}