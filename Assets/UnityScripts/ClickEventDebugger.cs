using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class ClickEventDebugger : MonoBehaviour
{
    [FormerlySerializedAs("text")] public TMPro.TMP_Text TMP_text;
    
    void LateUpdate()
    {
        EventSystem eventSystem = EventSystem.current;

        var obj = eventSystem.currentSelectedGameObject;
        // if (obj is null) // this does NOT catch 'obj is missing reference'; only null.
        // {
        //     return;
        // }
        if (!obj) //this catches 'missing reference '
        {
            return;
        }
        
        TMP_text.text = obj.name;
    }

}
