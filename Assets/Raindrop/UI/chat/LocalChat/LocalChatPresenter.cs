using System;
using OpenMetaverse;
using UnityEngine;

namespace Raindrop.UI.chat
{
    // for local chat, manage the printer and the input box.
    public class LocalChatPresenter : MonoBehaviour
    {
        private RaindropInstance instance => ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>();
        
        public TMPTextFieldPrinter printer; //the component in the textbox
        public LocalChatManager LocalChatManager;
        public ChatInputPresenter input;

        private void Start()
        {
            LocalChatManager = new LocalChatManager(instance, printer); //kind of like inject logic into the textbox.
            
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