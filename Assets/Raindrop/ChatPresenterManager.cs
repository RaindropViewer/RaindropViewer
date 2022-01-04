

using System;
using OpenMetaverse;
using System.Collections.Generic;
using Raindrop.Services;
using UnityEngine;
using Logger = OpenMetaverse.Logger;

namespace Raindrop
{
    //holds the chats in memory. functions as the model layer.
    // equivalent to a chat window with all the tabs and chats the user is chatting in.
    public class ChatPresenterManager
    { 
        public LocalChatTextManager LocalLocalChat { get; private set; } 
        public List<IMTextManager> IMChats { get; private set; } 

        RaindropInstance instance;

        public ChatPresenterManager(RaindropInstance instance, ITextPrinter textPrinter)
        {
            UnityEngine.Debug.Log("chatmanager being constructed");
            
            if (instance == null)
                Debug.LogError("instance is not avaailbe in chat prensetermgr");
            if (textPrinter == null)
                Debug.LogError("textPrinter is not avaailbe in chat prensetermgr");
            this.instance = instance;
            TextPrinter = textPrinter;
            
            RegisterClientEvents(instance);

            //make the chat manager. (seems like we destroy it on disconnection.)
            LocalLocalChat = new LocalChatTextManager(instance, TextPrinter);
            IMChats = new List<IMTextManager>();
        }

        private void RegisterClientEvents(RaindropInstance instance)
        {
            instance.Client.Network.SimConnected += Network_SimConnected;
            instance.Client.Network.Disconnected += Network_Disconnected;
            LocalLocalChat.ChatLineAdded += LocalChatManager_ChatLineAdded;
        }

        public ITextPrinter TextPrinter { get; set; }

        public void Dispose()
        {
            LocalLocalChat = null;
            IMChats = new List<IMTextManager>();
        }

        private void Network_Disconnected(object sender, OpenMetaverse.DisconnectedEventArgs e)
        {
            Logger.Log("Network_Disconnected", Helpers.LogLevel.Info);

            //wind down chatmanager
            LocalLocalChat.Dispose();
            LocalLocalChat = null;
        }
        
        private void LocalChatManager_ChatLineAdded(object sender, ChatLineAddedArgs chatLineAddedArgs)
        {
            Logger.Log("new message in local chat!", Helpers.LogLevel.Info);

            if (LocalLocalChat == null)
                LocalLocalChat = new LocalChatTextManager(instance, TextPrinter);
            Logger.Log("creating local chat in memory.", Helpers.LogLevel.Info);

            printToMainChat("todo");
        }
         
        private void Network_SimConnected(object sender, OpenMetaverse.SimConnectedEventArgs e)
        {
            //create local chat.
            Logger.Log("Simulator Connected", Helpers.LogLevel.Info);

            if (LocalLocalChat == null)
                LocalLocalChat = new LocalChatTextManager(instance, TextPrinter);
            Logger.Log("creating local chat in memory.", Helpers.LogLevel.Info);

            printToMainChat("Simulator Connected");
            
            //we experiment to start a IM with nuki.
            UUID agentID; //todo.
            // ah ok, so it seems use (A xor B) = C , where A is us and B is target, because this represents a link between us.
            // we send C to let the lindens know its between A and B. abit strange TBH.
            IMChats.Add(new IMTextManager(instance, null,IMTextManagerType.Agent,  instance.Client.Self.AgentID ^ agentID, "cutie nuki"));
        }

        public void printToMainChat(string message)
        {
            //print success msg.
            ChatBufferItem line = new ChatBufferItem(
                DateTime.Now,
                string.Empty,
                UUID.Zero,
                message,
                ChatBufferTextStyle.Normal
            );
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                //process printing in main thread.
                LocalLocalChat.ProcessBufferItem(line, true);
            });
        }
    }
}