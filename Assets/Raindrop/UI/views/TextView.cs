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
    
    public void setText_DistanceInt(int distance)
    {
        if (distance < 0)
        {
            this.GetComponent<TMPro.TMP_Text>().text = "Unknown";
            return;
        }
        this.GetComponent<TMPro.TMP_Text>().text = distance.ToString() + 'm';
    }
    public string getText()
    {
        return this.GetComponent<TMPro.TMP_Text>().text;
    }

    internal void setText(float v)
    {
        throw new NotImplementedException();
    }
}
