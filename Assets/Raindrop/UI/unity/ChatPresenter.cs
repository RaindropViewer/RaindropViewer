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


//view(unitytext) -- presenter(this) -- controller(this?) -- model (raindropinstance singleton)
namespace Raindrop.Presenters
{

    public class ChatPresenter : MonoBehaviour
    {
        //left pane: scrollable list of chats.
        //right pane: the contents of the selected chat in the left pane.+
        //             input bar of the text to send to said chat.
        private RaindropInstance instance { get { return RaindropInstance.GlobalInstance; } }
        private RaindropNetcom netcom { get { return instance.Netcom; } }

        private GridClient client => instance.Client;

        private Regex chatRegex = new Regex(@"^/(\d+)\s*(.*)", RegexOptions.Compiled);
        private List<string> chatHistory = new List<string>();
        private int chatPointer;

        private List<ChatLogPresenter> listOfChatLogs;
        private ChatLogPresenter selectedChatLog;

        public readonly Dictionary<UUID, ulong> agentSimHandle = new Dictionary<UUID, ulong>();
        private static readonly string LOCALCHATTITLE = "Local Chat";

        public ChatTextManager ChatManager { get; private set; }
        bool Active => instance.Client.Network.Connected;


        #region references to UI elements
        public Button CloseButton;
        public Button SendButton;
        public List<Button> GroupButtons;
        public Button SelectedButton;
        public InputField ChatInputField;
        public TMP_Text ChatBox;

        #endregion

        #region internal representations 



        private List<string> chatTitles= new List<string>
        {
            LOCALCHATTITLE
        };
        private string msgtext;



        #endregion


        // Use this for initialization
        void Start()
        {

            CloseButton.onClick.AsObservable().Subscribe(_ => OnCloseBtnClick()); //when clicked, runs this method.
            SendButton.onClick.AsObservable().Subscribe(_ => OnSendBtnClick()); //change username property.
            ChatInputField.onValueChanged.AsObservable().Subscribe(_ => OnInputChanged(_)); //change username property.


            var chatGO = (IPrintableMonobehavior) ChatBox.gameObject.GetComponent<ITextLikeGameObject>();
            ChatManager = new ChatTextManager(instance, new TextBoxPrinter(chatGO));

            RegisterClientEvents(client);
        }

        private void OnDestroy()
        {


            UnregisterClientEvents(client);
        }

        private void OnInputChanged(string _)
        {
            msgtext = _;
            return;
        }

        private void OnSendBtnClick()
        {
            if (selectedChatLog == null)
                return;

            ProcessChatInput(msgtext,ChatType.Normal);
            return;
        }

        private void OnCloseBtnClick()
        {
            instance.UI.canvasManager.popCanvas();

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

        






        //runs when the send btton is clicked.
        public void ProcessChatInput(string input, ChatType type)
        {
            if (string.IsNullOrEmpty(input)) return;
            chatHistory.Add(input);
            chatPointer = chatHistory.Count;
            ChatManager.TextPrinter.ClearText();
            //ClearChatInput();

            string msg;

            msg = input.Length >= 1000 ? input.Substring(0, 1000) : input;
            //msg = msg.Replace(ChatInputBox.NewlineMarker, Environment.NewLine);

            if (instance.GlobalSettings["mu_emotes"].AsBoolean() && msg.StartsWith(":"))
            {
                msg = "/me " + msg.Substring(1);
            }

            int ch = 0;
            Match m = chatRegex.Match(msg);

            if (m.Groups.Count > 2)
            {
                ch = int.Parse(m.Groups[1].Value);
                msg = m.Groups[2].Value;
            }

            //if (instance.CommandsManager.IsValidCommand(msg))
            //{
            //    instance.CommandsManager.ExecuteCommand(msg);
            //}
            //else
            //{
                #region RLV

                #endregion

                var processedMessage = GestureManager.Instance.PreProcessChatMessage(msg).Trim();
                if (!string.IsNullOrEmpty(processedMessage))
                {
                    netcom.ChatOut(processedMessage, type, ch);
                }
            //}

        }



    }
}