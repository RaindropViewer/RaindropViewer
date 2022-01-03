using System;
using System.Collections;
using System.Collections.Generic;
using OpenMetaverse;
using Raindrop;
using UnityEngine;
using Raindrop.Netcom;
using Raindrop.ServiceLocator;
using UnityEngine.UI;

// change the color of the image to red or green based on connection to server.
[RequireComponent(typeof(Image))]
public class connectivityUI : MonoBehaviour
{
    
    void Start()
    {
        notConnected();

        var _instance = ServiceLocator.Instance.Get<RaindropInstance>();
        _instance.Netcom.ClientLoginStatus += NetcomOnClientLoginStatus;
        _instance.Netcom.ClientLoggedOut += NetcomOnClientLoggedOut;

    }

    private void NetcomOnClientLoggedOut(object sender, EventArgs e)
    {
        notConnected();
    }

    private void NetcomOnClientLoginStatus(object sender, LoginProgressEventArgs e)
    {
        if (e.Status == LoginStatus.Success)
        {
            yesConnected();
        }
        else if (e.Status == LoginStatus.Failed)
        {
            notConnected();
        }
    }

    private void notConnected()
    {
        this.GetComponent<Image>().color = Color.red;
    }
    private void yesConnected()
    {
        this.GetComponent<Image>().color = Color.green;
    }
}
