using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Gui;
using Raindrop;
using Raindrop.ServiceLocator;
using Raindrop.Services;
using UnityEngine;

public class ButtonTriggerViewTransition : MonoBehaviour
{
    public CanvasType canvasTypeToPush;
    public bool popCurrent;
    private void Start()
    {
        this.GetComponent<LeanButton>().OnClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (popCurrent)
        {
            ServiceLocator.Instance.Get<UIService>().canvasManager.PopAndPush(canvasTypeToPush);
        } else
        {
            ServiceLocator.Instance.Get<UIService>().canvasManager.Push(canvasTypeToPush);
        }
    }
}
