using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//currently we need main camera to have over the shoulder mode. lets add more later.
public class MainCameraMode : MonoBehaviour
{
    public Mode cameraMode = Mode.shoulder;
    public enum Mode
    {
        shoulder
    }

}
