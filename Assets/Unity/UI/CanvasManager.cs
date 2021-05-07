using Raindrop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CanvasManager : Singleton<CanvasManager>
{
    List<CanvasIdentifier> canvasControllerList;
    //CanvasIdentifier lastActiveCanvas;
    public Stack<CanvasIdentifier> activeCanvasStack = new Stack<CanvasIdentifier>();


    protected override void Awake()
    {
        base.Awake();
        canvasControllerList = GetComponentsInChildren<CanvasIdentifier>().ToList();
        canvasControllerList.ForEach(x => x.gameObject.SetActive(false));
        pushCanvas(CanvasType.Login);

        //canvas manager (attached to the UI GO) should register with the globals. so that the globals can acess them
        Global.RaindropVM.registerWithRaindropClient(this);

    }
    public void pushCanvas(CanvasType _type)
    {
        if (activeCanvasStack.Count() != 0)
        {
            var lastActiveCanvas = activeCanvasStack.Peek();
            //i think the old canvas will essentially be 'frozen in time'
            lastActiveCanvas.gameObject.SetActive(false);
        }

        CanvasIdentifier desiredCanvas = canvasControllerList.Find(x => x.canvasType == _type);
        if (desiredCanvas != null)
        {
            desiredCanvas.gameObject.SetActive(true);
            activeCanvasStack.Push (desiredCanvas);
        }
        else { Debug.LogWarning("The desired canvas was not found!"); }
    }

    public void popCanvas()
    {
        if (activeCanvasStack.Count() == 0)
        {
            return;
        }

        var lastActiveCanvas = activeCanvasStack.Peek();
        if (lastActiveCanvas != null)
        {
            lastActiveCanvas.gameObject.SetActive(false);
        }


    }
}
