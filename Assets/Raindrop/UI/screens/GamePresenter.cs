using OpenMetaverse;
using OpenMetaverse.StructuredData;
using Raindrop;
using Raindrop.Netcom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using System;
using Settings = Raindrop.Settings;
using UnityEngine.UI;
using UniRx;
using TMPro;
using OpenMetaverse.Imaging;
using Raindrop.Services;


//view(unitytext) -- presenter(this) -- controller(this?) -- model (raindropinstance singleton)
namespace Raindrop.Presenters
{
    public class GamePresenter : MonoBehaviour
    {
        //what does the main user interaction have with the 3d env?
        //1 . movements - joy, run toggle, jump, fly toggle, fly-U/D, 
        //2 . notifications - a dragdown list-like UI
        //3 . buttons to access other features.

        private RaindropInstance instance { get { return ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>(); } }
        private RaindropNetcom netcom { get { return instance.Netcom; } }
        private GridClient client { get { return instance.Client; } }

        private UIService uimanager;
        private Settings s;

        bool Active => instance.Client.Network.Connected;


        #region references to UI elements
        //buttons that open up other canvas.
        public Button ChatButton; 
        public Button MapButton; 

        //currently unused
        public Toggle SoundToggle; 
        
        //HUD display.
        public TMP_Text locationText; // sim + x y z of the user.
        public TMP_Text usernameText; // user's name.
        //public MinimapModule minimap;
        #endregion

        private float timeSinceLastUpdate;
        private System.Threading.Timer configTimer;
        private const int saveConfigTimeout = 1000;
        #region behavior

        // Use this for initialization
        void Start()
        {
            initialiseFields();
            
            configTimer = new System.Threading.Timer(SaveConfig, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

            ChatButton.onClick.AsObservable().Subscribe(_ => OnChatBtnClick()); //when clicked, runs this method.
            MapButton.onClick.AsObservable().Subscribe(_ => OnMapBtnClick()); //when clicked, runs this method.
            SoundToggle.OnValueChangedAsObservable().Subscribe(_ => OnToggleSounds(_));

            //simName.AsObservable().Subscribe(_ => UpdateSimLocDisplay(_));

            //get uimanager service
            uimanager = ServiceLocator.ServiceLocator.Instance.Get<UIService>();
            
            client.Network.SimConnected += Network_SimConnected;

            //client.Network.SimChanged += Network_SimChanged;
            //client.Network.SimConnected += Network_SimConnected;
            //client.Self.SimPosition

            //set usename
            usernameText.text = client.Self.Name;
        }

        //its too late bro, the UI is started only after connected to sim has occured.
        private void Network_SimConnected(object sender, SimConnectedEventArgs e)
        {
            //update the user'sname
            usernameText.text = client.Self.Name;
            Debug.Log("Network_SimConnected is raised! wow. not expected. as sim connection should have occured far before this event is registered.");
        }

        private void SaveConfig(object state)
        {


            //s["parcel_audio_url"] = OSD.FromString(txtAudioURL.Text);
            //s["parcel_audio_vol"] = OSD.FromReal(audioVolume);
            //s["parcel_audio_play"] = OSD.FromBoolean(cbPlayAudioStream.Checked);
            //s["parcel_audio_keep_url"] = OSD.FromBoolean(cbKeep.Checked);
            //s["object_audio_vol"] = OSD.FromReal(instance.MediaManager.ObjectVolume);
            s["object_audio_enable"] = OSD.FromBoolean(SoundToggle.isOn);
            //s["ui_audio_vol"] = OSD.FromReal(instance.MediaManager.UIVolume);
        }



        private void OnToggleSounds(bool _)
        {
            OpenMetaverse.Logger.DebugLog("sound toggle is on? : " + _);
            //instance.MediaManager.ObjectEnable = _;
            configTimer.Change(saveConfigTimeout, System.Threading.Timeout.Infinite);
        }

        private void UpdateSimLocDisplay(string sim, OpenMetaverse.Vector3 pos)
        {
            var _x = String.Format("{0:0.00}", pos.X);
            var _y = String.Format("{0:0.00}", pos.Y);
            var _z = String.Format("{0:0.00}", pos.Z);
            locationText.text = sim + " " + _x + " " + _y + " " + _z;

        }

        //private void Network_SimConnected(object sender, SimConnectedEventArgs e)
        //{
        //    simName.Value = client.Network.CurrentSim.Name + ;

        //}

        //private void Network_SimChanged(object sender, SimChangedEventArgs e)
        //{
        //    simName.Value = e.;
        //}

        private void Update()
        {
            if (Active == false)
            {
                //prepare to go back to login screen?
                //do nothing for now.
                return;
            }

            var lastFrameTime = Time.deltaTime;
            timeSinceLastUpdate += lastFrameTime;


            if (timeSinceLastUpdate > 2)
            {
                //do updates
                UpdateSimLocDisplay(client.Network.CurrentSim.Name, client.Self.SimPosition); //update the title to the current sim

                timeSinceLastUpdate = 0;
            }
        }



        private void initialiseFields()
        {
            //map location


        }

        public void OnChatBtnClick()
        {
            uimanager.canvasManager.pushCanvas(CanvasType.Chat);


        }
        public void OnMapBtnClick()
        {
            uimanager.canvasManager.pushCanvas(CanvasType.Map);


        }


        #endregion
    }
}