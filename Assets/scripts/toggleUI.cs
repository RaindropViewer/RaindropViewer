using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class toggleUI : MonoBehaviour
{
    private bool state;

    [SerializeField] private Button button;
    [SerializeField] public GameObject UI;

    private void Awake()
    { 
    }

    public void toggle()
    {
        if (UI.activeInHierarchy)
        {
            UI.SetActive(false);
        } else
        {
            UI.SetActive(true);
        }
    }

}
