using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TMPro.TMP_Text))]
public class TextView : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }


    public void setText(string text)
    {
        this.GetComponent<TMPro.TMP_Text>().text = text;
    }
}
