using Raindrop;
using System;
using OpenMetaverse;
using Plugins.CommonDependencies;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Raindrop.UI.Movement
{
    [RequireComponent(typeof(Button))]
    class JumpButton_ToBackend : MonoBehaviour , IPointerDownHandler, IPointerUpHandler
    {
        public Button btn;

        private RaindropInstance instance { get { return ServiceLocator.Instance.Get<RaindropInstance>(); } }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (btn.interactable)
            {
                DoJump();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (btn.interactable)
            {
                ReleaseJump();
            }
        }

        private void ReleaseJump()
        {
            instance.Movement.Jump = false;
        }

        private void DoJump()
        {
            instance.Movement.Jump = true;
        }
    }
}
