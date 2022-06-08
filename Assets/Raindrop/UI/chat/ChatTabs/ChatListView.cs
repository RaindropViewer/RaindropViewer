using System.Collections.Generic;
using OpenMetaverse;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Raindrop.Presenters
{
    // a list of chats.
    [RequireComponent(typeof(ScrollRect))]
    public class ChatListView : MonoBehaviour
    {
        // root transform of the list of chats
        [FormerlySerializedAs("ChatsListRoot")] public Transform chatsListRoot;
        public Dictionary<string, HighlightableTabPresenter> ChatTabs =
            new Dictionary<string, HighlightableTabPresenter>();
        
        //add the chats to this view container:
        // public ScrollRect ViewContainer;

        [FormerlySerializedAs("prefabChatButton")] public GameObject chatTab;
        private ChatPresenter _chatPresenter;

        //add chat tab.
        public void AddIMTab(UUID chatID, UUID SessionID, string name)
        {
            GameObject button = Instantiate(chatTab, chatsListRoot);
            HighlightableTabPresenter buttonPresenter = button.GetComponent<HighlightableTabPresenter>();
            buttonPresenter.Init(_chatPresenter, chatID, name);
            ChatTabs.Add(name.ToLower(), buttonPresenter);
        }
        
        //remove chat tab.
        public void Remove(string name)
        {
            if(ChatTabs[name.ToLower()] != null)
                //destroy.
                Destroy(ChatTabs[name.ToLower()]);

        }

        public bool ContainsKey(string key)
        {
            return ChatTabs.ContainsKey(key.ToLower());
        }
        
        public void FlashChat(string imFromAgentName)
        {
            //todo
            // chatButtons[imFromAgentName].Flash();
        }

        public void Initialise(ChatPresenter chatPresenter)
        {
            this._chatPresenter = chatPresenter;
        }
    }
    
}