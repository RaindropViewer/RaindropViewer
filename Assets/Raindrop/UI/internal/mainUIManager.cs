using System;

using System.Collections.Generic;
using System.Threading;

using Raindrop.Netcom;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Assets;
using Raindrop;
using UnityEngine;

namespace Raindrop
{
    public class mainUIManager
    {
        //mainUImanager has dependencies:
        //   CanvasManager - stores and manages the pops, push of views onto the ui stack.
        //   ModalManager - pops and shows modals.

        private RaindropInstance instance;
        private RaindropNetcom netcom { get { return instance.Netcom; } }
        private GridClient client { get { return instance.Client; } }

        public CanvasManager canvasManager { get { return CanvasManager.GetInstance(); } }
        public ModalManager modalManager { get { return ModalManager.GetInstance(); } }

        public mainUIManager(RaindropInstance raindropInstance)
        {
            this.instance = raindropInstance;

            // Callbacks
            netcom.ClientLoginStatus += new EventHandler<LoginProgressEventArgs>(netcom_ClientLoginStatus);
            netcom.ClientLoggedOut += new EventHandler(netcom_ClientLoggedOut);
            netcom.ClientDisconnected += new EventHandler<DisconnectedEventArgs>(netcom_ClientDisconnected);
            instance.Names.NameUpdated += new EventHandler<UUIDNameReplyEventArgs>(Names_NameUpdated);

            RegisterClientEvents(client);

            initialiseUI();

        }

        private void initialiseUI()
        {
            canvasManager.pushCanvas(CanvasType.Login);
            modalManager.showSimpleModalBoxWithActionBtn("Disclaimer", "This software is a work in progress. There is no guarantee about its stability. ", "Accept");

        }

        public GameObject getCurrentForegroundPresenter()
        {
            return canvasManager.getForegroundCanvas();
        }

        private void RegisterClientEvents(GridClient client)
        {
            client.Parcels.ParcelProperties += new EventHandler<ParcelPropertiesEventArgs>(Parcels_ParcelProperties);
            client.Self.MoneyBalanceReply += new EventHandler<MoneyBalanceReplyEventArgs>(Self_MoneyBalanceReply);
            client.Self.MoneyBalance += new EventHandler<BalanceEventArgs>(Self_MoneyBalance);
        }

        private void UnregisterClientEvents(GridClient client)
        {
            client.Parcels.ParcelProperties -= new EventHandler<ParcelPropertiesEventArgs>(Parcels_ParcelProperties);
            client.Self.MoneyBalanceReply -= new EventHandler<MoneyBalanceReplyEventArgs>(Self_MoneyBalanceReply);
            client.Self.MoneyBalance -= new EventHandler<BalanceEventArgs>(Self_MoneyBalance);
        }




        #region client event implementation


        private void netcom_ClientLoginStatus(object sender, LoginProgressEventArgs e)
        {
            if (e.Status == LoginStatus.Failed)
            {
                modalManager.showModal("Login failed.", e.Message);
                //if (InAutoReconnect)
                //{
                //    if (instance.GlobalSettings["auto_reconnect"].AsBoolean() && e.FailReason != "tos")
                //        BeginAutoReconnect();
                //    else
                //        InAutoReconnect = false;
                //}
            }
            else if (e.Status == LoginStatus.Success)
            {
                modalManager.showModal("Login success!", e.Message);
                //InAutoReconnect = false;
                //reconnectToolStripMenuItem.Enabled = false;
                //loginToolStripMenuItem.Enabled = false;
                //tsb3D.Enabled = tbtnVoice.Enabled = disconnectToolStripMenuItem.Enabled =
                //tbtnGroups.Enabled = tbnObjects.Enabled = tbtnWorld.Enabled = tbnTools.Enabled = tmnuImport.Enabled =
                //    tbtnFriends.Enabled = tbtnInventory.Enabled = tbtnSearch.Enabled = tbtnMap.Enabled = true;

                //statusTimer.Start();
                //RefreshWindowTitle();


            }
        }

        private void netcom_ClientLoggedOut(object sender, EventArgs e)
        {
            modalManager.showSimpleModalBoxWithActionBtn("Logged out", "you have/were logged out", "ok");

            //tsb3D.Enabled = tbtnVoice.Enabled = disconnectToolStripMenuItem.Enabled =
            //tbtnGroups.Enabled = tbnObjects.Enabled = tbtnWorld.Enabled = tbnTools.Enabled = tmnuImport.Enabled =
            //    tbtnFriends.Enabled = tbtnInventory.Enabled = tbtnSearch.Enabled = tbtnMap.Enabled = false;

            //reconnectToolStripMenuItem.Enabled = true;
            //loginToolStripMenuItem.Enabled = true;
            //InAutoReconnect = false;

            //if (statusTimer != null)
            //    statusTimer.Stop();

            //RefreshStatusBar();
            //RefreshWindowTitle();
        }

        private void netcom_ClientDisconnected(object sender, DisconnectedEventArgs e)
        {
            modalManager.showSimpleModalBoxWithActionBtn("Client disconnected", e.Message+ " "+e.Reason.ToString() , "ok");

            firstMoneyNotification = true;

            if (e.Reason == NetworkManager.DisconnectType.ClientInitiated) return;
            netcom_ClientLoggedOut(sender, EventArgs.Empty);

            canvasManager.reinitToLoginScreen();

            //if (instance.GlobalSettings["auto_reconnect"].AsBoolean())
            //{
            //    BeginAutoReconnect();
            //}
        }

        bool firstMoneyNotification = true;
        private string tlblMoneyBalanceText;

        void Self_MoneyBalance(object sender, BalanceEventArgs e)
        {
            int oldBalance = 0;
            int.TryParse(tlblMoneyBalanceText, out oldBalance);
            int delta = Math.Abs(oldBalance - e.Balance);

            if (firstMoneyNotification)
            {
                firstMoneyNotification = false;
            }
            else
            {
                if (delta > 50)
                {
                    //if (oldBalance > e.Balance)
                    //{
                    //    instance.MediaManager.PlayUISound(UISounds.MoneyIn);
                    //}
                    //else
                    //{
                    //    instance.MediaManager.PlayUISound(UISounds.MoneyOut);
                    //}
                }
            }
        }


        void Names_NameUpdated(object sender, UUIDNameReplyEventArgs e)
        {
            if (!e.Names.ContainsKey(client.Self.AgentID)) return;
            

            //if (InvokeRequired)
            //{
            //    if (IsHandleCreated || !instance.MonoRuntime)
            //    {
            //        BeginInvoke(new MethodInvoker(() => Names_NameUpdated(sender, e)));
            //    }
            //    return;
            //}

            //RefreshWindowTitle();
            //RefreshStatusBar();
        }

        void Self_MoneyBalanceReply(object sender, MoneyBalanceReplyEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Description))
            {
                //if (instance.GlobalSettings["transaction_notification_dialog"].AsBoolean())
                //    AddNotification(new ntfGeneric(instance, e.Description));
                //if (instance.GlobalSettings["transaction_notification_chat"].AsBoolean())
                //    TabConsole.DisplayNotificationInChat(e.Description);
            }
        }


        #endregion


        #region Update status

        void Parcels_ParcelProperties(object sender, ParcelPropertiesEventArgs e)
        {
            //if (PreventParcelUpdate || e.Result != ParcelResult.Single) return;
            //if (InvokeRequired)
            //{
            //    BeginInvoke(new MethodInvoker(() => Parcels_ParcelProperties(sender, e)));
            //    return;
            //}

            Parcel parcel = instance.State.Parcel = e.Parcel;

            //tlblParcel.Text = parcel.Name;
            //tlblParcel.ToolTipText = parcel.Desc;

            //if ((parcel.Flags & ParcelFlags.AllowFly) != ParcelFlags.AllowFly)
            //    icoNoFly.Visible = true;
            //else
            //    icoNoFly.Visible = false;

            //if ((parcel.Flags & ParcelFlags.CreateObjects) != ParcelFlags.CreateObjects)
            //    icoNoBuild.Visible = true;
            //else
            //    icoNoBuild.Visible = false;

            //if ((parcel.Flags & ParcelFlags.AllowOtherScripts) != ParcelFlags.AllowOtherScripts)
            //    icoNoScript.Visible = true;
            //else
            //    icoNoScript.Visible = false;

            //if ((parcel.Flags & ParcelFlags.RestrictPushObject) == ParcelFlags.RestrictPushObject)
            //    icoNoPush.Visible = true;
            //else
            //    icoNoPush.Visible = false;

            //if ((parcel.Flags & ParcelFlags.AllowDamage) == ParcelFlags.AllowDamage)
            //    icoHealth.Visible = true;
            //else
            //    icoHealth.Visible = false;

            //if ((parcel.Flags & ParcelFlags.AllowVoiceChat) != ParcelFlags.AllowVoiceChat)
            //    icoNoVoice.Visible = true;
            //else
            //    icoNoVoice.Visible = false;
        }

        //private void RefreshStatusBar()
        //{
        //    if (netcom.IsLoggedIn)
        //    {
        //        tlblLoginName.Text = instance.Names.Get(client.Self.AgentID, client.Self.Name);
        //        tlblMoneyBalance.Text = client.Self.Balance.ToString();
        //        icoHealth.Text = client.Self.Health.ToString() + "%";

        //        var cs = client.Network.CurrentSim;
        //        tlblRegionInfo.Text =
        //            (cs == null ? "No region" : cs.Name) +
        //            " (" + Math.Floor(client.Self.SimPosition.X).ToString() + ", " +
        //            Math.Floor(client.Self.SimPosition.Y).ToString() + ", " +
        //            Math.Floor(client.Self.SimPosition.Z).ToString() + ")";
        //    }
        //    else
        //    {
        //        tlblLoginName.Text = "Offline";
        //        tlblMoneyBalance.Text = "0";
        //        icoHealth.Text = "0%";
        //        tlblRegionInfo.Text = "No Region";
        //        tlblParcel.Text = "No Parcel";

        //        icoHealth.Visible = false;
        //        icoNoBuild.Visible = false;
        //        icoNoFly.Visible = false;
        //        icoNoPush.Visible = false;
        //        icoNoScript.Visible = false;
        //        icoNoVoice.Visible = false;
        //    }
        //}

        //private void RefreshWindowTitle()
        //{
        //    string name = instance.Names.Get(client.Self.AgentID, client.Self.Name);
        //    StringBuilder sb = new StringBuilder();
        //    sb.Append("Radegast - ");

        //    if (netcom.IsLoggedIn)
        //    {
        //        sb.Append("[" + name + "]");

        //        if (instance.State.IsAway)
        //        {
        //            sb.Append(" - Away");
        //            if (instance.State.IsBusy) sb.Append(", Busy");
        //        }
        //        else if (instance.State.IsBusy)
        //        {
        //            sb.Append(" - Busy");
        //        }

        //        if (instance.State.IsFollowing)
        //        {
        //            sb.Append(" - Following ");
        //            sb.Append(instance.State.FollowName);
        //        }
        //    }
        //    else
        //    {
        //        sb.Append("Logged Out");
        //    }

        //    this.Text = sb.ToString();

        //    // When minimized to tray, update tray tool tip also
        //    if (WindowState == FormWindowState.Minimized && instance.GlobalSettings["minimize_to_tray"])
        //    {
        //        trayIcon.Text = sb.ToString();
        //        ctxTrayMenuLabel.Text = sb.ToString();
        //    }

        //    sb = null;
        //}

        #endregion


    }
}