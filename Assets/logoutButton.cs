using System.Collections;
using System.Collections.Generic;
using Raindrop;
using Raindrop.ServiceLocator;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LogoutButton : MonoBehaviour
{
    //get own references.
    void Awake()
    {
        Button btn = this.GetComponent<Button>();
        btn.onClick.AddListener(Logout);
    }
    
    //get others' references, if required.
    void Start()
    {
    }

    private void Logout()
    {
        Debug.Log("logout requested by user UI");
        ServiceLocator.Instance.Get<RaindropInstance>().Netcom.Logout();
        return;
    }
    
}
