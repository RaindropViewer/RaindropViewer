using System.Collections;
using System.Collections.Generic;
using Plugins.CommonDependencies;
using Raindrop;
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

    private void Logout()
    {
        Debug.Log("logout requested by user UI");
        ServiceLocator.Instance.Get<RaindropInstance>().Netcom.Logout();
        return;
    }
    
}
