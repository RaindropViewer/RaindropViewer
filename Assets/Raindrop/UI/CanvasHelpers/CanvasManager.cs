using Raindrop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//helper class that helps to pop, push, stack canvases.
//singleton.
//on awake, it searches children for all canvases.
public class CanvasManager : MonoBehaviour
{
    [SerializeField]
    //public GameObject[] CanvasPrefabsList;

    public List<CanvasIdentifier> canvasControllerList = new List<CanvasIdentifier>();
    public Stack<CanvasIdentifier> activeCanvasStack = new Stack<CanvasIdentifier>();
    
    private void Awake()
    {
        int childrenCount = transform.childCount;
        for (int i = 0; i < childrenCount ; i++)
        {
            //GameObject panelRoot = Instantiate(prefab) as GameObject;
            //panelRoot.transform.SetParent(this.transform);
            canvasControllerList.Add(transform.GetChild(i).GetComponent<CanvasIdentifier>());
        }
        canvasControllerList.ForEach(x => x.gameObject.SetActive(false));
        Debug.Log("Found " + canvasControllerList.Count + " canvas identifiers.");
    }

    public void resetToLoginScreen()
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
