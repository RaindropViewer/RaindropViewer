using Raindrop;
using Raindrop.Presenters;
using Raindrop.ServiceLocator;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//assign the target for the cinemachine freelook camera to look at.
public class RaindropCinemachineAssignLookAtAvatar : MonoBehaviour
{
    private RaindropInstance instance { get { return ServiceLocator.Instance.Get<RaindropInstance>(); } }
    //private RaindropNetcom netcom { get { return instance.Netcom; } }
    bool Active => instance.Client.Network.Connected;

    public AgentPresenter agents;
    public Cinemachine.CinemachineFreeLook cinemachine;
    private void Update()
    {
        TrySetTarget();
    }
    public void TrySetTarget()
    {
        if (Active)
        {
            if (agents.agentReference != null) //todo: ok we should do a event driven way instead
            {
                SetCinemachineTarget(agents.agentReference);
            }
        }

    }

    private void SetCinemachineTarget(GameObject agentReference)
    {
        cinemachine.LookAt = agentReference.transform;
        cinemachine.Follow = agentReference.transform;
    }
}
