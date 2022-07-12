using Raindrop.Presenters;
using System;
using System.Collections;
using OpenMetaverse;
using Plugins.CommonDependencies;
using Raindrop.Map.Model;
using Raindrop.Services;
using Raindrop.UI.Pancake;
using Raindrop.Utilities;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem.EnhancedTouch;
using Ray = UnityEngine.Ray;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;


namespace Raindrop.UI.Views
{
    /// <summary>
    /// Requires PanAndZoom game script.
    /// interprets and gives meaning to the pan and zoom Events raised in class PanAndZoom.
    /// </summary>
    [RequireComponent(typeof(PanAndZoom))]
    public class PanAndZoom_MapperModule : MonoBehaviour
    {
        [SerializeField]
        public MapLookAt lookAt;
        // private TouchScreenInteractionTracker fingerInteraction;

        public PanAndZoom pnz;
        private int layerMask;

        public UIService UI => ServiceLocator.Instance.Get<UIService>();
        
        // [SerializeField]
        // public string layerName = "minimap"; //to project into the 'fake map plane'

        void Awake()
        {
            Assert.IsTrue(pnz != null);
            pnz.ignoreUI = true;
            Assert.IsTrue(lookAt != null);
            
            EnhancedTouchSupport.Enable();
        }

        void Start()
        {
            //sub to the events in pnz.
            pnz.onSwipe += PnzOnonSwipe;
            pnz.onPinch += PnzOnonPinch;
            pnz.onTap += PnzOnonTap;

        }

        private void PnzOnonTap(Vector2 pos)
        {
            var camera = lookAt.orthoCam.Cam;

            //check if tile present.
            
            RaycastHit rayHit;
            Vector3 worldPoint = camera.ScreenToWorldPoint(pos);
            Ray ray = camera.ScreenPointToRay(pos);

            GameObject res = null;
            if (Physics.Raycast(ray, out rayHit, 500, layerMask))
            {
                res = rayHit.transform.gameObject;
                if (IsTile(res))
                {
                    //open UI of the location
                    var mapSpaceHit = rayHit.point;
                    var global_Handle = MapSpaceConverters.MapSpace2Handle(mapSpaceHit);

                    UI.MapFacade.OnFocusMapPosition(global_Handle, rayHit);
                }
            }
            
        }

        private bool IsTile(GameObject res)
        {
            // todo: implemeent is map tile logic.
            if (res)
            {
                return true;
            }
            return false;
        }

        private void PnzOnonPinch(float oldD, float newD)
        {
            lookAt.SetLookAt_Zoom_ByDelta(oldD/newD);
        }

        private void PnzOnonSwipe(Vector2 delta) //delta means the delta from previous "frame"
        {
            //delta is in pixels.
            //scale the delta by the resolution?
            var deltaWS =
                lookAt.orthoCam.Cam.ScreenToWorldPoint(delta)
                - lookAt.orthoCam.Cam.ScreenToWorldPoint(Vector2.zero);

            lookAt.SetLookAt_ByDelta(deltaWS.x, deltaWS.y);
        }

        private void Pnz_onTap(Vector2 obj)
        {
            Debug.Log("tapped");
        }

        void Update()
        {
            // //1. get current touch status
            // fingerInteraction.Update(); //get new touch data!
            //
            // //2. check if touch type has changed. reset state if necessary
            // if (fingerInteraction.IsInteractionChanged()) //interaction has changed in current frame.
            // {
            //     //finalise the current-previous interaction.
            //     fingerInteraction.finaliseInteraction();
            //
            //     var pandelta = fingerInteraction.getPanDelta();
            //     mm.SetLookAt_Relative_OnRelease(pandelta.x, pandelta.y);
            // }
            //
            // //3. move the camera as needed base on the current touch status.
            // if (fingerInteraction.isPan())
            // {
            //     var pandelta = fingerInteraction.getPanDelta();
            //     mm.MoveFloatingLookAt_Relative(pandelta.x, pandelta.y);
            // }
            // if (fingerInteraction.isZoom())
            // {
            //     var zoomdelta = fingerInteraction.getZoomDelta();
            //     Debug.Log("2 finger not supported yet!");
            //     //mm.MoveFloatingLookAt_Relative(pandelta.x, pandelta.y);
            // }


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