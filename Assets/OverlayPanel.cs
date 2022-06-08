using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

//the map overlay panel, that is laid on top of the map UI when a place is selected.
public class OverlayPanel : MonoBehaviour
{
    public GameObject panel;
    private void Awake()
    {
        //register my root.
        
    }

    public void Show()
    {
        panel.SetActive(true);
    }
    
    public void Disappear()
    {
        panel.SetActive(false);
    }
}
