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


//view(unitytext) -- presenter(this) -- controller(this?) -- model (raindropinstance singleton)
namespace Raindrop.Presenters
{
    public class GamePresenter : MonoBehaviour
    {
        //what does the main user interaction have with the 3d env?
        //1 . movements - joy, run toggle, jump, fly toggle, fly-U/D, 
        //2 . notifications - a dragdown list-like UI
        //3 . buttons to access other features.

        private RaindropInstance instance { get { return RaindropInstance.GlobalInstance; } }
        private RaindropNetcom netcom { get { return instance.Netcom; } }
        private GridClient client { get { return instance.Client; } }

        bool Active => instance.Client.Network.Connected;


        #region references to UI elements
        //buttons that open up other canvas.
        public Button ChatButton; 
        public Button MapButton; 
        
        //ui elements that just dock to the screen.
        public Image MapPane;
        public TMP_Text locationText;
        public MinimapModule minimap;

        public VariableJoystick variableJoystick;

        public UnityEngine.Vector2 jsDir;


        #endregion

        //camera data
        //public Vector3 cameraloc { get { return instance.cameraLoc; } }




        #region behavior

        // Use this for initialization
        void Start()
        {

            initialiseFields();

            ChatButton.onClick.AsObservable().Subscribe(_ => OnChatBtnClick()); //when clicked, runs this method.
            MapButton.onClick.AsObservable().Subscribe(_ => OnChatBtnClick()); //when clicked, runs this method.




        }


        private void Update()
        {
            float vert = variableJoystick.Vertical;
            float horz = variableJoystick.Horizontal;

            float thresh = 0.7f;
            if (vert > thresh)
            {
                instance.Movement.MovingForward = true;
                instance.Movement.MovingBackward = false;

            }
            else if (vert < -thresh)
            {
                instance.Movement.MovingBackward = true;
                instance.Movement.MovingForward = false;

            }
            else
            {
                instance.Movement.MovingBackward = false;
                instance.Movement.MovingForward= false;

            }

            if (horz > thresh)
            {
                instance.Movement.TurningRight= true;
                instance.Movement.TurningLeft = false;

            }
            else if (horz < -thresh)
            {
                instance.Movement.TurningLeft = true;
                instance.Movement.TurningRight = false;

            }
            else
            {
                instance.Movement.TurningLeft = false;
                instance.Movement.TurningRight= false;

            }


        }



        private void initialiseFields()
        {
            //map location


        }

        public void OnChatBtnClick()
        {
            instance.MainCanvas.canvasManager.pushCanvas(CanvasType.Chat);


        }
        public void OnMapBtnClick()
        {
            instance.MainCanvas.canvasManager.pushCanvas(CanvasType.Map);


        }


        #endregion





    }
}