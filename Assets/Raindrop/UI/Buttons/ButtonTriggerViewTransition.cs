using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Gui;
using Raindrop;
using Raindrop.ServiceLocator;
using Raindrop.Services;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Logger = OpenMetaverse.Logger;

// This component allows you to bring up the scene that the button is supposed to lead to.
public class ButtonTriggerViewTransition : MonoBehaviour
{
    public CanvasType canvasTypeToPush;
    [Tooltip("If pop and push is true, it means the UI stack will be popped first, then pushed. Ala, the current UI you seeing will be gone.")]
    public bool popAndPush;
    private void Start()
    {
        try
        {
            if (this.GetComponent<LeanButton>())
            {
                this.GetComponent<LeanButton>().OnClick.AddListener(OnClick);
            }
            if (this.GetComponent<Button>())
            {
                this.GetComponent<Button>().onClick.AddListener(OnClick);
            }
        }
        catch (Exception e)
        {
            Logger.DebugLog(e.ToString());
        }
    }

    private void OnClick()
    {
        if (canvasTypeToPush == CanvasType.POP)
        {
            ServiceLocator.Instance.Get<UIService>().ScreenStackManager.PopCanvas();
            return;
        }
        
        if (popAndPush)
        {
            ServiceLocator.Instance.Get<UIService>().ScreenStackManager.PopAndPush(canvasTypeToPush);
        } else
        {
            ServiceLocator.Instance.Get<UIService>().ScreenStackManager.Push(canvasTypeToPush);
        }
    }
}
