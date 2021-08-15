using Raindrop.Presenters;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;


namespace Raindrop.UI.Views
{
    /// <summary>
    /// A view that interprets the touch controls.
    /// </summary>
    public class PanAndZoomInterpreterModule : MonoBehaviour
    {
        [SerializeField]
        public GameObject objectToMove;
        private MapLookAt mm;
        private TouchScreenInteractionInstance fingerInteraction;

        [SerializeField]
        public GameObject mapCam;
        private Camera cam;

        [SerializeField]
        public string layerName = "minimap"; //to project into the 'fake map plane'


        private Vector3 worldPoint_startPos;


        void Awake()
        {
            fingerInteraction = new TouchScreenInteractionInstance();

            mm = objectToMove.GetComponent<MapLookAt>();
            cam = mapCam.GetComponent<Camera>();

            EnhancedTouchSupport.Enable();
        }

        void Update()
        {
            fingerInteraction.updateInteractionState(); //get new touch data!


            if (fingerInteraction.isPan())
            {
                var pandelta = fingerInteraction.getPanDelta();
                mm.MoveFloatingLookAt_Relative(pandelta.x, pandelta.y);
            }
            if (fingerInteraction.isZoom())
            {
                var zoomdelta = fingerInteraction.getZoomDelta();
                Debug.Log("2 finger not supported yet!");
                //mm.MoveFloatingLookAt_Relative(pandelta.x, pandelta.y);
            }

            if (fingerInteraction.isDifferentInteraction())
            {
                //finalise the current-previous interaction.
                var pandelta = fingerInteraction.getPanDelta();
                mm.MoveFloatingLookAt_Relative_OnRelease(pandelta.x, pandelta.y);
            }

            //if (currentState == InteractionState.pan)
            //{

            //    Vector3 ray_touchNow = cam.ScreenToWorldPoint(touchInitialPosition);
            //    if (isTouchBegan)
            //    {
            //        touchInitialPosition = Touch.activeFingers[0].currentTouch.screenPosition;
            //        worldPoint_startPos = cam.ScreenToWorldPoint(touchInitialPosition);
            //    }
            //    else if (isTouchContinue)
            //    {

            //        var touchPresentPosition = Touch.activeFingers[0].currentTouch.screenPosition;
            //        Vector3 worldPoint_now = cam.ScreenToWorldPoint(touchInitialPosition);


            //        var direction = worldPoint_startPos - worldPoint_now;
            //    }
            //    else if (isTouchEnd)
            //    {
            //        Debug.Log("finger lifted and data is set.");
            //        var touchPresentPosition = Touch.activeFingers[0].currentTouch.screenPosition;
            //        Vector3 worldPoint_now = cam.ScreenToWorldPoint(touchInitialPosition);


            //        var direction = worldPoint_startPos - worldPoint_now;
            //        mm.MoveFloatingLookAt_Relative_OnRelease(direction.x, direction.z);
            //    }


            //    Touch activeTouch = Touch.activeFingers[0].currentTouch;
            //    Debug.Log($"Phase: {activeTouch.phase} | Position: {activeTouch.startScreenPosition}");

            //} else if (currentState == InteractionState.zoom){

            //    if (Touch.activeFingers[1].currentTouch.isInProgress)


            //        mm.MoveFloatingLookAt_Relative(direction.x, direction.z);

            //    Touch activeTouch = Touch.activeFingers[0].currentTouch;
            //    Debug.Log($"Phase: {activeTouch.phase} | Position: {activeTouch.startScreenPosition}");
            //}


            return;

        }

    }

}