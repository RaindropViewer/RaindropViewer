using System;
using OpenMetaverse;
using Raindrop.UI.chat;
using UnityEngine;

namespace Raindrop.Presenters
{
    public class IMPresenter : MonoBehaviour
    {
        private RaindropInstance instance => ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>();
        public TMPTextFieldPrinter printer; //the component in the textbox
        public IMManager manager;
        public ChatInputPresenter input;

        private void Start()
        {
            manager = new IMManager(
                this,
                instance,
                printer,
                IMTextManagerType.Agent,
                UUID.Zero, 
                "loading...");
        }

        //use this after instantiation of prefab is done.
        public void SetTargetAndSessionId()
        {
            throw new NotImplementedException();

        }

        #region textual inputs
        // allow UI input field to send outgoing chat to the simulator.
        public void ProcessChatInput(string inputString, ChatType normal)
        {
            manager.ProcessChatInput(inputString, normal);
        }

        // clear user input field.
        public void ClearIMInput()
        {
            input.ClearInputField();
        }
        #endregion
    }
}