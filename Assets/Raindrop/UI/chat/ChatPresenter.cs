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
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;


//view(unitytext) -- presenter(this) -- controller(this?) -- model (raindropinstance singleton)
namespace Raindrop.Presenters
{
    //this class is attached to the chatview gameobject.
    //it launches the manager that takes care of incoming and outgoing chats.
    public class ChatPresenter : MonoBehaviour
    {
        //left pane: scrollable list of chats.
        //right pane: the contents of the selected chat in the left pane.+
        //             input bar of the text to send to said chat.

        private RaindropInstance instance => ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>();
        private RaindropNetcom netcom { get { return instance.Netcom; } }

        private GridClient client => instance.Client;

        bool Active => instance.Client.Network.Connected;

        private ChatManager chatManager;

        public readonly Dictionary<UUID, ulong> agentSimHandle = new Dictionary<UUID, ulong>();
        private static readonly string LOCALCHATTITLE = "Local Chat";

        public int selectedChatIdx = -1; //-1 means public chat


        #region references to UI elements
        [Tooltip("button to close the chat view")]
        public Button CloseButton;
        [Tooltip("button to send the text box to the chat")]
        public Button SendButton;

        [Tooltip("list of chats. buttons.")]
        public GameObject ChatButtonContainer;
        public List<GameObject> ChatButtons;
        
        //public GameObject SelectedButton;
        [Tooltip("user types here.")]
        public TMP_InputField ChatInputField;

        [Tooltip("show the current chat the user is viewing")]
        public TMP_Text ChatBox;
        //the printer that is printing into the big chat box.
        public TMPTextFieldPrinter localChatPrinter;
        #endregion

        #region internal representations 



        private List<string> chatTitles= new List<string>
        {
            LOCALCHATTITLE
        };
        private string msgtext;
        public GameObject buttonPrefab;



        #endregion


        // Use this for initialization
        void Start()
        {
            CloseButton.onClick.AsObservable().Subscribe(_ => OnCloseBtnClick()); //when clicked, runs this method.
            SendButton.onClick.AsObservable().Subscribe(_ => OnSendBtnClick()); //change username property.
            ChatInputField.onValueChanged.AsObservable().Subscribe(_ => OnInputChanged(_)); //change username property.

            RegisterClientEvents(client);

            //check if printer is attached to textbox. else attach it.
            localChatPrinter = ChatBox.gameObject.GetComponent<TMPTextFieldPrinter>();
            if (!localChatPrinter)
            {
                localChatPrinter = ChatBox.gameObject.AddComponent<TMPTextFieldPrinter>(); 
                //OpenMetaverse.Logger.Log("failed to make the tmp_printer component to the tmp textbox.", Helpers.LogLevel.Error);
            }
            
            chatManager = new ChatManager(instance, localChatPrinter);

            // chatManager.localChatManager.ChatLineAdded += LocalChatManager_ChatLineAdded;

            //startChat(true);
        }

        // private void LocalChatManager_ChatLineAdded(object sender, ChatLineAddedArgs e)
        // {
        //     //new message in local chat! print it.
        //     ChatBufferItem item = e.Item;
        //     runPrinterOnItem(tmp_printer,  item);
        // }

        //id relative to the list. -1 means local. 0 means others.
        public void setShowingChat(int id)
        {
            if (id == -1)
            {
                //print everything in text buffer list.
                List<ChatBufferItem> x = chatManager.localChatManager.getChatBuffer();
                // runPrinterOnList(tmp_printer, x, 0,20); //print the selected range into the UI
            }

        }
        //
        // public void runPrinterOnItem(TMPTextFieldPrinter TextPrinter, ChatBufferItem item)
        // {
        //     //ChatBufferItem item = x[i];
        //
        //     if (/*showTimestamps*/ true)
        //     {
        //         //if(fontSettings.ContainsKey("Timestamp"))
        //         //{
        //         //    var fontSetting = fontSettings["Timestamp"];
        //         //    TextPrinter.ForeColor = fontSetting.ForeColor;
        //         //    TextPrinter.BackColor = fontSetting.BackColor;
        //         //    TextPrinter.Font = fontSetting.Font;
        //         //    TextPrinter.PrintText(item.Timestamp.ToString("[HH:mm] "));
        //         //}
        //         //else
        //         //{
        //         //TextPrinter.ForeColor = SystemColors.GrayText;
        //         //TextPrinter.BackColor = Color.Transparent;
        //         //TextPrinter.Font = Settings.FontSetting.DefaultFont;
        //         TextPrinter.PrintText(item.Timestamp.ToString("[HH:mm] "));
        //         //}
        //     }
        //
        //     //if(fontSettings.ContainsKey("Name"))
        //     //{
        //     //    var fontSetting = fontSettings["Name"];
        //     //    TextPrinter.ForeColor = fontSetting.ForeColor;
        //     //    TextPrinter.BackColor = fontSetting.BackColor;
        //     //    TextPrinter.Font = fontSetting.Font;
        //     //}
        //     //else
        //     //{
        //     //TextPrinter.ForeColor = SystemColors.WindowText;
        //     //TextPrinter.BackColor = Color.Transparent;
        //     //TextPrinter.Font = Settings.FontSetting.DefaultFont;
        //     //}
        //
        //     if (item.Style == ChatBufferTextStyle.Normal && item.ID != UUID.Zero && instance.GlobalSettings["av_name_link"])
        //     {
        //         TextPrinter.InsertLink(item.From, $"secondlife:///app/agent/{item.ID}/about");
        //     }
        //     else
        //     {
        //         TextPrinter.PrintText(item.From);
        //     }
        //
        //     //if(fontSettings.ContainsKey(item.Style.ToString()))
        //     //{
        //     //    var fontSetting = fontSettings[item.Style.ToString()];
        //     //    TextPrinter.ForeColor = fontSetting.ForeColor;
        //     //    TextPrinter.BackColor = fontSetting.BackColor;
        //     //    TextPrinter.Font = fontSetting.Font;
        //     //}
        //     //else
        //     //{
        //     //    TextPrinter.ForeColor = SystemColors.WindowText;
        //     //    TextPrinter.BackColor = Color.Transparent;
        //     //    TextPrinter.Font = Settings.FontSetting.DefaultFont;
        //     //}
        //
        //     TextPrinter.PrintTextLine(item.Text);
        // }
        //
        // //you can run the printer when a new text comes in.
        // //or you can run it when the user opens a new chat and you gotta update the chat text.
        // public void runPrinterOnList(TMPTextFieldPrinter TextPrinter, List<ChatBufferItem> x, int rangeNew, int rangeOld)
        // {
        //     for (int i = rangeNew; i < rangeOld; i++)
        //     {
        //         runPrinterOnItem(TextPrinter, x[i]);
        //     }
        // }

        //adds another chat to the left side
        public void startChat(bool isSimChat)
        {
            if (isSimChat)
            {
                //add button and set transforms
                var chatButton = Instantiate(buttonPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                ChatButtons.Add(chatButton);
                //SelectedButton = chatButton;
                chatButton.transform.SetParent(ChatButtonContainer.transform);

                //setup internal lcoalchat manager.
                
                //ChatManager = new ChatTextManager(instance, tmp_printer); //you gotta pass the printer, not the gameobject.

            } else
            {
                //var chatButton = Instantiate(buttonPrefab, new Vector3(0, 0, 0), Quaternion.identity);

                //ChatButtons.Add(chatButton);
                //SelectedButton = chatButton;
                //chatButton.transform.SetParent(ChatButtonContainer.transform);

                ////IM = new ChatTextManager(instance, tmp_printer); //you gotta pass the printer, not the gameobject.


            }

        }

        private void OnDestroy()
        {


            UnregisterClientEvents(client);

            chatManager.Dispose();
            chatManager = null;
        }

        private void OnInputChanged(string _)
        {
            msgtext = _;
            return;
        }

        private void OnSendBtnClick()
        {
            //public chat
            ProcessChatInput(msgtext, ChatType.Normal);
            Debug.Log("Sending chat to local");

            chatManager.printToMainChat("sending message (test)");
            
        }

        private void OnCloseBtnClick()
        {
            var uimanager = ServiceLocator.ServiceLocator.Instance.Get<UIService>();
            uimanager.canvasManager.popCanvas();

        }

        private void RegisterClientEvents(GridClient client)
        {
            //client.Grid.CoarseLocationUpdate += new EventHandler<CoarseLocationUpdateEventArgs>(Grid_CoarseLocationUpdate);
            client.Self.TeleportProgress += new EventHandler<TeleportEventArgs>(Self_TeleportProgress);
            client.Network.SimDisconnected += new EventHandler<SimDisconnectedEventArgs>(Network_SimDisconnected);
        }

        private void UnregisterClientEvents(GridClient client)
        {
            //client.Grid.CoarseLocationUpdate -= new EventHandler<CoarseLocationUpdateEventArgs>(Grid_CoarseLocationUpdate);
            client.Self.TeleportProgress -= new EventHandler<TeleportEventArgs>(Self_TeleportProgress);
            client.Network.SimDisconnected -= new EventHandler<SimDisconnectedEventArgs>(Network_SimDisconnected);
        }

        //adds a tab for this particular IM session
        public void AddIMTab(UUID target, UUID session, string targetName)
        {
            //IMTabWindow imTab = new IMTabWindow(instance, target, session, targetName);

            //GroupButtons.Add(new IMTextManager(instance,??,IMTextManagerType.Agent,));
            //imTab.SelectIMInput();

            //return imTab;
        }

        void Self_TeleportProgress(object sender, TeleportEventArgs e)
        {
            if (e.Status == TeleportStatus.Progress || e.Status == TeleportStatus.Finished)
            {
                //ResetAvatarList();
            }
        }
        private void Network_SimDisconnected(object sender, SimDisconnectedEventArgs e)
        {
            try
            {

                //if (InvokeRequired)
                //{
                //    if (!instance.MonoRuntime || IsHandleCreated)
                //        BeginInvoke(new MethodInvoker(() => Network_SimDisconnected(sender, e)));
                //    return;
                //}
                lock (agentSimHandle)
                {
                    var h = e.Simulator.Handle;
                    List<UUID> remove = new List<UUID>();
                    foreach (var uh in agentSimHandle)
                    {
                        if (uh.Value == h)
                        {
                            remove.Add(uh.Key);
                        }
                    }
                    if (remove.Count == 0) return;
                   
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to update radar: " + ex);
            }
        }

        






        //process the content of the inputfield to send to the simulator as local chat
        public void ProcessChatInput(string input, ChatType type)
        {
            if (string.IsNullOrEmpty(input)) return;
         
            //call the ProcessChatInput in the respective manager class.
            if (/*chatList.getSelected() == "local chat"*/ true)
            {
                chatManager.localChatManager.ProcessChatInput(input, type);

            } 
            //else
            //{

            //    netcom.SendInstantMessage(msg, target, SessionId);
            //    chatHistory.Add(cbxInput.Text);
            //    chatPointer = chatHistory.Count;
            //}

             
            ClearChatInput();


        }

        private void ClearChatInput()
        {
            ChatInputField.text = "";

        }
    }
}