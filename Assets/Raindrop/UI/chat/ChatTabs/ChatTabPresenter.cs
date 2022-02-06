using Raindrop;
using System;
using System.Collections;
using System.Collections.Generic;
using OpenMetaverse;
using OpenMetaverse.ImportExport.Collada14;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Raindrop.Presenters
{
    //each tab in the list of chats.
    // if you click it, you select another chat.
    // if a new IM arrives, the tab is highlighted.
    [RequireComponent(typeof(TMP_Text))]
    [RequireComponent(typeof(Image))]
    public class ChatTabPresenter : MonoBehaviour, IPointerDownHandler
    {
        #region Globals
        public static List<ChatTabPresenter> ChatTabList;
        public static ChatTabPresenter CurrentChatTab; //the currently visible chat.
        #endregion
        
        [SerializeField]
        public UUID ChatID;

        public TMP_Text tmp_text;

        public IMPresenter ChatboxUI;
        private Image _image;
        public bool Highlighted { get; set; }

        // Start is called before the first frame update
        void Start()
        {
            //register.
            ChatTabList.Add(this);
            
            tmp_text = this.GetComponent<TMP_Text>();

            tmp_text.text = "loading...";

            _image = this.GetComponent<Image>();
            
            _image.color = Color.white;
            
        }

        public void LinkToChatbox(IMPresenter textbox)
        {
            ChatboxUI = textbox;
        }
        
        public void initChatTab(ChatManager chatManager, UUID chatID, string chatName)
        {
            tmp_text.text = chatName;
            ChatID = chatID;
        }

        // this callback lets the main text view know what to display.
        private void OnTabClicked()
        {
            // disable the one currently open..
            if (ChatTabPresenter.CurrentChatTab != null)
            {
                ChatTabPresenter.CurrentChatTab.DeselectTab();
            }
            //assign myself as active.
            ChatTabPresenter.CurrentChatTab = this;

            //show the associated chatbox UI
            ChatboxUI.gameObject.SetActive(true);
        }

        //show the button as deselected and turn off its associated view.
        private void DeselectTab()
        {
            var v = this.GetComponent<TabButton>();
            v.Deselect();
            
            this.ChatboxUI.gameObject.SetActive(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnTabClicked();
        }

        // tell the user something is new inside here.
        public void PartialHighlight()
        {
            _image.color = Color.yellow;
        }

        public void Unhighlight()
        {
            _image.color = Color.white;
        }

        public void Highlight()
        {
            _image.color = Color.green;
        }
    }
}
