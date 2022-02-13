using Lean.Gui;
using Raindrop;
using Raindrop.ServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Raindrop.UI.Movement
{
    [RequireComponent(typeof(Button))]
    class JumpToBackend : MonoBehaviour , IPointerDownHandler, IPointerUpHandler
    {
        private Button btn;


        private RaindropInstance instance { get { return ServiceLocator.Instance.Get<RaindropInstance>(); } }
        //private RaindropNetcom netcom { get { return instance.Netcom; } }
        bool Active => instance.Client.Network.Connected;

        public void OnPointerDown(PointerEventData eventData)
        {
            DoJump();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ReleaseJump();
        }

        private void Awake()
        {
            btn = GetComponent<Button>();
        }

        private void ReleaseJump()
        {
            if (Active)
            {
                instance.Movement.Jump = false;
            }
        }

        private void DoJump()
        {
            if (Active)
            {
                instance.Movement.Jump = true;
            }
        }
    }
}
