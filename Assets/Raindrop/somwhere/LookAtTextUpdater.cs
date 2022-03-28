using System;
using Raindrop.UI.Views;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

//subscribe to event and updates the current sim text accordingly
[RequireComponent(typeof(TMPro.TextMeshProUGUI))]
public class LookAtTextUpdater : MonoBehaviour
{
    public GameObject mapUIGO;
    private MapUIView _mapUIView;

    private TMP_Text tmp;

    private void Awake()
    {
        _mapUIView = mapUIGO.GetComponent<MapUIView>();
        tmp = this.GetComponent<TMP_Text>();

        //sub
        //mapUI.StartProcess();
    }

    private void Start()
    {
        var prez = _mapUIView.getPresenter();
        prez.MapClicked += bl_UserMapClick; // register with an event
    }


    // event handler
    public void bl_UserMapClick(object sender, string text)
    {
        if (tmp != null)
        {
            this.tmp.SetText(text);
        }
    }
}
