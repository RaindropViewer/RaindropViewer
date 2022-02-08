using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

// sets the color of the view.
// 2 colors for: selected, not-selected
[RequireComponent(typeof(Image))]
public class TabButton : MonoBehaviour
{
    [HideInInspector]
    public Image background;
    
    public static Color tabIdleColor;
    public static Color tabSelectedColor;

    void Start()
    {
        background = GetComponent<Image>();
    }
    
    public void Select()
    {
        background.color = tabSelectedColor;
    }

    public void Deselect()
    {
        background.color = tabIdleColor;
    }


}