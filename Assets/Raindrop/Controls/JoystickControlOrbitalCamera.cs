using Cinemachine;
using Lean.Gui;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class JoystickControlOrbitalCamera : MonoBehaviour
{
    public float sensX = 0.4f;
    public float sensY = 0.1f;
    public LeanJoystick js;

    public Cinemachine.CinemachineFreeLook freelook;
    private float JoyThreshold = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        js = this.GetComponent<LeanJoystick>();
        if (js == null)
        {
            Debug.Log("bad hook up!");
        }

        
    }

    // Update is called once per frame
    void Update()
    {
        var joyinput = js.ScaledValue;
        if (joyinput.magnitude < JoyThreshold)
        {
            return;
        }

        freelook.m_XAxis.Value += joyinput.x * sensX * Time.deltaTime; 
        freelook.m_YAxis.Value += joyinput.y * sensY * Time.deltaTime; 
    }
}
