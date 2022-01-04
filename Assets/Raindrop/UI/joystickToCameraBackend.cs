using System.Collections;
using System.Collections.Generic;
using Lean.Gui;
using Raindrop;
using Raindrop.ServiceLocator;
using UnityEngine;

// currently, this turns the character. in the future, we will use  this as input the the 3rd person camera controller.

[RequireComponent(typeof(LeanJoystick))]
public class joystickToCameraBackend : MonoBehaviour
{
    LeanJoystick joy;
    private RaindropInstance instance;
    public float thresh = 0.7f;
 
    void Start()
    {
         joy = this.gameObject.GetComponent<LeanJoystick>();
         joy.OnSet.AddListener(OnJoySet);

         instance = ServiceLocator.Instance.Get<RaindropInstance>();
    }

    private void OnJoySet(Vector2 arg0)
    {
        
        float vert = arg0.y;
        float horz = arg0.x;

        int horz_clamp = (Mathf.Abs(horz) > thresh) ? 1 :
                (Mathf.Abs(horz) < thresh) ? -1 :0;

        if (horz_clamp == 0){
            instance.Movement.SetTurningStop();
        } else if(horz_clamp == 1){
            instance.Movement.SetTurningRight();
        }
        else
        {
            instance.Movement.SetTurningLeft();
        }

    }
    
}
