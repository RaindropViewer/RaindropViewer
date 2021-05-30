using Raindrop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//helper class that helps to pop, push, stack canvases.
//singleton.
//on awake, it searches children for all canvases.
public class CanvasManager : Singleton<CanvasManager>
{
    List<CanvasIdentifier> canvasControllerList;
    //CanvasIdentifier lastActiveCanvas;
    public Stack<CanvasIdentifier> activeCanvasStack = new Stack<CanvasIdentifier>();

    protected override void Awake()
    {
        base.Awake();
        canvasControllerList = FindObjectsOfType<CanvasIdentifier>().ToList();
        canvasControllerList.ForEach(x => x.gameObject.SetActive(false));
        Debug.Log("Found " + canvasControllerList.Count + " canvas identifiers." );

     

    }

    public void reinitToLoginScreen()
    {
        canvasControllerList.ForEach(x => x.gameObject.SetActive(false));
        pushCanvas(CanvasType.Login);
    }

    public GameObject getForegroundCanvas()
    {
        if (activeCanvasStack.Count == 0)
        {
            return null;
        }

        return activeCanvasStack.Peek().gameObject;
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
            Debug.LogWarning("tried to pop empty canvas stack.");
            return;
        }

        var lastActiveCanvas = activeCanvasStack.Peek();
        if (lastActiveCanvas != null)
        {
            lastActiveCanvas.gameObject.SetActive(false); //this lince causes error, as the function was called from the login thread!
            activeCanvasStack.Pop();
        }


    }

}
