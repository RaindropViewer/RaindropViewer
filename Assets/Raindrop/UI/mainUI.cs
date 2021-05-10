using System;

using System.Collections.Generic;
using System.Threading;

using Raindrop.Netcom;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Assets;
using Raindrop;


namespace Raindrop
{
    public class mainUI
    {
        private RaindropNetcom netcom { get { return instance.Netcom; } }
        private RaindropInstance instance;

        private RaindropViewControl rdControl;

        public mainUI(RaindropInstance raindropInstance)
        {
            this.instance = raindropInstance;
            rdControl = new RaindropViewControl();

            // Callbacks
            //netcom.ClientLoginStatus += new EventHandler<LoginProgressEventArgs>(LoginVM.netcom_ClientLoginStatus);
            //netcom.ClientLoggedOut += new EventHandler(netcom_ClientLoggedOut);
            //netcom.ClientDisconnected += new EventHandler<DisconnectedEventArgs>(netcom_ClientDisconnected);
            //instance.Names.NameUpdated += new EventHandler<UUIDNameReplyEventArgs>(Names_NameUpdated);
        }
        
    }
}