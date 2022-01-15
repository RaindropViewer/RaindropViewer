

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
        public LocalChatTextManager LocalChatManager { get; private set; } 
        public List<IMTextManager> IMChatManagers { get; private set; } 

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
            
            //make managers that we depend on.
            LocalChatManager = new LocalChatTextManager(instance, TextPrinter);
            IMChatManagers = new List<IMTextManager>();

            RegisterClientEvents(instance);
        }

        private void RegisterClientEvents(RaindropInstance instance)
        {
            instance.Client.Network.SimConnected += Network_SimConnected;
            instance.Client.Network.Disconnected += Network_Disconnected;
            LocalChatManager.ChatLineAdded += LocalChatManager_ChatLineAdded;
        }

        public ITextPrinter TextPrinter { get; set; }

        public void Dispose()
        {
            LocalChatManager = null;
            IMChatManagers = new List<IMTextManager>();
        }

        private void Network_Disconnected(object sender, OpenMetaverse.DisconnectedEventArgs e)
        {
            Logger.Log("Network_Disconnected", Helpers.LogLevel.Info);

            //wind down chatmanager
            LocalChatManager.Dispose();
            LocalChatManager = null;
        }
        
        private void LocalChatManager_ChatLineAdded(object sender, ChatLineAddedArgs chatLineAddedArgs)
        {
            Logger.Log("new message in local chat!", Helpers.LogLevel.Info);

            if (LocalChatManager == null)
                LocalChatManager = new LocalChatTextManager(instance, TextPrinter);
            Logger.Log("creating local chat in memory.", Helpers.LogLevel.Info);

            printToMainChat("todo");
        }
         
        private void Network_SimConnected(object sender, OpenMetaverse.SimConnectedEventArgs e)
        {
            //create local chat.
            Logger.Log("Simulator Connected", Helpers.LogLevel.Info);

            if (LocalChatManager == null)
                LocalChatManager = new LocalChatTextManager(instance, TextPrinter);
            Logger.Log("creating local chat in memory.", Helpers.LogLevel.Info);

            printToMainChat("Simulator Connected");
            
            //we experiment to start a IM with nuki.
            UUID agentID; //todo.
            // ah ok, so it seems use (A xor B) = C , where A is us and B is target, because this represents a link between us.
            // we send C to let the lindens know its between A and B. abit strange TBH.
            IMChatManagers.Add(new IMTextManager(instance, null,IMTextManagerType.Agent,  instance.Client.Self.AgentID ^ agentID, "cutie nuki"));
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
                LocalChatManager.ProcessBufferItem(line, true);
            });
        }
    }
}