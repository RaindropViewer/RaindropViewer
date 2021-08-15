using System.Collections.Generic;
using UnityEngine;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

/// <summary>
/// A instance of a user interacting with the screen.
/// Responsible for tracking fingers and counting them over their lifecycle.
/// Responsible for providing finger zoom delta and finger move delta.
/// </summary>
public class TouchScreenInteractionInstance
{
    // interaction state.
    internal enum InteractionType
    {
        none, //no touch/ more than 2 fingers
        zoom, //pinching
        pan   //single finger.
    }
    private InteractionType previousType = InteractionType.none;
    private bool interactionTypeHasChanged = false; // if pan->zoom / none->zoom, etc.

    // internal memory of touches.
    private List<Vector2> initialTouchPositions = new List<Vector2>(); //each finger's first touch position.
    private List<Vector2> currentTouchPositions = new List<Vector2>();
    //private Collider touchFocus; //what we think that the user is touching

    public Vector2 oneFingerMoveDelta { get; private set; }
    public float twoFingerPinchDelta { get; private set; }
    public Vector2 twoFingerMoveDelta { get; private set; }

    /// <summary>
    /// update the internal status.
    /// </summary>
    public void updateInteractionState()
    {
        //touch 
        InteractionType presentType = fingerCountToInteractionType(Touch.activeFingers.Count);
        interactionTypeHasChanged = isStateChanged(previousType, presentType);
        if (interactionTypeHasChanged)
        {
            UpdateInitialFingerPositions();
            //getProbableTouchFocus();
            return;
        }
        else
        {
            updatePresentFingerPositions();
            updateGlobalAccessibleStates(presentType);

        }



    }

    private void updateGlobalAccessibleStates(InteractionType presentType)
    {
        if (presentType == InteractionType.zoom)
        {
            twoFingerPinchDelta = Vector2.Distance(initialTouchPositions[0], initialTouchPositions[1]);
            Vector2 ave_initial = (initialTouchPositions[0] + initialTouchPositions[1]) / 2;
            Vector2 ave_current = (currentTouchPositions[0] + currentTouchPositions[1]) / 2;

            Vector2 direction = initialTouchPositions[0] - currentTouchPositions[0];
            twoFingerMoveDelta = direction;
        }
        if (presentType == InteractionType.pan)
        {
            Vector2 direction = initialTouchPositions[0] - currentTouchPositions[0];
            oneFingerMoveDelta = direction;
        }
    }

    internal InteractionType GetCurrentInteractionState()
    {
        return previousType;
    }

    public bool isDifferentInteraction()
    {
        return interactionTypeHasChanged;
    }

    //private void getProbableTouchFocus()
    //{
    //    if (Touch.activeFingers.Count == 0)
    //    {
    //        touchFocus = null;
    //    } else
    //    {
    //        Vector2 pos = Touch.activeFingers[0].screenPosition;
    //        touchFocus =  pos;

    //        worldPoint_startPos = cam.ScreenToWorldPoint(pos);
    //    }
    //}

    private void updatePresentFingerPositions()
    {
        currentTouchPositions.Clear();
        for (int i = 0; i < Touch.activeFingers.Count; i++)
        {
            currentTouchPositions.Add(Touch.activeFingers[i].screenPosition);

        }
    }

    private void UpdateInitialFingerPositions()
    {
        initialTouchPositions.Clear();
        for (int i = 0; i < Touch.activeFingers.Count; i++)
        {
            initialTouchPositions.Add(Touch.activeFingers[i].screenPosition);
        }
    }

    /// <summary>
    /// convert numberOfFingers into InteractionState
    /// </summary>
    /// <param name="fingerCount"></param>
    /// <returns></returns>
    private InteractionType fingerCountToInteractionType(int fingerCount)
    {
        switch (fingerCount)
        {
            case 0:
                return InteractionType.none;
            case 1:
                return InteractionType.pan;
            case 2:
                return InteractionType.zoom;
            default:
                return InteractionType.none;
        }

    }

    /// <summary>
    /// get the relative zoom change from present zoom (0).
    /// </summary>
    /// <returns></returns>
    public float getZoomDelta()
    {
        return twoFingerPinchDelta;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool isZoom()
    {
        return previousType == InteractionType.zoom;
    }

    /// <summary>
    /// get the relative pan change from present pan (0,0).
    /// </summary>
    /// <returns></returns>
    public Vector2 getPanDelta()
    {
        //var direction = touchInitialPosition - worldPoint_now;
        return oneFingerMoveDelta;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool isPan()
    {
        return previousType == InteractionType.pan;
    }


    /// <summary>
    /// Check if internal 'interaction' state is changed.
    /// </summary>
    /// <param name="newState"></param>
    private bool isStateChanged(InteractionType oldState, InteractionType newState)
    {
        if (oldState == newState)
        {
            return false;
        }
        return true;
    }
}

