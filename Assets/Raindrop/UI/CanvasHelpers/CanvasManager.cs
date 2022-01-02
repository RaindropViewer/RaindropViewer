using Raindrop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Raindrop.ServiceLocator;
using UnityEngine;

//helper class that helps to pop, push, stack canvases.
//singleton.
//on awake, it searches children for all canvases.
public class CanvasManager : MonoBehaviour
{
    [Tooltip("The canvases that are available to use.")]
    public List<CanvasIdentifier> canvasControllerList = new List<CanvasIdentifier>();
    [Tooltip("The canvases that are created and in memory stack.")]
    public Stack<CanvasIdentifier> activeCanvasStack = new Stack<CanvasIdentifier>();

    public CanvasType main;
    
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
        if (! getEulaAcceptance())
        {
            canvasControllerList.ForEach(x => x.gameObject.SetActive(false));
            pushCanvas(CanvasType.Eula);
        }
        else
        {
            canvasControllerList.ForEach(x => x.gameObject.SetActive(false));
            pushCanvas(main);
        }
    }

    private bool getEulaAcceptance()
    {
        if (ServiceLocator.Instance.Get<RaindropInstance>().GlobalSettings["EulaAccepted"])
        {
            Debug.Log("User has already accepted the EULA");
            return true;
        }
        else
        {
            Debug.Log("User has NOT accepted the EULA");
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
    public void pushCanvas(string _type)
    {
        pushCanvasWithOrWithoutPop(_type, false);
    }

    //isPopCurrentActiveCanvas true will pop the current top canvas and then push the new desired one.
    public void pushCanvasWithOrWithoutPop(string _type, bool isPopCurrentActiveCanvas)
    {
        CanvasType theCanvasType = getCanvasTypeFromString(_type);
        if (theCanvasType ==CanvasType.UNKNOWN)
        {
            Debug.LogError("unable to get the canvas of identifer: "+ _type);
        }

        if (isPopCurrentActiveCanvas)
        {
            popCanvas();
        }
        pushCanvas(theCanvasType);
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
