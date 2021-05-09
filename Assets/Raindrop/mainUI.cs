using System;

using System.Collections.Generic;
using System.Threading;

using Raindrop.Netcom;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Assets;


namespace Raindrop
{
    public class mainUI
    {
        private RaindropInstance raindropInstance;

        public mainUI(RaindropInstance raindropInstance)
        {
            this.raindropInstance = raindropInstance;


            // Callbacks
            netcom.ClientLoginStatus += new EventHandler<LoginProgressEventArgs>(netcom_ClientLoginStatus);
            netcom.ClientLoggedOut += new EventHandler(netcom_ClientLoggedOut);
            netcom.ClientDisconnected += new EventHandler<DisconnectedEventArgs>(netcom_ClientDisconnected);
            instance.Names.NameUpdated += new EventHandler<UUIDNameReplyEventArgs>(Names_NameUpdated);
        }

        public object instance { get; private set; }
    }
}