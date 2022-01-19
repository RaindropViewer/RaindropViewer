using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoggerInit : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        OpenMetaverse.Logger.Log("First log. Logger.Log is working.", OpenMetaverse.Helpers.LogLevel.Info);

    }
}
     
