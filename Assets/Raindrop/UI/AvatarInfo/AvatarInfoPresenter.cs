using OpenMetaverse;
using OpenMetaverse.StructuredData;
using Raindrop;
using Raindrop.Netcom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using System;
using FMOD;
using OpenMetaverse.Assets;
using Raindrop.Services;
using Settings = Raindrop.Settings;
using UnityEngine.UI;
using UniRx;
using TMPro;
using static Raindrop.LoginUtils;
using Avatar = OpenMetaverse.Avatar;
using Logger = OpenMetaverse.Logger;


namespace Raindrop.Presenters
{
    // show the avatar card and all the information.

    public class AvatarInfoPresenter : MonoBehaviour
    {
        private RaindropInstance instance { get { return ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>(); } }
        private RaindropNetcom netcom { get { return instance.Netcom; } }
        private UIService uimanager => ServiceLocator.ServiceLocator.Instance.Get<UIService>();
        
        #region UI elements - the 'view' in MVP
        public RawImageView avatarImg;
        public TMP_Text aviName;
        #endregion

        #region internal data representation 

        // public Avatar avatarShown;
        public UUID aviID;
        public UUID aviImgID { get; set; }
        #endregion

         
        void Start()
        {

            AddNetcomEvents();
        }

        private void UpdateAviName(string s)
        {
            aviName.text = s;
        }

        

        private void AddNetcomEvents()
        {
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
            
            //Image UUID and image
            this.aviImgID = e.Properties.ProfileImage;
            instance.Client.Assets.RequestImage(e.AvatarID, ImageReady_Callback);
            
            //UI - set name
            this.aviName.text = instance.Names.Get(aviID);
        }

        private void ImageReady_Callback(TextureRequestState state, AssetTexture assettexture)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                var ok = assettexture.Decode(); //call openjpg to decode j2k->managedimage
                if (ok)
                {
                    var img =   assettexture.Image;
                    Texture2D t2d = img.ExportTex2D();
                    
                    this.avatarImg.setRawImage(t2d);
                }
                else
                {
                    Logger.Log("decoding texture failed!", Helpers.LogLevel.Error);
                }
            });
        }


        private void RemoveNetcomEvents()
        {
            netcom.ClientLoggingIn -= new EventHandler<OverrideEventArgs>(NetcomOnClientConnected);
        }



    }

}