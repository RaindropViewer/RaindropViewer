
using System;
using OpenMetaverse;

namespace Raindrop.Netcom
{
    public partial class RaindropNetcom
    {
        // For the NetcomSync stuff
        private delegate void OnClientLoginRaise(LoginProgressEventArgs e);
        private delegate void OnClientLogoutRaise(EventArgs e);
        private delegate void OnClientDisconnectRaise(DisconnectedEventArgs e);
        private delegate void OnChatRaise(ChatEventArgs e);
        private delegate void OnInstantMessageRaise(InstantMessageEventArgs e);
        private delegate void OnAlertMessageRaise(AlertMessageEventArgs e);
        private delegate void OnMoneyBalanceRaise(BalanceEventArgs e);
        private delegate void OnTeleportStatusRaise(TeleportEventArgs e);

        public event EventHandler<OverrideEventArgs> ClientLoggingIn;
        public event EventHandler<LoginProgressEventArgs> ClientLoginStatus;
        public event EventHandler<OverrideEventArgs> ClientLoggingOut;
        public event EventHandler ClientLoggedOut;
        public event EventHandler<DisconnectedEventArgs> ClientDisconnected;
        public event EventHandler<ChatEventArgs> ChatReceived;
        public event EventHandler<ChatSentEventArgs> ChatSent;
        public event EventHandler<InstantMessageEventArgs> InstantMessageReceived;
        public event EventHandler<InstantMessageSentEventArgs> InstantMessageSent;
        public event EventHandler<TeleportingEventArgs> Teleporting;
        public event EventHandler<TeleportEventArgs> TeleportStatusChanged;
        public event EventHandler<AlertMessageEventArgs> AlertMessageReceived;
        public event EventHandler<BalanceEventArgs> MoneyBalanceUpdated;

        protected virtual void OnClientLoggingIn(OverrideEventArgs e)
        {
            if (ClientLoggingIn != null) ClientLoggingIn(this, e);
        }

        protected virtual void OnClientLoginStatus(LoginProgressEventArgs e)
        {
            if (ClientLoginStatus != null) ClientLoginStatus(this, e);
        }

        protected virtual void OnClientLoggingOut(OverrideEventArgs e)
        {
            if (ClientLoggingOut != null) ClientLoggingOut(this, e);
        }

        protected virtual void OnClientLoggedOut(EventArgs e)
        {
            if (ClientLoggedOut != null) ClientLoggedOut(this, e);
        }

        protected virtual void OnClientDisconnected(DisconnectedEventArgs e)
        {
            if (ClientDisconnected != null) ClientDisconnected(this, e);
        }

        protected virtual void OnChatReceived(ChatEventArgs e)
        {
            if (ChatReceived != null) ChatReceived(this, e);
        }

        protected virtual void OnChatSent(ChatSentEventArgs e)
        {
            if (ChatSent != null) ChatSent(this, e);
        }

        protected virtual void OnInstantMessageReceived(InstantMessageEventArgs e)
        {
            if (InstantMessageReceived != null) InstantMessageReceived(this, e);
        }

        protected virtual void OnInstantMessageSent(InstantMessageSentEventArgs e)
        {
            if (InstantMessageSent != null) InstantMessageSent(this, e);
        }

        protected virtual void OnTeleporting(TeleportingEventArgs e)
        {
            if (Teleporting != null) Teleporting(this, e);
        }

        protected virtual void OnTeleportStatusChanged(TeleportEventArgs e)
        {
            if (TeleportStatusChanged != null) TeleportStatusChanged(this, e);
        }

        protected virtual void OnAlertMessageReceived(AlertMessageEventArgs e)
        {
            if (AlertMessageReceived != null) AlertMessageReceived(this, e);
        }

        protected virtual void OnMoneyBalanceUpdated(BalanceEventArgs e)
        {
            if (MoneyBalanceUpdated != null) MoneyBalanceUpdated(this, e);
        }
    }
}
