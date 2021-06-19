using Raindrop;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Raindrop.Presenters
{
    //controls and handles input from chat buttons.
    public class ChatButtonPresenter : MonoBehaviour
    {
        public string nameOfChat;
        public IMTextManager manager; //it makes sense for a chat tab to hold the chat manager instance.
        public ChatPresenter chatPane; //it also makes sense that the chat tab holds the right side panel.. i think
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
