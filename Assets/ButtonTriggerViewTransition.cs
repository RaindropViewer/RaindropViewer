using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Gui;
using Raindrop;
using Raindrop.ServiceLocator;
using Raindrop.Services;
using UnityEngine;
using Logger = OpenMetaverse.Logger;

public class ButtonTriggerViewTransition : MonoBehaviour
{
    public CanvasType canvasTypeToPush;
    public bool popCurrent;
    private void Start()
    {
        try
        {
            this.GetComponent<LeanButton>().OnClick.AddListener(OnClick);
        }
        catch (Exception e)
        {
            Logger.DebugLog(e.ToString());
        }
    }

    private void OnClick()
    {
        if (canvasTypeToPush == CanvasType.NONE)
        {
            ServiceLocator.Instance.Get<UIService>().canvasManager.PopCanvas();
            return;
        }
        
        if (popCurrent)
        {
            ServiceLocator.Instance.Get<UIService>().canvasManager.PopAndPush(canvasTypeToPush);
        } else
        {
            ServiceLocator.Instance.Get<UIService>().canvasManager.Push(canvasTypeToPush);
        }
    }
}
