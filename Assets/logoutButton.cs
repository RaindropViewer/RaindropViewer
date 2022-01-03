using System.Collections;
using System.Collections.Generic;
using Raindrop;
using Raindrop.ServiceLocator;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class logoutButton : MonoBehaviour
{
    
    void Start()
    {
        Button btn = this.GetComponent<Button>();
        btn.onClick.AddListener(logout);
    }

    private void logout()
    {
        Debug.Log("logout requested by user UI");
        ServiceLocator.Instance.Get<RaindropInstance>().Netcom.Logout();
        return;
    }
    
}
