using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean;
using Raindrop;
using OpenMetaverse;
using Raindrop.Netcom;
using Lean.Gui;
using Raindrop.ServiceLocator;
using Vector2 = UnityEngine.Vector2;

//make the joystick position drive the user's movement (u,d,l,r) :)
public class joystickToMovementBackend : MonoBehaviour
{
    public LeanJoystick variableJoystick;
    public GameObject theJoystickInScene;
    public float joyThresh = 0.7f;
    private RaindropInstance instance { get { return ServiceLocator.Instance.Get<RaindropInstance>(); } }
    private RaindropNetcom netcom { get { return instance.Netcom; } }
    private GridClient client { get { return instance.Client; } }

    bool Active => instance.Client.Network.Connected;

    void Start()
    {
        if (theJoystickInScene.GetComponent<LeanJoystick>() == null)
        {
            Debug.LogError("the joystick object is not found!");
        } 
        variableJoystick = theJoystickInScene.GetComponent<LeanJoystick>();

        //set zero.
        OnJoyUp();
        
        variableJoystick.OnUp.AddListener(OnJoyUp);
        variableJoystick.OnSet.AddListener(OnJoySet);
    }

    private void OnJoySet(Vector2 arg0)
    {
        setWASDMovements(arg0);
        
        
    }

    private void setWASDMovements(Vector2 arg0)
    {
        float vert = arg0.y;
        float horz = arg0.x;
        
        if (vert > joyThresh)
        {
            instance.Movement.setForward();
        }
        if (vert < -joyThresh)
        {
            instance.Movement.setBackward();
        }

        if (horz > joyThresh)
        {
            instance.Movement.setRightward();
        }
        if (horz < -joyThresh)
        {
            instance.Movement.setLeftward();
        }

        
    }

    // no more sideways movment.
    private void OnJoyUp()
    {
        if (! Active)
        {
            return;
        }

        setWASDMovementsZero();

    }

    private void setWASDMovementsZero()
    {
        instance.Movement.stopMovementAndSendUpdate();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
