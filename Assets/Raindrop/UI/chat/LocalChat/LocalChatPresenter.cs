using System;
using OpenMetaverse;
using Raindrop.Netcom;
using UnityEngine;

namespace Raindrop.UI.chat
{
    // for local chat, manage the printer and the input box.
    public class LocalChatPresenter : MonoBehaviour
    {
        private RaindropInstance instance => ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>();
        private RaindropNetcom netcom => instance.Netcom;
        
        public TMPTextFieldPrinter printer; //the component in the textbox
        public LocalChatManager LocalChatManager;
        public ChatInputPresenter input;

        private void Start()
        {
            LocalChatManager = new LocalChatManager(instance, printer); //kind of like inject logic into the textbox.
            netcom.ChatSent += Netcom_ChatSent;
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
            input.ClearInputField();
        }
    }
}