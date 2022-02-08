using Raindrop;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraIdentifier : MonoBehaviour
{
    public CameraType type;


    public enum CameraType
    {
        Main,
        Minimap
    }
}
