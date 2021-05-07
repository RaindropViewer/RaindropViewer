using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibreMetaverse;
using OpenMetaverse;


namespace Raindrop
{
    //implements (actually, composites) the gridclient
    public class RaindropClient
    {
        //todo: internal eventbus for raindrop subsystems comms; include ui?
        public event Notify LoginCompleted;
        public event Notify LoginFailed;

        public GridClient Client;

        public enum RaindropState
        {
            intialise,
            connected,
            disconnected

        }

        private RaindropState clientstate;

        private string Username { get; set; }
        private string Password { get; set; }
        public string configdatapath { get; private set; }

        public RaindropClient()
        {
            clientstate = RaindropState.intialise;

            Client = new GridClient(); // instantiates the GridClient class
                                       // to the global Client object


            Client.Settings.USE_LLSD_LOGIN = true;
            const string AGNI_LOGIN_SERVER = "https://login.agni.lindenlab.com/cgi-bin/login.cgi";
            Client.Settings.LOGIN_SERVER = AGNI_LOGIN_SERVER;




            RegisterClientEvents();
        }


        private void RegisterClientEvents()
        {
            ////register events
            //Client.Appearance.AgentWearablesReply += Appearance_AgentWearablesReply;
            //Client.Appearance.AppearanceSet += Appearance_AppearanceSet;
            //Client.Appearance.CachedBakesReply += Appearance_CachedBakesReply;
            //Client.Appearance.RebakeAvatarRequested += Appearance_RebakeAvatarRequested;

            ////local chat
            Client.Self.ChatFromSimulator += Self_ChatFromSimulator;

            ////user connected/disconnected
            Client.Network.Disconnected += Network_Disconnected;
            Client.Network.LoginProgress += Network_LoginProgress;

            ////grid location update
            Client.Grid.CoarseLocationUpdate += Grid_CoarseLocationUpdate;
            Client.Network.SimChanged += Network_SimChanged;

            //log
            OpenMetaverse.Logger.OnLogMessage += Logger_OnLogMessage;
        }

        private void Logger_OnLogMessage(object message, Helpers.LogLevel level)
        {
            if (level == Helpers.LogLevel.Debug)
            {
                Debug.Log(message);
            }
            else if (level == Helpers.LogLevel.Error)
            {
                Debug.LogError(message);
            }
            else if (level == Helpers.LogLevel.Info)
            {
                Debug.Log(message);
            }
            else if (level == Helpers.LogLevel.Warning)
            {
                Debug.LogWarning(message);
            }
            else 
            {
                Debug.LogError(message);
            }
        }

        internal void setConfigPath(string app_data_Path)
        {
            this.configdatapath = app_data_Path;

        }

        private void Network_SimChanged(object sender, SimChangedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void Grid_CoarseLocationUpdate(object sender, CoarseLocationUpdateEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void Network_LoginProgress(object sender, LoginProgressEventArgs e)
        {
            if (e.Status == LoginStatus.Failed)
            {
                LoginFailed?.Invoke(); //raise event.
                Debug.LogWarning("logging in failed. \n Message: " + e.Message + "fail reason: " + e.FailReason);
            }
            else if (e.Status == LoginStatus.Success)
            {
                this.clientstate = RaindropState.connected;

                LoginCompleted?.Invoke(); //raise event.

                Disk.SaveLoad.saveCred(Username,Password, configdatapath);


                Debug.Log("logging in success. \n Message: " + e.Message);
                
            }
            else if (e.Status == LoginStatus.ConnectingToLogin)
            {
                Debug.Log("ConnectingToLogin. \n Message: " + e.Message);

            } 
            else if (e.Status == LoginStatus.ConnectingToSim)
            {
                Debug.Log("ConnectingToSim. \n Message: " + e.Message);

            }
            else
            {
                Debug.LogError("logging in not-supported. \n status type: " + e.Status.ToString() + "\n Message: " + e.Message);
            }
        }

        private void Network_Disconnected(object sender, DisconnectedEventArgs e)
        {
            Debug.Log("Disconnected!");
        }

        private void Self_ChatFromSimulator(object sender, ChatEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        //private void Appearance_RebakeAvatarRequested(object sender, RebakeAvatarTexturesEventArgs e)
        //{
        //    throw new System.NotImplementedException();
        //}

        //private void Appearance_CachedBakesReply(object sender, AgentCachedBakesReplyEventArgs e)
        //{
        //    throw new System.NotImplementedException();
        //}

        //private void Appearance_AppearanceSet(object sender, AppearanceSetEventArgs e)
        //{
        //    throw new System.NotImplementedException();
        //}

        //private void Appearance_AgentWearablesReply(object sender, AgentWearablesReplyEventArgs e)
        //{
        //    throw new System.NotImplementedException();
        //}

        //connects to SL. username is either format; ie: "firstname lastname" or "Jaken69" are both compaitble.
        public void connectTo(string username, string password)
        {

            Username = username;
            Password = password;

            // Login to Simulator
            string[] _splitname = username.Split();
            if (_splitname.Length == 1)
            {
                var temp = _splitname[0];
                _splitname = new string[2];
                _splitname[0] = temp;
                _splitname[1] = "";
            }
            else if (_splitname.Length != 2)
            {
                Debug.LogError("username is not 2 words (tip examples of valid usernames: \'Jake69 Resident\' or \'Jake Aabye\')");
                return;
            }

            var lp = new LoginParams(Global.MainRaindropInstance.Client, _splitname[0], _splitname[1], password, "raindropviewer@gmail.com", "RaindropViewer_0.1");
            Debug.Log("attempt login with name1 " + _splitname[0] + " name 2 " + _splitname[1] + " password " + password);
            Global.MainRaindropInstance.Client.Network.BeginLogin(lp);

            //UnityClient.Client.Network.Login(lp);
            // Logout of simulator
            //Client.Network.Logout();

        }

    }
}