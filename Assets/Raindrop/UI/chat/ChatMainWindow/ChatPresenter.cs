using OpenMetaverse;
using Plugins.CommonDependencies;
using Raindrop.Services;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

//view(unitytext) -- presenter(this) -- controller(this?) -- model (raindropinstance singleton) 
//5_jan_2022 : view(unity UI) --has a component--> chatpresenter(this, monobehavior)
//                          -> chatPresenterManager(not monobehavior)
//                                -> chattextmaanager(seems to handle all the local chat printing and events.)
//                                     -> chattextprinter(monobehavior. depedency injected all the way down from root.)
namespace Raindrop.Presenters
{
    // this class tracks and adds chats that are currently active.
    public class ChatPresenter : MonoBehaviour, IInitialisable
    {
        /* +---------------------------+
         * | Local ^ | [user1] Yo!      |
         * | IM  1 | | [user2] oh hey   |
         * | IM  2 | |       ...       |
         * |       | |____________ ____|
         * | (+)   v | reply...   |SEND|
         *  +-------------------------+
         * left : chat list
         * right: chat box, owned by one of the chats in chat list
         */
        
        //left pane: scrollable list of chats.
        //right pane: the contents of the selected chat in the left pane.+
        //             input bar of the text to send to said chat.

        private RaindropInstance instance => ServiceLocator.Instance.Get<RaindropInstance>();

        #region UI elements - left tabs 
        [FormerlySerializedAs("chatList")] [Tooltip("The container of the list of chat tabs.")]
        public ChatListView chatListView;
        //public GameObject PrefabChatButton;
        #endregion
        
        #region UI elements - right chatbox
        [Tooltip("The container that holds the chat-box.")]
        public ChatBoxView ChatBoxView;
        // public Transform ChatboxesRoot;
        #endregion
        
        #region UI elements - 'new chat; Plus' overlay 
        [Tooltip("The button that opens up the 'new chat' view.")]
        public Button NewChatBtn;
        [FormerlySerializedAs("ChatChooserPrefab")] [SerializeField] public GameObject ChatChooserModal;
        #endregion

        private ChatManager manager;

        // private ChatManager ChatManager => instance.ChatManger;
        //[FormerlySerializedAs("PrefabMainChat")] [SerializeField] public GameObject MainChat;
        
        public void Initialise()
        {
            manager = new ChatManager(this);
            
            chatListView.Initialise(this);

            NewChatBtn.onClick.AddListener(OnRequestNewChat);

        }

        private void OnRequestNewChat()
        {
            // string nameModal = "NewChatChooser";
            ServiceLocator.Instance.Get<UIService>()
                .ModalsManager.Show(ChatChooserModal);
        }

        //append another chat to the chat-list.
        // public void AddChatToActiveChatsList(string name)
        // {
        //     //add left side button
        //     var chatTab = Instantiate(buttonPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        //     var behavior = chatTab.GetComponent<ChatTabPresenter>();
        //     ChatButtons.Add(behavior);
        //     chatTab.transform.SetParent(ChatsListRoot.transform);
        //     
        //     //construct and attach the textbox view to the chatbutton
        //     GameObject chatTextbox = (GameObject)Instantiate(
        //         chatTextboxPrefab, 
        //         new Vector3(0, 0, 0), Quaternion.identity);
        //     var presenter = chatTextbox.GetComponent<IMPresenter>();
        //     behavior.LinkToChatbox(presenter);
        //     
        //     
        // }

        private void OnDestroy()
        {
            // UnregisterClientEvents(client);

            // _chatPresenterManager.Dispose();
            // _chatPresenterManager = null;
        }

        //adds a tab for this particular IM session
        // target: who to talk to.
        // label : label on the tab UI
        public void AddIMTab(UUID target, UUID session, string label)
        {
            chatListView.AddIMTab(target, session, label);
        }
        
        // flash the chat - make it blink.
        public void FlashChat(string imFromAgentName)
        {
            chatListView.FlashChat(imFromAgentName);
        }

        //open some chat.
        ////used by the tab-buttons to open the chatbox.

        public void OnShowChat(UUID chatID)
        {
            ChatBoxView.openIM(chatID);
        }

        public void CreateMainChatTab()
        {
            AddIMTab(UUID.Zero, UUID.Zero, "Local Chat");
        }
    }

    public interface IInitialisable
    {
        public void Initialise();
    }
}