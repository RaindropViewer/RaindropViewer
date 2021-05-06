using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibreMetaverse;
using OpenMetaverse;

public class Globals : MonoBehaviour
{
    //public static GridClient Client;

    public string m_Path { get; private set; }

    void Start()
    { 
        //Client = new GridClient(); // instantiates the GridClient class
                                   // to the global Client object
                                   // Login to Simulator
                                   //Client.Network.Login("FirstName", "LastName", "Password", "FirstBot", "1.0");
                                   // Wait for a Keypress
                                   //Console.ReadLine();
                                   // Logout of simulator
                                   //Client.Network.Logout();


        ////register events
        //Client.Appearance.AgentWearablesReply += Appearance_AgentWearablesReply;
        //Client.Appearance.AppearanceSet += Appearance_AppearanceSet;
        //Client.Appearance.CachedBakesReply += Appearance_CachedBakesReply;
        //Client.Appearance.RebakeAvatarRequested += Appearance_RebakeAvatarRequested;

        ////local chat
        //Client.Self.ChatFromSimulator += Self_ChatFromSimulator;

        ////user connected/disconnected
        //Client.Network.Disconnected += Network_Disconnected;
        //Client.Network.LoginProgress += Network_LoginProgress;

        ////grid location update
        //Client.Grid.CoarseLocationUpdate += Grid_CoarseLocationUpdate;
        //Client.Network.SimChanged += Network_SimChanged;


        //Get the path of the Game data folder
        m_Path = Application.persistentDataPath;

        //Output the Game data path to the console
        Debug.Log("dataPath : " + m_Path);

        var myCred = new Raindrop.Types.Credential();
        myCred.User = "username69";
        myCred.Pass = "password96";

        var myRW = new Disk.ReadWrite();
        myRW.saveCredentials(myCred,m_Path);
    }

    //private void Network_SimChanged(object sender, SimChangedEventArgs e)
    //{
    //    throw new System.NotImplementedException();
    //}

    //private void Grid_CoarseLocationUpdate(object sender, CoarseLocationUpdateEventArgs e)
    //{
    //    throw new System.NotImplementedException();
    //}

    //private void Network_LoginProgress(object sender, LoginProgressEventArgs e)
    //{
    //    throw new System.NotImplementedException();
    //}

    //private void Network_Disconnected(object sender, DisconnectedEventArgs e)
    //{
    //    Debug.Log("Disconnected!");
    //}

    //private void Self_ChatFromSimulator(object sender, ChatEventArgs e)
    //{
    //    throw new System.NotImplementedException();
    //}

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

    // Update is called once per frame
    void Update()
    {
        
    }
}
