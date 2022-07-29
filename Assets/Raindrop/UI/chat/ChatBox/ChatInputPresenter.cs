using System;
using OpenMetaverse;
using Plugins.CommonDependencies;
using Raindrop.Presenters;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

namespace Raindrop.UI.chat
{
    // this class sends whatever is in the inputField when the send button is pressed.
    public class ChatInputPresenter : MonoBehaviour, IInitialisable
    {
        // _____________________
        // | hello there.. |SEND|
        // _____________________

        public RaindropInstance instance => RaindropInstance.GlobalInstance;

        public IMPresenter DestChatInputAcceptor; // the monobehavior that accepts our input.

        [Tooltip("user input field")]
        public TMP_InputField ChatInputField;
        private string inputString;

        [Tooltip("button to send the text box to the chat")]
        public Button SendButton;

        private bool started;

        public void Initialise()
        {
            ChatInputField
                .onValueChanged
                .AsObservable()
                .Subscribe(_ => OnInputChanged(_)); 
            SendButton
                .onClick
                .AsObservable()
                .Subscribe(_ => OnSendBtnClick());

            StartIt();
        }

        private void StartIt()
        {
            started = true;
            
            if (instance != null)
            {
                UpdateClickablity();
            }
        }

        private void OnEnable()
        {
            if (!started)
                return;
            
            if (instance != null)
            {
                UpdateClickablity();
            }
        }

        private void UpdateClickablity()
        {
            if (instance.Client.Network.Connected)
            {
                SendButton.interactable = true;
            }
            else
            {
                SendButton.interactable = false;
            }
        }

        private void OnInputChanged(string _)
        {
            inputString = _;
            return;
        }

        private void OnSendBtnClick()
        {
            //public chat
            // ProcessChatInput(inputString, ChatType.Normal);
            if (DestChatInputAcceptor == null)
            {
                Debug.LogError("the dest field of the chatinputpresenter is not set.");
                return;
            }
            Debug.Log("Sending localchat to server");
            DestChatInputAcceptor.ProcessChatInput(inputString, ChatType.Normal);
        }


        public void ClearInputField()
        {
            this.ChatInputField.text = "";
            //this.inputString = "";
        }

    }

    // UI that show the chat need to implement this interface,
    // so that the user's message input can be sent out to the server
    public interface IChatInputAcceptor
    {
        void ProcessChatInput(string inputString, ChatType normal);
    }
}