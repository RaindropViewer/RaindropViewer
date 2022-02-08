using Raindrop;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenMetaverse;
using Raindrop.ServiceLocator;
using Raindrop.Services;
using UnityEngine;
using UnityEngine.Serialization;

//helper class that helps to pop, push, stack canvases.
//on start, it searches children for all canvases.
public class ScreensManager : MonoBehaviour
{ 
    [Tooltip("The canvases that are available to use.")]
    public List<CanvasIdentifier> availableCanvases = new List<CanvasIdentifier>();
    [Tooltip("The canvases that are created and in memory stack.")]
    public Stack<CanvasIdentifier> activeCanvasStack = new Stack<CanvasIdentifier>();

    public CanvasIdentifier TopCanvas
    {
        get
        {
            try
            {
                return activeCanvasStack.Peek();
            }
            catch (InvalidOperationException e)
            {
                OpenMetaverse.Logger.Log("pop canvas has error : " + e.ToString(), Helpers.LogLevel.Error);
                return null;
            }
        }
    }

    public CanvasType initialPage;
    
    private void Start()
    {
        int childrenCount = transform.childCount;
        for (int i = 0; i < childrenCount ; i++)
        {
            var _ = transform.GetChild(i).GetComponent<CanvasIdentifier>();
            if (_ != null)
            {
                availableCanvases.Add(_);
            }
        }

        try
        {
            availableCanvases.ForEach(x => x.gameObject.SetActive(false));
        }
        catch (Exception e)
        {
            Debug.LogError("missing canvas controller in some child.");
            
        }
    }

    public void ResetToInitialScreen()
    {
        if (! IsEulaAccepted())
        {
            availableCanvases.ForEach(x => x.gameObject.SetActive(false));
            Push(CanvasType.Eula);
        }
        else
        {
            availableCanvases.ForEach(x => x.gameObject.SetActive(false));
            Push(initialPage);
        }
    }

    private bool IsEulaAccepted()
    {
        if (ServiceLocator.Instance.Get<RaindropInstance>().GlobalSettings["EulaAccepted"])
        {
            return true;
        }
        else
        {
            return false;
        } 
    }

    public GameObject GetForegroundCanvas()
    {
        if (activeCanvasStack.Count == 0)
        {
            return null;
        }

        return activeCanvasStack.Peek().gameObject;
    }

    //pop current, then push the desired canvas.
    public void PopAndPush(CanvasType type)
    {
        if (TopCanvas)
        {
            PopCanvas();
        }
        Push(type);
    }
    public void Push(CanvasType type)
    {
        CanvasIdentifier desiredCanvas = availableCanvases.Find(x => x.canvasType == type);
     
        //check if the canvas is not supported:
        if (desiredCanvas == null)
        {
            OpenMetaverse.Logger.Log(
                "The desired canvas was not found!" + type.ToString(),
                Helpers.LogLevel.Warning
            );
            var ui = ServiceLocator.Instance.Get<UIService>();
            ui.modalManager.showModalNotification(
                "The desired feature is not implemented yet: ",
                type.ToString() + " UI + \n \n + Stay tuned for updates!");
            var instance = ServiceLocator.Instance.Get<RaindropInstance>();
            instance.MediaManager.PlayUISound(UISounds.Warning);
            return;
        }
        
        //deactivate present canvas, if any.
        //(because the old canvas may appear above the new one - due to heirachy ordering)
        if (activeCanvasStack.Count() != 0)
        {
            var lastActiveCanvas = activeCanvasStack.Peek();
            lastActiveCanvas.gameObject.SetActive(false);
        }
        //push it
        desiredCanvas.gameObject.SetActive(true);
        activeCanvasStack.Push(desiredCanvas);
    }

    public void PopCanvas()
    {
        if (! TopCanvas)
        {
            OpenMetaverse.Logger.Log("tried to pop empty canvas stack.", Helpers.LogLevel.Warning);
            return;
        }

        //1 remove topmost
        TopCanvas.gameObject.SetActive(false); //this lince causes error, as the function was called from the login thread!
        activeCanvasStack.Pop();
        
        //2 reactivate the one underneath.
        if (TopCanvas)
        {
            TopCanvas.gameObject.SetActive(true);
        }
    }
}
