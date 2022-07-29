using System;
using OpenMetaverse;
using Plugins.CommonDependencies;
using Raindrop.Netcom;
using Raindrop.UI.chat;
using Raindrop.UI.chat.printer_component;
using UnityEngine;

namespace Raindrop.Presenters
{
    public class IMPresenter : MonoBehaviour
    {
        private RaindropInstance instance => RaindropInstance.GlobalInstance;
        private RaindropNetcom netcom => instance.Netcom;
        public TMPTextFieldPrinter printer; //the component in the textbox
        public IMManager manager;
        public ChatInputPresenter input;
        public UUID SessionID;

        
        public void Init(UUID sessionID)
        {
            manager = new IMManager(
                this,
                instance,
                printer,
                IMTextManagerType.Agent,
                sessionID, 
                "todo...");
            netcom.InstantMessageSent += NetcomOnInstantMessageSent;
        }

        private void NetcomOnInstantMessageSent(object sender, InstantMessageSentEventArgs e)
        {
            if (e.SessionID != SessionID) return;
        }


        #region textual inputs
        // allow UI input field to send outgoing chat to the simulator.
        public void ProcessChatInput(string inputString, ChatType normal)
        {
            manager.ProcessChatInput(inputString, normal);
        }

        // clear user input field.
        public void ClearTextInput()
        {
            input.ClearInputField();
        }
        #endregion

        public void ProcessIM(InstantMessageEventArgs instantMessageEventArgs)
        {
            throw new NotImplementedException();
        }
    }
}