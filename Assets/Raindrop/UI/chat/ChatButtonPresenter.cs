using Raindrop;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Raindrop.Presenters
{
    //controls and handles input from chat buttons.
    //this is attached to a chat button
    [RequireComponent(typeof(Button))]
    public class ChatButtonPresenter : MonoBehaviour
    {
        //name to display.
        public string nameOfChat;
        //the chat button holds the printer for the chat window.
        public IMTextManager manager; 
        
        public ChatPresenter chatPane; 
        public bool isSelected;

        // Start is called before the first frame update
        void Start()
        {
            nameOfChat = "???";


            this.gameObject.GetComponent<Button>().onClick.AddListener(() => buttonCallBack(this.gameObject));
        }

        private void buttonCallBack(GameObject gameObject)
        {
            if (isSelected)
            {
                //chatPane.Set
            }
            else
            {

            }
            //manager.show();
        }

        public void subscribe(IMTextManager manager)
        {
            this.manager = manager;

        }

        private void OnDestroy()
        {

        }

    }
}
