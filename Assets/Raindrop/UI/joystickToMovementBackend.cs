using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean;
using Raindrop;
using OpenMetaverse;
using Raindrop.Netcom;
using Lean.Gui;

//make the joystick position drive the user's movement (u,d,l,r) :)
public class joystickToMovementBackend : MonoBehaviour
{
    public LeanJoystick variableJoystick;
    public GameObject theJoystickInScene;
    private RaindropInstance instance { get { return RaindropInstance.GlobalInstance; } }
    private RaindropNetcom netcom { get { return instance.Netcom; } }
    private GridClient client { get { return instance.Client; } }

    bool Active => instance.Client.Network.Connected;

    void Awake()
    {
        if (theJoystickInScene.GetComponent<LeanJoystick>() == null)
        {
            Debug.LogError("the joystick object is not found!");
        } 
        variableJoystick = theJoystickInScene.GetComponent<LeanJoystick>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (! Active)
        //{
        //    return;
        //}

        ////do not flood! no setting unless changed.
        //float vert = variableJoystick.ScaledValue.y;
        //float horz = variableJoystick.ScaledValue.x;

        //float thresh = 0.7f;
        //if (vert > thresh)
        //{
        //    instance.Movement.MovingBackward = false;
        //    instance.Movement.MovingForward = true;

        //}
        //else if (vert < -thresh)
        //{
        //    instance.Movement.MovingBackward = true;
        //    instance.Movement.MovingForward = false;

        //}
        //else
        //{
        //    instance.Movement.MovingBackward = false;
        //    instance.Movement.MovingForward = false;

        //}

        //if (horz > thresh)
        //{
        //    instance.Movement.TurningLeft = false;
        //    instance.Movement.TurningRight = true;

        //}
        //else if (horz < -thresh)
        //{
        //    instance.Movement.TurningLeft = true;
        //    instance.Movement.TurningRight = false;

        //}
        //else
        //{
        //    instance.Movement.TurningLeft = false;
        //    instance.Movement.TurningRight = false;

        //}

    }
}
