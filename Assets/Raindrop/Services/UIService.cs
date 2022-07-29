using System;
using OpenMetaverse;
using Plugins.CommonDependencies;
using Raindrop.Netcom;
using Raindrop.Presenters;
using Raindrop.UI.LoadingScreen;
using Raindrop.UI.Views;
using UnityEngine;

namespace Raindrop.Services
{
    public class UIService : IGameService, IDisposable
    {
        //UI is a service. it will always be available.
        // presenters themselves provide the logic of ui-traversal.
        // modals on the other hand are provided and popped in by the presenters themselves. for example a confirmation prompt for the user - obviously that should fall under the responsibility of the UI-logic layer.
        //   UIservice contains:
        //   CanvasManager - manages the UI stack. Access this to pop and push views onto the ui stack.
        //   ModalManager - manages the modals. access this to pop and show modals. 
        //   Notification - manages app-wide notifications. 
        //   <deprecated> LoadingCanvasPresenter - this particular modal/screen is tricky; it appears only when the scene is loading.

        private RaindropInstance instance;
        private RaindropNetcom netcom { get { return instance.Netcom; } }
        private GridClient client { get { return instance.Client; } }

        // canvases are stack-based. only 1 is top-most and active at any time.
        private ScreenStackManager ScreenStackManager { set; get; }

        // modals are single-display. however, there is a modal queue, such that when the current modal is dismissed, the next-in-queue will appear.
        //care has to be taken not to spam the user with modals.
        public ModalsManager ModalsManager { set; get; }
        
        // fade into loading screen.
        public LoadingController _loadingController;
        
        //chat screen.
        public ChatPresenter chatFacade;

        public MapUIView MapFacade;

        // refactor:
        /* initial:  UIService(ScreensManager cm, ModalManager mm)
         * desired:  UIService(raindropinstance)
         *          +UIService.showScreen(UIBuilder(CanvasType.Login))
         *          +UIService.showModal
         */
        public UIService(RaindropInstance raindropInstance,
            ScreenStackManager cm, ModalsManager mm, LoadingView loadingView,
            ChatPresenter ChatPresenter)
        {
            instance = raindropInstance;
            ScreenStackManager = cm;
            ModalsManager = mm;
            _loadingController = new LoadingController(loadingView);
            chatFacade = ChatPresenter;
            
            // Callbacks
            netcom.ClientLoginStatus += new EventHandler<LoginProgressEventArgs>(netcom_ClientLoginStatus);
            netcom.ClientLoggedOut += new EventHandler(netcom_ClientLoggedOut);
            netcom.ClientDisconnected += new EventHandler<DisconnectedEventArgs>(netcom_ClientDisconnected);
            instance.Names.NameUpdated += new EventHandler<UUIDNameReplyEventArgs>(Names_NameUpdated);

            RegisterClientEvents(client);

            initialise();
        }

        public void Dispose()
        {
            _loadingController?.Dispose();

            if (netcom != null)
            {
                netcom.ClientLoginStatus -= new EventHandler<LoginProgressEventArgs>(netcom_ClientLoginStatus);
                netcom.ClientLoggedOut -= new EventHandler(netcom_ClientLoggedOut);
                netcom.ClientDisconnected -= new EventHandler<DisconnectedEventArgs>(netcom_ClientDisconnected);
                instance.Names.NameUpdated -= new EventHandler<UUIDNameReplyEventArgs>(Names_NameUpdated);
            }

            UnregisterClientEvents(client);
        }

        public void initialise()
        {
            startUIInitialView();
            ready = true;
        }

        protected void startUIInitialView()
        {
            ScreenStackManager.ResetToInitialScreen();
            ModalsManager.showModal_NotificationGeneric("Disclaimer", "This software is a work in progress. There is no guarantee about its stability. ");
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
                //modalManager.showModalNotification("Login failed. Server reply: ", e.Message);
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
                //modalManager.showModalNotification("Login success! Server reply: ", e.Message);
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
            //modalManager.showModalNotification("Logged out", "you have/were logged out");

            ScreenStackManager.ResetToInitialScreen();
        }

        private void netcom_ClientDisconnected(object sender, DisconnectedEventArgs e)
        {
            //modalManager.showModalNotification("Client disconnected", e.Message+ " "+e.Reason.ToString());

            firstMoneyNotification = true;

            if (e.Reason == NetworkManager.DisconnectType.ClientInitiated) return;
            netcom_ClientLoggedOut(sender, EventArgs.Empty);

            ScreenStackManager.ResetToInitialScreen();

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
                    
                }
            }
        }


        void Names_NameUpdated(object sender, UUIDNameReplyEventArgs e)
        {
            if (!e.Names.ContainsKey(client.Self.AgentID)) return;
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
            Parcel parcel = instance.State.Parcel = e.Parcel;
        }
        #endregion

        public class Notification
        {
            public static void AddNotification()
            {
                throw new NotImplementedException();
            }
        }

        #region screen poppin
        public void ResetToInitialScreen()
        {
            ScreenStackManager.ResetToInitialScreen();
        }

        public void PopCanvas()
        {
            ScreenStackManager.PopCanvas();
        }

        public void PopAndPush(CanvasType canvasTypeToPush)
        {
            ScreenStackManager.PopAndPush(canvasTypeToPush);
        }

        public void Push(CanvasType canvasTypeToPush)
        {
            ScreenStackManager.Push(canvasTypeToPush);
        }

        public CanvasType GetPresentCanvasType()
        {
            return ScreenStackManager.TopCanvas.canvasType;
        }
        #endregion

    }
}