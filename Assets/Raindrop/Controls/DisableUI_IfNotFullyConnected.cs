using Lean.Gui;
using Raindrop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenMetaverse;
using Plugins.CommonDependencies;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Raindrop.UI.Movement
{
    class DisableUI_IfNotFullyConnected : MonoBehaviour
    {
        private Selectable btn;

        private RaindropInstance instance { get { return RaindropInstance.GlobalInstance; } }
        //can the component run?
        bool CanSendCommandsToSimulator => instance.Client.Network.Connected && 
                                  !instance.Netcom.IsTeleporting &&
                                  instance.Netcom.IsLoggedIn && 
                                  instance.Client.Network.CurrentSim != null && //does this null check prevent the next accessor failure?
                                  instance.Client.Network.CurrentSim.HandshakeComplete;

        private void Awake()
        {
            btn = GetComponent<Button>();
        }

        private void Start()
        {
            instance.Netcom.TeleportStatusChanged += NetcomOnTeleportStatusChanged;
        }

        private void OnEnable()
        {
            try
            {
                if (CanSendCommandsToSimulator) //could throw null ref exc
                {
                    SetInteractable(true);
                }
                else
                {
                    SetInteractable(false);
                }
            }
            catch (Exception e)
            {
                SetInteractable(false);
            }
        }

        private void SetInteractable(bool b)
        {
            btn.interactable = b;
        }

        private void NetcomOnTeleportStatusChanged(object sender, TeleportEventArgs e)
        {
            if (e.Status == TeleportStatus.Finished)
            {
                SetInteractable(true);
            }
            else
            {
                SetInteractable(false);
            }
        }

    }
}
