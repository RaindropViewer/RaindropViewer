using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Gui;
using Plugins.CommonDependencies;
using Raindrop;
using UnityEngine;

public class ButtonSound : MonoBehaviour
{
    
    public RaindropInstance Instance => ServiceLocator.Instance.Get<RaindropInstance>();

    private LeanButton btn;
    void Awake()
    {
        try
        {
            btn = this.GetComponent<LeanButton>();
            
            if (btn)
            {
                this.GetComponent<LeanButton>().OnClick.AddListener(OnClick);
            }
        }
        catch (Exception e)
        {
            OpenMetaverse.Logger.DebugLog(e.ToString());
        }
    }

    private void OnClick()
    {
        if (btn.interactable)
        {
            Instance.MediaManager.PlayUISound(UISounds.Click);
        }
    }
}
