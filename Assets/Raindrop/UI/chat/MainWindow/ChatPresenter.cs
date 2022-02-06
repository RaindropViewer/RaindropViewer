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
using System.Text.RegularExpressions;
using Raindrop.Core;
using Raindrop.Services;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;


//view(unitytext) -- presenter(this) -- controller(this?) -- model (raindropinstance singleton) 
//5_jan_2022 : view(unity UI) --has a component--> chatpresenter(this, monobehavior)
//                          -> chatPresenterManager(not monobehavior)
//                                -> chattextmaanager(seems to handle all the local chat printing and events.)
//                                     -> chattextprinter(monobehavior. depedency injected all the way down from root.)
namespace Raindrop.Presenters
{
    // this class tracks and adds chats that are currently active.
    public class ChatPresenter : MonoBehaviour
    {
        /* +---------------------------+
         * | Local ^ | [user1] Yo!      |
         * | IM  1 | | [user2] oh hey   |
         * | IM  2 | |       ...       |
         * |       | |____________ ____|
         * |       v | reply...   |SEND|
         *  +-------------------------+
         * left : chat list
         * right: chat box, owned by one of the chats in chat list
         */
        
        //left pane: scrollable list of chats.
        //right pane: the contents of the selected chat in the left pane.+
        //             input bar of the text to send to said chat.

        private RaindropInstance instance => ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>();
       
        bool Active => instance.Client.Network.Connected;

        private ChatManager _chatManager;

        #region UI elements
        [Tooltip("The root container of the list of chats. container of the buttons.")]
        public GameObject ChatsListRoot;
        public List<ChatTabPresenter> ChatButtons;
        
        [Tooltip("The container to add the chat-textbox to.")]
        public GameObject ChatboxesRoot;
        #endregion

        #region Prefabs
        public GameObject buttonPrefab;
        public GameObject chatTextboxPrefab;
        #endregion

        //link up children and components.
        void Awake()
        {
            // RegisterClientEvents(client);
            
            // AddChatToActiveChatsList("localChat");
        }

        private void Start()
        {
            // the manager class remembers which chats are open.
            // you can open and close chats from code using this manager.
            // _chatManager = new ChatManager(instance);

            _chatManager = instance.ChatManger;
        }

        //append another chat to the chat-list.
        public void AddChatToActiveChatsList(string name)
        {
            //add left side button
            var chatTab = Instantiate(buttonPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            var behavior = chatTab.GetComponent<ChatTabPresenter>();
            ChatButtons.Add(behavior);
            chatTab.transform.SetParent(ChatsListRoot.transform);
            
            //construct and attach the textbox view to the chatbutton
            GameObject chatTextbox = (GameObject)Instantiate(
                chatTextboxPrefab, 
                new Vector3(0, 0, 0), Quaternion.identity);
            var presenter = chatTextbox.GetComponent<IMPresenter>();
            behavior.LinkToChatbox(presenter);
            
            
        }

        private void OnDestroy()
        {
            // UnregisterClientEvents(client);

            // _chatPresenterManager.Dispose();
            // _chatPresenterManager = null;
        }

        //adds a tab for this particular IM session
        public void AddIMTab(UUID target, UUID session, string targetName)
        {
            //IMTabWindow imTab = new IMTabWindow(instance, target, session, targetName);

            //GroupButtons.Add(new IMTextManager(instance,??,IMTextManagerType.Agent,));
            //imTab.SelectIMInput();

            //return imTab;
        }
        
    }
}