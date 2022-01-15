﻿using System;
using OpenMetaverse;
using Raindrop.Netcom;
using Raindrop.ServiceLocator;
using UnityEngine;

namespace Raindrop.Services
{
    public class UIService : IGameService
    {
        //UI is a service. it will always be available.
        // note that presenters should register with the canvas manager. presenters themselves provide the logic of ui-traversal.
        // modals on the other hand are provided and popped in by the presenters themselves. for example a confirmation prompt for the user - obviously that should fall under the responsibility of the UI-logic layer.
        // UIservice contains:
        //   CanvasManager - manages the UI stack. Access this to pop and push views onto the ui stack.
        //   ModalManager - manages the modals. access this to pop and show modals. 
        //  <deprecated> LoadingCanvasPresenter - this particular modal/screen is tricky; it appears only when the scene is loading.

        private RaindropInstance instance;
        private RaindropNetcom netcom { get { return instance.Netcom; } }
        private GridClient client { get { return instance.Client; } }

        // canvases are stack-based. only 1 is top-most and active at any time.
        private CanvasManager _canvasManager;
        public CanvasManager canvasManager { set { _canvasManager = value; } get { return _canvasManager; } }
        // modals are single-display. however, there is a modal queue, such that when the current modal is dismissed, the next-in-queue will appear.
        //care has to be taken not to spam the user with modals.
        private ModalManager _modalManager;
        public ModalManager modalManager { set { _modalManager = value; } get { return _modalManager; } }

        public UIService(CanvasManager cm, ModalManager mm)
        {
            canvasManager = cm;
            modalManager = mm;

            // UI depends on raindrop business layer.
            try
            {
                this.instance = ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>();
            } catch (InvalidOperationException)
            {
                Debug.LogError("UIService failed to get raindrop service");
                //failed to find service.
                return;    
            }



            // Callbacks
            netcom.ClientLoginStatus += new EventHandler<LoginProgressEventArgs>(netcom_ClientLoginStatus);
            netcom.ClientLoggedOut += new EventHandler(netcom_ClientLoggedOut);
            netcom.ClientDisconnected += new EventHandler<DisconnectedEventArgs>(netcom_ClientDisconnected);
            instance.Names.NameUpdated += new EventHandler<UUIDNameReplyEventArgs>(Names_NameUpdated);

            RegisterClientEvents(client);

            initialise();

        }

        ~UIService()
        {

            netcom.ClientLoginStatus -= new EventHandler<LoginProgressEventArgs>(netcom_ClientLoginStatus);
            netcom.ClientLoggedOut -= new EventHandler(netcom_ClientLoggedOut);
            netcom.ClientDisconnected -= new EventHandler<DisconnectedEventArgs>(netcom_ClientDisconnected);
            instance.Names.NameUpdated -= new EventHandler<UUIDNameReplyEventArgs>(Names_NameUpdated);

            UnregisterClientEvents(client);
        }

        public void initialise()
        {
            startUIInitialView();
            ready = true;
        }

        protected void startUIInitialView()
        {
            canvasManager.resetToInitialScreen();
            modalManager.showModalNotification("Disclaimer", "This software is a work in progress. There is no guarantee about its stability. ");
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
                modalManager.setVisibleGenericModal("Login failed. Server reply: ", e.Message, true);
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
                modalManager.setVisibleGenericModal("Login success! Server reply: ", e.Message, true);
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
            modalManager.showModalNotification("Logged out", "you have/were logged out");

            canvasManager.resetToInitialScreen();

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
            modalManager.showModalNotification("Client disconnected", e.Message+ " "+e.Reason.ToString());

            firstMoneyNotification = true;

            if (e.Reason == NetworkManager.DisconnectType.ClientInitiated) return;
            netcom_ClientLoggedOut(sender, EventArgs.Empty);

            canvasManager.resetToInitialScreen();

            //if (instance.GlobalSettings["auto_reconnect"].AsBoolean())
            //{
            //    BeginAutoReconnect();
            //}
        }

        bool firstMoneyNotification = true;
        private string tlblMoneyBalanceText;
        public bool ready = false;

        void Self_MoneyBalance(object sender, BalanceEventArgs e)
        {
            Debug.Log("you have moneybalance of " + e.Balance);

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

        public object GetService(Type serviceType)
        {


            throw new NotImplementedException();
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