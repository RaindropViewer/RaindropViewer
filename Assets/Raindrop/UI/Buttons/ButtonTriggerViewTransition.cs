using System;
using Lean.Gui;
using Plugins.CommonDependencies;
using Raindrop.Services;
using UnityEngine;
using UnityEngine.UI;
using Logger = OpenMetaverse.Logger;

// This component allows you to bring up the scene that the button is supposed to lead to.
public class ButtonTriggerViewTransition : MonoBehaviour
{
    [Header("Use POP to pop the current view")]
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
            ServiceLocator.Instance.Get<UIService>().PopCanvas();
            return;
        }
        
        if (popAndPush)
        {
            ServiceLocator.Instance.Get<UIService>().PopAndPush(canvasTypeToPush);
        } else
        {
            ServiceLocator.Instance.Get<UIService>().Push(canvasTypeToPush);
        }
    }
}
