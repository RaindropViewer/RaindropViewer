using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// put this in a panel on the UI. 
// on click, draw a ray to the object under the finger. pop up the parcel details.
// on drag, move camera by delta screentoworldpoint(x,y)
// on pinch, who the fuck knows?
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
