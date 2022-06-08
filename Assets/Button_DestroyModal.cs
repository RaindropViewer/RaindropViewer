using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Button_DestroyModal : MonoBehaviour
{
    public Button btn;
    public GameObject modal;
    void Awake()
    {
        btn.onClick.AddListener(destroy);
    }

    private void destroy()
    {
        Destroy(modal);
        
    }
}
