using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class nameTagPresenter : MonoBehaviour
{
    public TextMesh tm;
    
    
    
    public void setName(string aviname)
    {
        tm.text = aviname;
    }
}
