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
    private ReactiveProperty<bool> isConnected = new ReactiveProperty<bool>();
    private RaindropInstance instance => ServiceLocator.Instance.Get<RaindropInstance>();
    
    void Start()
    {
        if (instance == null || instance.Netcom == null)
            Logger.Log("raindrop instance/netcom not available", Helpers.LogLevel.Error);

        isConnected.AsObservable().Subscribe(_ => updateConnectivityUI(_));
        isConnected.Value = instance.Netcom.IsLoggedIn; // wtf.
        
        instance.Netcom.ClientLoginStatus += NetcomOnClientLoginStatus;
        instance.Netcom.ClientLoggedOut += NetcomOnClientLoggedOut;
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
        isConnected.Value = false;
    }

    private void NetcomOnClientLoginStatus(object sender, LoginProgressEventArgs e)
    {
        if (e.Status == LoginStatus.Success)
        {        
            isConnected.Value = true;
        }
        else if (e.Status == LoginStatus.Failed)
        {
            isConnected.Value = false;
        }
    }
    #endregion

    #region view
    private void show_notConnected()
    {
        this.GetComponent<Image>().color = Color.red;
    }
    private void show_isConnected()
    {
        this.GetComponent<Image>().color = Color.green;
    }
    #endregion
}
