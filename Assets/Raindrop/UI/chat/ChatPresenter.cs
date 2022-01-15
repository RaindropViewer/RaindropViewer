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
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;


//view(unitytext) -- presenter(this) -- controller(this?) -- model (raindropinstance singleton) 
//5_jan_2022 : view(unity UI) --has a component--> chatpresenter(this, monobehavior)
//                          -> unityPresenterManager(not monobehavior)
//                                -> chattextmaanager(seems to handle all the local chat printing and events.)
//                                     -> chattextprinter(monobehavior. depedency injected all the way down from root.)
namespace Raindrop.Presenters
{
    //this class is attached to the chatview gameobject.
    //it launches the manager that takes care of incoming and outgoing chats.
    public class ChatPresenter : MonoBehaviour
    {
        /* +---------------------------+
         * | Local ^ | [user1] Yo!      |
         * | IM  1 | | [user2] oh hey   |
         * | IM  2 | |       ...       |
         * |       | |____________ ____|
         * |       v | reply...   |SEND|
         *  +-------------------------+
         */
        
        //left pane: scrollable list of chats.
        //right pane: the contents of the selected chat in the left pane.+
        //             input bar of the text to send to said chat.

        private RaindropInstance instance => ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>();
        private RaindropNetcom netcom { get { return instance.Netcom; } }

        private GridClient client => instance.Client;
        bool Active => instance.Client.Network.Connected;

        private ChatPresenterManager _chatPresenterManager;

        #region references to UI elements
        [Tooltip("button to close the chat view")]
        public Button CloseButton;
        [Tooltip("button to send the text box to the chat")]
        public Button SendButton;

        [Tooltip("list of chats. buttons.")]
        public GameObject ChatsListRoot;
        public List<GameObject> ChatButtons;
        
        [Tooltip("user input field")]
        public TMP_InputField ChatInputField;

        [Tooltip("contents of currently-selected chat")]
        public TMP_Text ChatBox;
        [Tooltip("chatbox's printer component")]
        public TMPTextFieldPrinter localChatPrinter;
        #endregion

        #region internal representations 

        private string inputString;
        public GameObject buttonPrefab;

        #endregion

        void Awake()
        {
            CloseButton.onClick.AsObservable().Subscribe(_ => OnCloseBtnClick()); //when clicked, runs this method.
            SendButton.onClick.AsObservable().Subscribe(_ => OnSendBtnClick()); //change username property.
            ChatInputField.onValueChanged.AsObservable().Subscribe(_ => OnInputChanged(_)); //change username property.

            // RegisterClientEvents(client);
            
            _chatPresenterManager = new ChatPresenterManager(instance, localChatPrinter);
            
            AddChatTabToListOfChats("localChat");
        }

        //append another chat to the chat-list.
        public void AddChatTabToListOfChats(string name)
        {
            //add button and set transforms
            var chatButton = Instantiate(buttonPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            ChatButtons.Add(chatButton);
            chatButton.transform.SetParent(ChatsListRoot.transform);
        }

        private void OnDestroy()
        {
            // UnregisterClientEvents(client);

            _chatPresenterManager.Dispose();
            _chatPresenterManager = null;
        }

        private void OnInputChanged(string _)
        {
            inputString = _;
            return;
        }

        private void OnSendBtnClick()
        {
            //public chat
            ProcessChatInput(inputString, ChatType.Normal);
            Debug.Log("Sending localchat to server");
        }

        private void OnCloseBtnClick()
        {
            var uimanager = ServiceLocator.ServiceLocator.Instance.Get<UIService>();
            uimanager.canvasManager.PopCanvas();
        }
        
        //adds a tab for this particular IM session
        public void AddIMTab(UUID target, UUID session, string targetName)
        {
            //IMTabWindow imTab = new IMTabWindow(instance, target, session, targetName);

            //GroupButtons.Add(new IMTextManager(instance,??,IMTextManagerType.Agent,));
            //imTab.SelectIMInput();

            //return imTab;
        }
        
        //send message into local chat
        public void ProcessChatInput(string input, ChatType type)
        {
            if (string.IsNullOrEmpty(input)) return;
         
            //call the ProcessChatInput in the respective manager class.
            if (/*chatList.getSelected() == "local chat"*/ true)
            {
                _chatPresenterManager.LocalChatManager.ProcessChatInput(input, type);

            } 
            //else
            //{

            //    netcom.SendInstantMessage(msg, target, SessionId);
            //    chatHistory.Add(cbxInput.Text);
            //    chatPointer = chatHistory.Count;
            //}
        }

        private void ClearChatInput()
        {
            ChatInputField.text = "";

        }
    }
}