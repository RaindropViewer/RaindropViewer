using Raindrop;
using System;
using System.Collections;
using System.Collections.Generic;
using OpenMetaverse;
using OpenMetaverse.ImportExport.Collada14;
using Plugins.CommonDependencies;
using Raindrop.Services;
using Raindrop.UI.chat;
using Raindrop.UI.chat.UI_tabs;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Raindrop.Presenters
{
    // each tab in the list of chats.
    // if you click it, you select another chat.
    // if a new IM arrives, the tab is created(if not already present) and then highlighted.
    [RequireComponent(typeof(TMP_Text))]
    [RequireComponent(typeof(Image))]
    public class HighlightableTabPresenter : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IPointerUpHandler, IHighlightableTabUI
    {
        #region Globals
        public static List<HighlightableTabPresenter> ChatTabList;
        public static HighlightableTabPresenter CurrentHighlightableTab; //the currently visible chat.
        #endregion
        
        [SerializeField]
        public UUID ChatID;
        
        public TMP_Text tmp_text;

        private UUID imageUUID;
        private Image _image;
        private ChatPresenter chatPrez => ServiceLocator.Instance.Get<UIService>().chatFacade;

        //private IMPresenter Presenter;
        public bool Highlighted { get; set; }

        public void Init(ChatPresenter presenter, UUID chatID, string name)
        {
            Init(presenter, chatID, name, UUID.Zero);
        }
        
        public void Init(ChatPresenter chatPresenter, UUID chatID, string name, UUID image)
        {
            tmp_text = this.GetComponent<TMP_Text>();
            tmp_text.text = name;
            _image = this.GetComponent<Image>();
            _image.color = Color.white;
            this.ChatID = chatID;

            imageUUID = image;
        }
        

        private void OnTabClicked()
        {
            // disable the one currently open..
            if (HighlightableTabPresenter.CurrentHighlightableTab != null)
            {
                HighlightableTabPresenter.CurrentHighlightableTab.OnDeselectTab();
            }
            //assign myself as active.
            HighlightableTabPresenter.CurrentHighlightableTab = this;

            //show the associated chatbox UI
            chatPrez.OnShowChat(ChatID);
            Highlight();
            //ChatboxUI.gameObject.SetActive(true);
        }

        //show the button as deselected and turn off its associated view.
        private void OnDeselectTab()
        {
            var v = this.GetComponent<TabButton>();
            v.Deselect();
            Unhighlight();
            
            // this.ChatboxUI.SetActive(false);
        }

        //just flash a different color on touching it.
        public void OnPointerDown(PointerEventData eventData)
        {
            PartialHighlight();
        }
        
        //just flash a different color on not-touching it.
        public void OnPointerUp(PointerEventData eventData)
        {
            Unhighlight();
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

        public void SelectTab()
        {
            throw new NotImplementedException();
        }


        // Does: open the chat.
        public void OnPointerClick(PointerEventData eventData)
        {
            OnTabClicked();

            // chatPrez.openIM(name);
        }
    }

    // interface that defines the visual aspects of chat tabs
    public interface IHighlightableTabUI
    {
        bool Highlighted { get; set; }
        void PartialHighlight();
        void Unhighlight();
        void Highlight();

        void SelectTab();
    }
}
