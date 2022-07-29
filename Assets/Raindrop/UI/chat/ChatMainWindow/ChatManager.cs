using System;
using System.Collections;
using OpenMetaverse;
using Plugins.CommonDependencies;
using Raindrop.Netcom;
using Raindrop.Presenters;
using Raindrop.Services;
using UnityEngine;
using Logger = OpenMetaverse.Logger;

namespace Raindrop
{
    //subscribes to incoming chats, then it updates the view in response.
    
    //notes:
    //because of the very 'flexible' nature of the incoming chats, this class has been implemented nearer to the UI layer.
    public class ChatManager: IDisposable
    {
        private RaindropNetcom netcom { get { return instance.Netcom; } }
        private GridClient client => instance.Client;

        private UIService ui => ServiceLocator.Instance.Get<UIService>();

        //public ChatTabs Tabs = new ChatTabs();
        //public ChatTabPresenter MainTab;
        public LocalChatManager MainChatManger = null;
        
        private ChatPresenter presenter;
        public ChatListView chatListView => presenter.chatListView;


        RaindropInstance instance => RaindropInstance.GlobalInstance;
        public ChatManager(ChatPresenter view)
        {
            this.presenter = view;
            //subscribe to incoming IMs, where we open the IM in the view.
            instance.Netcom.InstantMessageReceived += NetcomOnInstantMessageReceived;
            instance.Netcom.ChatReceived += NetcomOnChatReceived;
            
            // MainChatManger = view.CreateMainChatTab();
        }
        
        public void Dispose()
        {
            instance.Netcom.InstantMessageReceived -= NetcomOnInstantMessageReceived;
            instance.Netcom.ChatReceived -= NetcomOnChatReceived;
        }

        private void NetcomOnChatReceived(object sender, ChatEventArgs e)
        {
            presenter.FlashChat(e.FromName);
        }

        private void NetcomOnInstantMessageReceived(object sender, InstantMessageEventArgs e)
        {
            ProcessIM_InBackground(e);
        }

        private void ProcessIM_InBackground(InstantMessageEventArgs e)
        {
            // Message from someone we muted?
            if (null != client.Self.MuteList.Find(me => me.Type == MuteType.Resident && me.ID == e.IM.FromAgentID)) return;
                
            switch (e.IM.Dialog)
            {
                case InstantMessageDialog.SessionSend:
                    if (instance.Groups.ContainsKey(e.IM.IMSessionID))
                    {
                        OpenMetaverse.Logger.Log("Not support GroupIM yet",Helpers.LogLevel.Error);
                        //HandleGroupIM(e);
                    }
                    else
                    {
                        OpenMetaverse.Logger.Log("Not support ConferenceIM yet",Helpers.LogLevel.Error);
                        // HandleConferenceIM(e);
                    }
                    break;

                case InstantMessageDialog.MessageFromAgent:
                    if (e.IM.FromAgentName == "Second Life")
                    {
                        HandleIMFromObject(e);
                    }
                    else if (e.IM.FromAgentID == UUID.Zero)
                    {
                        ui.ModalsManager.showModal_NotificationGeneric("Notification", e.IM.Message);
                    }
                    else if (e.IM.GroupIM || instance.Groups.ContainsKey(e.IM.IMSessionID))
                    {
                        //HandleGroupIM(e);
                    }
                    else if (e.IM.BinaryBucket.Length > 1)
                    { // conference
                        //HandleConferenceIM(e);
                    }
                    else if (e.IM.IMSessionID == UUID.Zero)
                    {
                        String msg = string.Format("Message from {0}: {1}", instance.Names.Get(e.IM.FromAgentID, e.IM.FromAgentName), e.IM.Message);
                        //Notification.AddNotification(new ntfGeneric(instance, msg));
                        DisplayNotificationInChat(msg);
                    }
                    else
                    {
                        HandleIM(e);
                    }
                    break;

                case InstantMessageDialog.MessageFromObject:
                    HandleIMFromObject(e);
                    break;

                case InstantMessageDialog.StartTyping:
                    if (presenter.chatListView.ContainsKey(e.IM.FromAgentName))
                    {
                        IHighlightableTabUI tab = presenter.chatListView.ChatTabs[e.IM.FromAgentName.ToLower()];
                        if (!tab.Highlighted) tab.PartialHighlight();
                    }

                    break;

                case InstantMessageDialog.StopTyping:
                    if (presenter.chatListView.ContainsKey(e.IM.FromAgentName))
                    {
                        IHighlightableTabUI tab = presenter.chatListView.ChatTabs[e.IM.FromAgentName.ToLower()];
                        if (!tab.Highlighted) tab.Unhighlight();
                    }

                    break;

                case InstantMessageDialog.MessageBox:
                    // UIService.Notification.AddNotification(new ntfGeneric(instance, e.IM.Message));
                    break;

                case InstantMessageDialog.RequestTeleport:
                    // instance.MainForm.AddNotification(new ntfTeleport(instance, e.IM));
                    break;

                case InstantMessageDialog.RequestLure:
                    // instance.MainForm.AddNotification(new ntfRequestLure(instance, e.IM));
                    break;

                case InstantMessageDialog.GroupInvitation:
                    // instance.MainForm.AddNotification(new ntfGroupInvitation(instance, e.IM));
                    break;

                case InstantMessageDialog.FriendshipOffered:
                    if (e.IM.FromAgentName == "Second Life")
                    {
                        HandleIMFromObject(e);
                    }
                    else
                    {
                        
                        Debug.LogError("not supported yest: frienship");
                        // instance.MainForm.AddNotification(new ntfFriendshipOffer(instance, e.IM));
                    }
                    break;

                case InstantMessageDialog.InventoryAccepted:
                    DisplayNotificationInChat(e.IM.FromAgentName + " accepted your inventory offer.");
                    break;

                case InstantMessageDialog.InventoryDeclined:
                    DisplayNotificationInChat(e.IM.FromAgentName + " declined your inventory offer.");
                    break;

                case InstantMessageDialog.GroupNotice:
                    // Is this group muted?
                    if (null != client.Self.MuteList.Find(me => me.Type == MuteType.Group && me.ID == e.IM.FromAgentID)) break;

                    Debug.LogError("not supported yest: group notice");
                    // ChatPresenter.AddNotification(new ntfGroupNotice(instance, e.IM));
                    break;

                case InstantMessageDialog.InventoryOffered:
                    // var ion = new ntfInventoryOffer(instance, e.IM);
                    //instance.MainForm.AddNotification(ion);
                    // if (instance.GlobalSettings["inv_auto_accept_mode"].AsInteger() == 1)
                    // {
                    //     ion.btnAccept.PerformClick();
                    // }
                    // else if (instance.GlobalSettings["inv_auto_accept_mode"].AsInteger() == 2)
                    // {
                    //     ion.btnDiscard.PerformClick();
                    // }
                    break;

                case InstantMessageDialog.TaskInventoryOffered:
                    // Is the object muted by name?
                    if (null != client.Self.MuteList.Find(me => me.Type == MuteType.ByName && me.Name == e.IM.FromAgentName)) break;

                    // var iont = new ntfInventoryOffer(instance, e.IM);
                    // instance.MainForm.AddNotification(iont);
                    // if (instance.GlobalSettings["inv_auto_accept_mode"].AsInteger() == 1)
                    // {
                    //     iont.btnAccept.PerformClick();
                    // }
                    // else if (instance.GlobalSettings["inv_auto_accept_mode"].AsInteger() == 2)
                    // {
                    //     iont.btnDiscard.PerformClick();
                    // }
                    break;
            }
            
        }


        //print a message to chat.
        private void HandleIMFromObject(InstantMessageEventArgs e)
        {
            // Is the object or the owner muted?
            if (null != client.Self.MuteList.Find(m => (m.Type == MuteType.Object && m.ID == e.IM.IMSessionID) // muted object by id 
                                                       || (m.Type == MuteType.ByName && m.Name == e.IM.FromAgentName) // object muted by name
                                                       || (m.Type == MuteType.Resident && m.ID == e.IM.FromAgentID) // object's owner muted
                )) return;

            DisplayNotificationInChat(e.IM.FromAgentName + ": " + e.IM.Message);
        }
        
        
        /// <param name="msg">Message to be printed in the chat tab</param>
        public void DisplayNotificationInChat(string msg)
        {
            DisplayNotificationInChat(msg, ChatBufferTextStyle.ObjectChat);
        }

        /// <param name="msg">Message to be printed in the chat tab</param>
        /// <param name="style">Style of the message to be printed, normal, object, etc.</param>
        public void DisplayNotificationInChat(string msg, ChatBufferTextStyle style)
        {
            DisplayNotificationInChat(msg, style, true);
        }

        /// <summary>
        /// Displays notification in the main chat tab
        /// </summary>
        /// <param name="msg">Message to be printed in the chat tab</param>
        /// <param name="style">Style of the message to be printed, normal, object, etc.</param>
        /// <param name="highlightChatTab">Highligt (and flash in taskbar) chat tab if not selected</param>
        public void DisplayNotificationInChat(string msg, ChatBufferTextStyle style, bool highlightChatTab)
        {
            if (! UnityMainThreadDispatcher.isOnMainThread())
            {
                UnityMainThreadDispatcher.Instance().Enqueue(
                        () => DisplayNotificationInChat(msg, style, highlightChatTab)
                );
                return;
            }

            if (style != ChatBufferTextStyle.Invisible)
            {
                ChatBufferItem line = new ChatBufferItem(
                    DateTime.Now,
                    string.Empty,
                    UUID.Zero,
                    msg,
                    style
                );

                try
                {
                    MainChatManger.ProcessBufferItem(line, true);
                    if (highlightChatTab)
                    {
                        presenter.chatListView.ChatTabs["chat"].Highlight();
                    }
                }
                catch (Exception) { }
            }

            // This is for alerting others on the fact that we raised a chat notification.
            
            // if (OnChatNotification != null)
            // {
            //     try { OnChatNotification(this, new ChatNotificationEventArgs(msg, style)); }
            //     catch { }
            // }
        }
        
        public bool TabExists(string name)
        {
            return presenter.chatListView.ChatTabs.ContainsKey(name.ToLower());
        }

        
        private void HandleIM(InstantMessageEventArgs e)
        {
            bool isNew = ShowIMTab(e.IM.FromAgentID, e.IM.FromAgentName, false);
            if (!TabExists(e.IM.IMSessionID.ToString())) return; // this should now exist. sanity check anyway
            var tab = presenter.chatListView.ChatTabs[e.IM.IMSessionID.ToString()];
            tab.Highlight();
            
            presenter.FlashChat(e.IM.FromAgentName);

            if (isNew)
            {
                presenter.ChatBoxView.ProcessIM(e, true);
                // tab.GetTextManager().ProcessIM(e, true);
            }
        }
 
        /// <summary>
        /// Creates new IM tab if needed
        /// </summary>
        /// <param name="agentID">IM session with agentID</param>
        /// <param name="label">Tab label</param>
        /// <param name="makeActive">Should tab be selected and focused</param>
        /// <returns>True if there was an existing IM tab, false if it was created</returns>
        public bool ShowIMTab(UUID agentID, string label, bool makeActive)
        {
            if (chatListView.ChatTabs.ContainsKey((agentID).ToString()))
            {
                return true;
            }

            if (makeActive)
            {
                instance.MediaManager.PlayUISound(UISounds.IMWindow);
            }
            else
            {
                instance.MediaManager.PlayUISound(UISounds.IM);
            }
            
            chatListView.AddIMTab(agentID, client.Self.AgentID ^ agentID, label);

            return true;
        }

    }
}