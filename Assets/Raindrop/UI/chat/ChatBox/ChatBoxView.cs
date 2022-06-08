using System;
using System.Collections.Generic;
using System.Linq;
using OpenMetaverse;
using Raindrop.UI.chat;
using Raindrop.UI.chat.printer_component;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Raindrop.Presenters
{
    // i live in the container that contains the chat boxes. I enable and disable chat boxes. Only 1 can exist at one time.   
    public class ChatBoxView : MonoBehaviour
    {
        [FormerlySerializedAs("LocalChatPresenter")] public GameObject LocalChatBox;
        public GameObject _prev;
        public Dictionary<UUID, IMPresenter> ImPresenters;
        
        #region Prefabs
        public GameObject chatTextboxPrefab;
        #endregion

        //open an IM using the uuid provided.
        public void openIM(UUID uuid)
        {
            if (uuid == UUID.Zero)
            {
                //use zero as local chat.
                Show(LocalChatBox);
                UnShow(_prev);
                _prev = LocalChatBox;
            }
            else
            {
                IMPresenter found;
                if (!ImPresenters.TryGetValue(uuid, out found))
                {
                    //IMPresenter IMprez = AddTheChatBox(uuid);
                    //if (IMprez is null) 
                      //  Debug.LogError("impresenter is not attached to the prefab!?");
                    found.Init(uuid); //don't forget this.
                    
                    // found = IMprez.gameObject;
                    ImPresenters.Add(uuid, found);
                }
                Show(found.gameObject);
                UnShow(_prev);
                _prev = found.gameObject;
            }
        }

        private void Show(GameObject go)
        {
            go.SetActive(true);
        }

        private void UnShow(GameObject go)
        {
            go.SetActive(false);
        }

        private IMPresenter AddTheChatBox(UUID uuid)
        {
            IMPresenter res;
            var GO = Instantiate(chatTextboxPrefab, this.transform);
            res = GO.GetComponent<IMPresenter>();
            return res;
        }

        // process IM that, because the IMMangager is not subscribed to the event yet, so we need to raise this manually for the first time.
        public void ProcessIM(InstantMessageEventArgs e, bool b)
        {
            var chat = ImPresenters[e.IM.IMSessionID];
            chat.ProcessIM(e);
        }
    }
}