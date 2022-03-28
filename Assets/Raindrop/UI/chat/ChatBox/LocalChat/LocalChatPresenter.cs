using System;
using OpenMetaverse;
using Raindrop.Netcom;
using Raindrop.UI.chat.printer_component;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace Raindrop.UI.chat
{
    // for local chat, manage the printer and the input box.
    public class LocalChatPresenter : MonoBehaviour
    {
        private RaindropInstance instance => ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>();
        private RaindropNetcom netcom => instance.Netcom;
        
        public TMPTextFieldPrinter printer; //the component in the textbox
        public LocalChatManager LocalChatManager;
        public ChatInputPresenter input; //nullable.

        private void Init()
        {
            LocalChatManager = new LocalChatManager(instance);
            netcom.ChatSent += Netcom_ChatSent; //local chat sent successfully.
        }

        private void Netcom_ChatSent(object sender, ChatSentEventArgs e)
        {
            ClearTextInput();
        }

        // allow UI input field to send outgoing chat to the simulator.
        public void ProcessChatInput(string inputString, ChatType normal)
        {
            LocalChatManager.ProcessChatInput(inputString, normal);
        }
        
        
        // clear user input field.
        public void ClearTextInput()
        {
            if (input)
            {
                input.ClearInputField();
            }
        }
    }

}
