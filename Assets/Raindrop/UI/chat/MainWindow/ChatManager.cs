using System;
using System.Collections;
using OpenMetaverse;
using System.Collections.Generic;
using Raindrop.Netcom;
using Raindrop.Presenters;
using Raindrop.Services;
using Raindrop.Services.Bootstrap;
using UnityEngine;
using Logger = OpenMetaverse.Logger;

namespace Raindrop
{
    //holds the chats in memory?
    // coordinates the left side tab list, and the right side current text box visible.
    public class ChatManager
    {
        private RaindropNetcom netcom { get { return instance.Netcom; } }
        private GridClient client => instance.Client;

        public Dictionary<String, ChatTabPresenter> Tabs = new Dictionary<string, ChatTabPresenter>();
        // public ChatTabs ChatTabs { get; set; }

        public LocalChatManager MainChatManger = null;

        RaindropInstance instance;
        public ChatManager(RaindropInstance instance)
        {
            if (instance == null)
                OpenMetaverse.Logger.Log("instance is not avaailbe in chatmanager", Helpers.LogLevel.Error);
            this.instance = instance;
            
            //subscribe to incoming IMs, where we open the IM in the view.
            instance.Netcom.InstantMessageReceived += NetcomOnInstantMessageReceived;
        }

        private void NetcomOnInstantMessageReceived(object sender, InstantMessageEventArgs e)
        {
            ProcessIM_InBackground(e);
        }

        private void ProcessIM_InBackground(InstantMessageEventArgs e)
        {
            // Message from someone we muted?
            if (null != client.Self.MuteList.Find(me => me.Type == MuteType.Resident && me.ID == e.IM.FromAgentID)) return;
                
            //LSLhelper automation was removed.
                
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
                        // ChatPresenter.AddNotification(new ntfGeneric(instance, e.IM.Message));
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
                        //HandleIM(e);
                    }
                    break;

                case InstantMessageDialog.MessageFromObject:
                    HandleIMFromObject(e);
                    break;

                case InstantMessageDialog.StartTyping:
                    if (Tabs.ContainsKey(e.IM.FromAgentName))
                    {
                        ChatTabPresenter tab = Tabs[e.IM.FromAgentName.ToLower()];
                        if (!tab.Highlighted) tab.PartialHighlight();
                    }

                    break;

                case InstantMessageDialog.StopTyping:
                    if (Tabs.ContainsKey(e.IM.FromAgentName))
                    {
                        ChatTabPresenter tab = Tabs[e.IM.FromAgentName.ToLower()];
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
            if (! Globals.isOnMainThread())
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
                        Tabs["chat"].Highlight();
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

        public void Dispose()
        {
            
        }
    }

    public class ChatTabs : Dictionary<string, GameObject>
    {
        public ChatTabs()
        {
        }

        public bool TabExists(string imFromAgentName)
        {
            return true;

        }
    }
}