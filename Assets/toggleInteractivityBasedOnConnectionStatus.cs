using System.Collections;
using System.Collections.Generic;
using OpenMetaverse;
using Raindrop;
using Raindrop.ServiceLocator;
using UnityEngine;

//this one have bug : update does not run when disabled.
public class toggleInteractivityBasedOnConnectionStatus : MonoBehaviour
{
    private RaindropInstance _instance;

    // Start is called before the first frame update
    void Start()
    {
        notConnected();
        
        _instance = ServiceLocator.Instance.Get<RaindropInstance>();
    }


    // Update is called once per frame
    void Update()
    {
        if (_instance.Client.Network.Connected == false)
        {
            notConnected();
        }
        else
        {
            yesConnected();
        }
    }
    
    
    private void notConnected()
    {
        this.gameObject.SetActive(false);
    }
    private void yesConnected()
    {        
        this.gameObject.SetActive(true);

    }
}
