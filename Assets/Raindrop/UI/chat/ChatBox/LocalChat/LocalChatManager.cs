using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raindrop.Netcom;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
//using System.Web.Script.Serialization;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using Object = System.Object;
using System.Text.RegularExpressions;
using Plugins.CommonDependencies;
using Raindrop.Core;
using Raindrop.Services;
using Raindrop.Services.Bootstrap;
using Raindrop.UI.chat.printer_component;
using Logger = OpenMetaverse.Logger;

namespace Raindrop
{
    //This class orchestrates the printing into a text box.
    //it has a text buffer to hold data (optional).
    public class LocalChatManager : IDisposable
    {
        private Regex chatRegex = new Regex(@"^/(\d+)\s*(.*)", RegexOptions.Compiled);
        private int chatPointer;
        public event EventHandler<ChatLineAddedArgs> ChatLineAdded;

        private RaindropInstance instance;
        private RaindropNetcom netcom => instance.Netcom;
        private GridClient client => instance.Client;

        private List<ChatBufferItem> textBuffer; //a list of all the lines of chat that we have.
        public List<ChatBufferItem> getChatBuffer()
        {
            return textBuffer;
        }
        public ITextPrinter TextPrinter { get; set; }

        public bool isReady => (TextPrinter != null);
        private bool showTimestamps;

        public static Dictionary<string, Settings.FontSetting> fontSettings = 
            new Dictionary<string, Settings.FontSetting>();

        public LocalChatManager(RaindropInstance instance)
        {
            textBuffer = new List<ChatBufferItem>(); // a pipe into the string.
            
            this.instance = instance;
            InitializeConfig();

            // Callbacks
            netcom.ChatReceived += new EventHandler<ChatEventArgs>(netcom_ChatReceived);
            netcom.ChatSent += new EventHandler<ChatSentEventArgs>(netcom_ChatSent);
            netcom.AlertMessageReceived += new EventHandler<AlertMessageEventArgs>(netcom_AlertMessageReceived);
            client.Self.TeleportProgress += new EventHandler<TeleportEventArgs>(Self_TeleportProgress);
            client.Network.SimConnected += Network_SimConnected;

            PrintStartupMessage();
        }

        public void SetPrinter(ITextPrinter textPrinter)
        {
            TextPrinter = textPrinter;
        }
        
        public void Dispose()
        {
            netcom.ChatReceived -= new EventHandler<ChatEventArgs>(netcom_ChatReceived);
            netcom.ChatSent -= new EventHandler<ChatSentEventArgs>(netcom_ChatSent);
            netcom.AlertMessageReceived -= new EventHandler<AlertMessageEventArgs>(netcom_AlertMessageReceived);
            client.Self.TeleportProgress -= new EventHandler<TeleportEventArgs>(Self_TeleportProgress);
        }

        private void InitializeConfig()
        {
            var ui = ServiceLocator.Instance.Get<UIService>();

            Settings s = instance.GlobalSettings;
            
            if (s["chat_timestamps"].Type == OSDType.Unknown)
            {
                s["chat_timestamps"] = OSD.FromBoolean(true);
            }

            GetFontFromSettings();

            showTimestamps = s["chat_timestamps"].AsBoolean();

            s.OnSettingChanged += new Settings.SettingChangedCallback(s_OnSettingChanged);

            void GetFontFromSettings()
            {
                if (s["chat_fonts"].Type == OSDType.Unknown)
                {
                    SaveDefaultFontSettingsToDisk();
                }

                try
                {
                    fontSettings = JsonConvert.DeserializeObject<Dictionary<string, Settings.FontSetting>>(s["chat_fonts"]);
                }
                catch (Exception ex)
                {
                    ui.ModalsManager.showModal_NotificationGeneric("Failed to read chat font settings. Using defaults. " , ex.Message);
                }

                void SaveDefaultFontSettingsToDisk()
                {
                    try
                    {
                        s["chat_fonts"] = JsonConvert.SerializeObject(Settings.DefaultFontSettings);
                    }
                    catch (Exception ex)
                    {
                        ui.ModalsManager.showModal_NotificationGeneric("Failed to save default font settings: ", ex.Message);
                    }
                }
            }
        }

        void s_OnSettingChanged(object sender, SettingsEventArgs e)
        {
            if (e.Key == "chat_timestamps" && e.Value != null)
            {
                showTimestamps = e.Value.AsBoolean();
                ReprintAllText();
            }
            
        }
        
        
        private void Network_SimConnected(object sender, OpenMetaverse.SimConnectedEventArgs e)
        {
            //create local chat.
            Logger.Log("Simulator Connected", Helpers.LogLevel.Info);
            TextPrinter.PrintTextLine("Simulator Connected");
        }

        
        void Self_TeleportProgress(object sender, TeleportEventArgs e)
        {
            if (e.Status == TeleportStatus.Progress || e.Status == TeleportStatus.Finished)
            {
                TextPrinter.PrintTextLine("teleport... " + e.Message.ToString());
            }
        }

        private void netcom_ChatSent(object sender, ChatSentEventArgs e)
        {
            //because local, visible chats are going to be echoed back to us by the server, we don't display it.
            if (e.Channel == 0) return; 
            
            ProcessOutgoingChat(e);
        }

        private void netcom_AlertMessageReceived(object sender, AlertMessageEventArgs e)
        {
            //if (e.Message.ToLower().Contains("autopilot canceled")) return; //workaround the stupid autopilot alerts

            ChatBufferItem item = new ChatBufferItem(
                DateTime.Now, "Alert message", UUID.Zero, ": " + e.Message, ChatBufferTextStyle.Alert);

            ProcessBufferItem(item, true);
        }

        private void netcom_ChatReceived(object sender, ChatEventArgs e)
        {
            ProcessIncomingChat(e);
        }

        public void PrintStartupMessage()
        {
            ChatBufferItem title = new ChatBufferItem(
                DateTime.Now, "",
                UUID.Zero,
                Globals.FullAppName, 
                ChatBufferTextStyle.StartupTitle);

            ChatBufferItem ready = new ChatBufferItem(
                DateTime.Now, "", UUID.Zero, "Local chat Ready.", ChatBufferTextStyle.StatusBlue);

            ProcessBufferItem(title, true);
            ProcessBufferItem(ready, true);
        }

        private Object SyncChat = new Object();

        // append the buffer item into the chat.
        // optionally, append buffer item into buffer list.
        public void ProcessBufferItem(ChatBufferItem item, bool addToBuffer)
        {
            ChatLineAdded?.Invoke(this, new ChatLineAddedArgs(item));

            lock (SyncChat)
            {
                instance.LogClientMessage("chat.txt", item.From + item.Text);
                if (addToBuffer) textBuffer.Add(item);

                if (showTimestamps)
                {
                    TextPrinter.PrintText(
                        item.Timestamp.ToString("[HH:mm] "), 
                        UnityEngine.Color.gray
                        );
                }
                
                if (item.Style == ChatBufferTextStyle.Normal && item.ID != UUID.Zero && instance.GlobalSettings["av_name_link"])
                {
                    TextPrinter.InsertLink(item.From, $"secondlife:///app/agent/{item.ID}/about");
                }
                else
                {
                    TextPrinter.PrintText(item.From, Color.black);
                }

                Color color = Color.black;
                if(fontSettings.ContainsKey(item.Style.ToString()))
                {
                    var fontSetting = fontSettings[item.Style.ToString()];
                    color = fontSetting.ForeColor;
                }
                
                // TextPrinter.PrintText(item.Timestamp.ToString("[HH:mm] "));
                // TextPrinter.PrintText(item.From);
                TextPrinter.PrintTextLine(item.Text, color);
            }
        }


        //process sending-out of local chat
        internal void ProcessChatInput(string input, ChatType type)
        {
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

            var processedMessage = GestureManager.Instance.PreProcessChatMessage(msg).Trim();
            if (!string.IsNullOrEmpty(processedMessage))
            {
                netcom.ChatOut(processedMessage, type, ch);
            }

        }
        
        //Used only for non-public chat
        private void ProcessOutgoingChat(ChatSentEventArgs e)
        {
            StringBuilder sb = new StringBuilder();

            switch (e.Type)
            {
                case ChatType.Normal:
                    sb.Append(": ");
                    break;

                case ChatType.Whisper:
                    sb.Append(" whisper: ");
                    break;

                case ChatType.Shout:
                    sb.Append(" shout: ");
                    break;
            }

            sb.Append(e.Message);

            ChatBufferItem item = new ChatBufferItem(
                DateTime.Now, $"(channel {e.Channel}) {client.Self.Name}", client.Self.AgentID, sb.ToString(), ChatBufferTextStyle.StatusDarkBlue);

            ProcessBufferItem(item, true);

            sb = null;
        }

        private void ProcessIncomingChat(ChatEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Message)) return;

            // Check if the sender agent is muted
            if (e.SourceType == ChatSourceType.Agent &&
                null != client.Self.MuteList.Find(me => me.Type == MuteType.Resident && me.ID == e.SourceID)
                ) return;

            // Check if it's script debug
            if (e.Type == ChatType.Debug && !instance.GlobalSettings["show_script_errors"])
            {
                return;
            }

            // Check if sender object is muted
            if (e.SourceType == ChatSourceType.Object &&
                null != client.Self.MuteList.Find(me =>
                    (me.Type == MuteType.Resident && me.ID == e.OwnerID) // Owner muted
                    || (me.Type == MuteType.Object && me.ID == e.SourceID) // Object muted by ID
                    || (me.Type == MuteType.ByName && me.Name == e.FromName) // Object muted by name
                )) return;

            ChatBufferItem item = new ChatBufferItem {ID = e.SourceID, RawMessage = e};
            StringBuilder sb = new StringBuilder();

            item.From = e.SourceType == ChatSourceType.Agent 
                ? instance.Names.Get(e.SourceID, e.FromName) 
                : e.FromName;

            bool isEmote = e.Message.ToLower().StartsWith("/me ");

            AppendShoutOrWhisper();

            AppendTheMessage();

            item.Timestamp = DateTime.Now;
            item.Text = sb.ToString();

            StyleTextBySourceType();

            ProcessBufferItem(item, true);
            //instance.TabConsole.Tabs["chat"].Highlight();

            sb = null;

            void AppendShoutOrWhisper()
            {
                if (!isEmote)
                {
                    switch (e.Type)
                    {
                        case ChatType.Whisper:
                            sb.Append(" whispers");
                            break;

                        case ChatType.Shout:
                            sb.Append(" shouts");
                            break;
                    }
                }
            }

            void AppendTheMessage()
            {
                if (isEmote)
                {
                    sb.Append(e.Message.Substring(3));
                }
                else
                {
                    sb.Append(": ");
                    sb.Append(e.Message);
                }
            }

            void StyleTextBySourceType()
            {
                switch (e.SourceType)
                {
                    case ChatSourceType.Agent:
                        if (e.FromName.EndsWith("Linden"))
                        {
                            item.Style = ChatBufferTextStyle.LindenChat;
                        }
                        else if (isEmote)
                        {
                            item.Style = ChatBufferTextStyle.Emote;
                        }
                        else if (e.SourceID == client.Self.AgentID)
                        {
                            item.Style = ChatBufferTextStyle.Self;
                        }
                        else
                        {
                            item.Style = ChatBufferTextStyle.Normal;
                        }

                        break;
                    case ChatSourceType.Object:
                        if (e.Type == ChatType.OwnerSay)
                        {
                            if (isEmote)
                            {
                                item.Style = ChatBufferTextStyle.Emote;
                            }
                            else
                            {
                                item.Style = ChatBufferTextStyle.OwnerSay;
                            }
                        }
                        else if (e.Type == ChatType.Debug)
                        {
                            item.Style = ChatBufferTextStyle.Error;
                        }
                        else
                        {
                            item.Style = ChatBufferTextStyle.ObjectChat;
                        }

                        break;
                }
            }
        }

        //reprint all the text in the printer.
        public void ReprintAllText()
        {
            TextPrinter.ClearText();

            foreach (ChatBufferItem item in textBuffer)
            {
                ProcessBufferItem(item, false);
            }
        }

        public void ClearInternalBuffer()
        {
            textBuffer.Clear();
        }

    }

    public class ChatLineAddedArgs : EventArgs
    {
        public ChatBufferItem Item { get; }

        public ChatLineAddedArgs(ChatBufferItem item)
        {
            Item = item;
        }
    }
}
