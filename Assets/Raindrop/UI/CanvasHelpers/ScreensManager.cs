using Raindrop;
using System;
using System.Collections.Generic;
using System.Linq;
using Raindrop.ServiceLocator;
using UnityEngine;
using UnityEngine.Serialization;

//helper class that helps to pop, push, stack canvases.
//singleton.
//on awake, it searches children for all canvases.
public class ScreensManager : MonoBehaviour
{
    [Tooltip("The canvases that are available to use.")]
    public List<CanvasIdentifier> canvasControllerList = new List<CanvasIdentifier>();
    [Tooltip("The canvases that are created and in memory stack.")]
    public Stack<CanvasIdentifier> activeCanvasStack = new Stack<CanvasIdentifier>();

    public CanvasIdentifier topCanvas
    {
        get
        {
            try
            {
                return activeCanvasStack.Peek();
            }
            catch (InvalidOperationException e)
            {
                return null;
            }
        }
    }

    public CanvasType initialPage;
    
    private void Awake()
    {
        int childrenCount = transform.childCount;
        for (int i = 0; i < childrenCount ; i++)
        {
            var _ = transform.GetChild(i).GetComponent<CanvasIdentifier>();
            if (_ != null)
            {
                canvasControllerList.Add(_);
            }
        }

        try
        {
            canvasControllerList.ForEach(x => x.gameObject.SetActive(false));
            Debug.Log("Found " + canvasControllerList.Count + " canvas identifiers.");
        }
        catch (Exception e)
        {
            Debug.LogError("missing canvas controller in some child.");
            
        }
    }

    public void resetToInitialScreen()
    {
        if (! GetEulaAcceptance())
        {
            canvasControllerList.ForEach(x => x.gameObject.SetActive(false));
            Push(CanvasType.Eula);
        }
        else
        {
            canvasControllerList.ForEach(x => x.gameObject.SetActive(false));
            Push(initialPage);
        }
    }

    private bool GetEulaAcceptance()
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

    public GameObject getForegroundCanvas()
    {
        if (activeCanvasStack.Count == 0)
        {
            return null;
        }

        return activeCanvasStack.Peek().gameObject;
    }
    public CanvasType getCanvasTypeFromString(string _type)
    {
        foreach(CanvasIdentifier _ in canvasControllerList)
        {
            var thecanvastype = _.canvasType;
            if (_type == thecanvastype.ToString())
            {
                return thecanvastype;
            }
            
        }

        return CanvasType.UNKNOWN;
         
    }

    //pop current, then push the desired canvas.
    public void PopAndPush(CanvasType type)
    {
       
        
        // CanvasType theCanvasType = getCanvasTypeFromString(_type);
        // if (theCanvasType ==CanvasType.UNKNOWN)
        // {
        //     Debug.LogError("unable to get the canvas of identifer: "+ _type);
        // } 
        if (topCanvas)
        {
            PopCanvas();
        }
        Push(type);
    }
    
    // pop the current login screen, and push the game view. This is for viewing chats offline.
    // planned for debug only. 
    public void LoginWithoutNetworking()
    {
        while ((activeCanvasStack.Count() != 0))
        {
            activeCanvasStack.Pop();
        }
        PopAndPush(CanvasType.Game);
    }
    
    public void Push(CanvasType type)
    {
        //deactivate present canvas
        if (activeCanvasStack.Count() != 0)
        {
            var lastActiveCanvas = activeCanvasStack.Peek();
            lastActiveCanvas.gameObject.SetActive(false);
        }

        //find the new canvas to push.
        CanvasIdentifier desiredCanvas = canvasControllerList.Find(x => x.canvasType == type);
        if (desiredCanvas != null)
        {
            desiredCanvas.gameObject.SetActive(true);
            activeCanvasStack.Push(desiredCanvas);
        }
        else { Debug.LogWarning("The desired canvas was not found!" + desiredCanvas.ToString()); }
    }

    public void PopCanvas()
    {
        if (! topCanvas)
        {
            Debug.LogWarning("tried to pop empty canvas stack.");
            return;
        }

        //1 remove topmost
        topCanvas.gameObject.SetActive(false); //this lince causes error, as the function was called from the login thread!
        activeCanvasStack.Pop();
        
        //2 reactivate the one underneath.
        if (topCanvas)
        {
            topCanvas.gameObject.SetActive(true);
        }
    }
}
