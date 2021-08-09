using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// An implementation of view. Attach to GO that has TMP_text.
[RequireComponent(typeof(TMPro.TMP_Text))]
public class TextView : MonoBehaviour
{

    public void setText(string text)
    {
        this.GetComponent<TMPro.TMP_Text>().text = text;
    }

    internal void setText(float v)
    {
        throw new NotImplementedException();
    }
}
