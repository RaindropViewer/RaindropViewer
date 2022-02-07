using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapTouchHandler : MonoBehaviour, IPointerClickHandler, IDragHandler
{
    // Create a selection context if the user touches me.
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Clicked!");
    }

    // move camera if the user drags me.
    public void OnDrag(PointerEventData eventData)
    {
        
        
    }
}
