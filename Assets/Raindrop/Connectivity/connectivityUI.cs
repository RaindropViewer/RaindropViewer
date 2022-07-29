using System;
using System.Collections;
using System.Collections.Generic;
using OpenMetaverse;
using Plugins.CommonDependencies;
using Raindrop;
using UnityEngine;
using Raindrop.Netcom;
using UniRx;
using UnityEngine.UI;
using Logger = OpenMetaverse.Logger;

// change the color of the image to red or green based on connection to server.
[RequireComponent(typeof(Image))]
public class connectivityUI : MonoBehaviour
{
    private RaindropInstance instance => ServiceLocator.Instance.Get<RaindropInstance>();
    
    void Start()
    {
        if (instance == null || instance.Netcom == null)
            Logger.Log("raindrop instance/netcom not available", Helpers.LogLevel.Error);

        instance.Netcom.ClientLoginStatus += NetcomOnClientLoginStatus;
        instance.Netcom.ClientLoggedOut += NetcomOnClientLoggedOut;
    }

    private void OnDisable()
    {
        if (instance == null)
            return;
        instance.Netcom.ClientLoginStatus -= NetcomOnClientLoginStatus;
        instance.Netcom.ClientLoggedOut -= NetcomOnClientLoggedOut;
    }

    private void updateConnectivityUI(bool isConnected)
    {
        if (! isConnected)
        {
            show_notConnected();
        }
        else
        {
            show_isConnected();
        }
    }

    #region Subscribe to backend connectivity events
    private void NetcomOnClientLoggedOut(object sender, EventArgs e)
    {
        updateConnectivityUI(false);
    }

    private void NetcomOnClientLoginStatus(object sender, LoginProgressEventArgs e)
    {
        if (e.Status == LoginStatus.Success)
        {
            updateConnectivityUI(true);
        }
        else if (e.Status == LoginStatus.Failed)
        {
            updateConnectivityUI(false);
        }
    }
    #endregion

    #region view
    private void show_notConnected()
    {
        if (this != null)
        {
            this.GetComponent<Image>().color = Color.red;
        }
    }
    private void show_isConnected()
    {
        if (this != null)
        {
            this.GetComponent<Image>().color = Color.green;
        }
    }
    #endregion
}
