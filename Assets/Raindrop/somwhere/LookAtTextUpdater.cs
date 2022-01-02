using Raindrop.UI.Views;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//subscribe to event and updates the current sim text accordingly
[RequireComponent(typeof(TMPro.TextMeshPro))]
public class LookAtTextUpdater : MonoBehaviour
{
    public GameObject mapUIGO;
    private MapUI mapUI;

    private TMPro.TextMeshPro tmp;

    private void Awake()
    {
        mapUI = mapUIGO.GetComponent<MapUI>();
        tmp = this.GetComponent<TMPro.TextMeshPro>();

        //sub
        mapUI.getPresenter().MapClicked += bl_UserMapClick; // register with an event
        //mapUI.StartProcess();
    }


    // event handler
    public void bl_UserMapClick(object sender, string text)
    {
        if (tmp != null)
        {
            this.tmp.SetText(text);
        }
    }

    // Update is called once per frame
    void Update()
    {

        
    }
}
