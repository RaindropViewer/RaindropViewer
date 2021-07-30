﻿// 
// Radegast Metaverse Client
// Copyright (c) 2009-2014, Radegast Development Team
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
//     * Redistributions of source code must retain the above copyright notice,
//       this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the application "Radegast", nor the names of its
//       contributors may be used to endorse or promote products derived from
//       this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
// $Id$
//
using OpenMetaverse;
using System.Collections.Generic;

namespace Raindrop
{
    //holds the chats in memory. functions as the model layer.
    public class ChatManager
    { 
        public ChatTextManager localChatManager { get; private set; } //TODO: refactor this class to become model. UI can access data in the model as required.
        //public List<IMTextManager> IMManagerList { get; private set; }

        RaindropInstance instance;
        GridClient client { get { return instance.Client; } }
         

        public ChatManager(RaindropInstance instance)
        {
            this.instance = instance; 

            UnityEngine.Debug.Log("chatmanager being constructed");
            //setup

            //start local chat if connected to localsim
            instance.Client.Network.SimConnected += Network_SimConnected;
            instance.Client.Network.Disconnected += Network_Disconnected;

            //subscribe to localchat received event
            //localChatManager.ChatLineAdded += LocalChatManager_ChatLineAdded;
        }

        public void Dispose()
        {
            localChatManager = null;
        }

        private void Network_Disconnected(object sender, OpenMetaverse.DisconnectedEventArgs e)
        {
            Logger.Log("Network_Disconnected", Helpers.LogLevel.Info);

            //wind down chatmanager
            localChatManager.Dispose();
            localChatManager = null;
        }
         
        private void Network_SimConnected(object sender, OpenMetaverse.SimConnectedEventArgs e)
        {
            //create local chat.
            Logger.Log("Simulator Connected", Helpers.LogLevel.Info);

            if (localChatManager == null)
                localChatManager = new ChatTextManager(instance);
            
        }
         



    }
}