using OpenMetaverse;
using Raindrop.Netcom;
using UnityEngine;
using System;
using OpenMetaverse.Assets;
using Plugins.CommonDependencies;
using Plugins.ObjectPool;
using Raindrop.Services;
using TMPro;
using UnityEngine.Serialization;
using Logger = OpenMetaverse.Logger;

namespace Raindrop.Presenters
{
    // show the avatar card and all the information.

    public class AvatarInfoPresenter : MonoBehaviour
    {
        private RaindropInstance instance { get { return RaindropInstance.GlobalInstance; } }
        private RaindropNetcom netcom { get { return instance.Netcom; } }
        private UIService uimanager => ServiceLocator.Instance.Get<UIService>();
        
        #region UI elements - the 'view' in MVP
        public RawImageView avatarImg;
        public TMP_Text aviName;
        #endregion

        #region internal data representation 
        public UUID aviID = UUID.Zero;
        [FormerlySerializedAs("avi2ndLifeImage")] public UUID avi2ndLifeImageID;
        public Texture2D t2d_2;
        public Texture2D t2d;
        #endregion

        enum State
        {
            Uninitialised,
            Initialised
            
        }

        private State state = State.Uninitialised;
        
        void Start()
        {
            
            if (instance != null)
            {
                if (netcom != null)
                {
                    init(instance.Client.Self.AgentID);
                }
            }
        }

        public void init(UUID aviID)
        {
            this.aviID = aviID;
            
            t2d_2 = new Texture2D(69, 69);
            t2d = new Texture2D(69, 69);
            AddNetcomEvents();
            state = State.Initialised;
        }

        //on enable runs even before start...
        private void OnEnable()
        {
            
            if (state != State.Initialised)
            {
                return;
            }
            
            //ask server for avatar info.
            instance.Client.Avatars.RequestAvatarProperties(aviID);
        }
        
        private void AddNetcomEvents()
        {
            //raised by the call to  <see vref = "Netcom.Network_LoginProgress()" />
            netcom.ClientConnected += NetcomOnClientConnected; 
            
        }

        private void NetcomOnClientConnected(object sender, EventArgs e)
        {
            instance.Client.Avatars.AvatarPropertiesReply += new EventHandler<AvatarPropertiesReplyEventArgs>(Avatars_AvatarPropertiesReply);
        }

        private void Avatars_AvatarPropertiesReply(object sender, AvatarPropertiesReplyEventArgs e)
        {
            if (e.AvatarID != aviID) return;
            
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Avatars_AvatarPropertiesReply(sender, e);
            });

            UpdateAgentInfos(e);
        }

        //write UI. please do this on main thread.
        private void UpdateAgentInfos(AvatarPropertiesReplyEventArgs e)
        {
            // ID.
            this.aviID = e.AvatarID;
            
            //UI - set name
            this.aviName.text = instance.Names.Get(aviID);
            
            //Image UUID and image
            this.avi2ndLifeImageID = e.Properties.ProfileImage;
            instance.Client.Assets.RequestImage(e.AvatarID, ImageReady_Callback);
            
        }

        private void ImageReady_Callback(TextureRequestState state, AssetTexture assettexture)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                var decodeSuccess = assettexture.Decode(); //call openjpg to decode j2k->managedimage
                if (decodeSuccess)
                {
                    var img = assettexture.Image;
                    Texture2D t2d = TexturePoolSelfImpl.GetInstance().GetFromPool(TextureFormat.RGB24);
                    img.ExportTex2D(t2d);
                    
                    avatarImg.setRawImage(t2d);
                }
                else
                {
                    Logger.Log("decoding texture failed!", Helpers.LogLevel.Error);
                }
            });
        }


        //todo: make destor call this.
        private void RemoveNetcomEvents()
        {
            netcom.ClientLoggingIn -= new EventHandler<OverrideEventArgs>(NetcomOnClientConnected);
        }

    }

}