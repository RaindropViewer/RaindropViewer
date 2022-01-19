using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class toggleUI : MonoBehaviour
{
    [SerializeField] private Toggle toggle;
    [SerializeField] public GameObject targetToToggle;

    private void Awake()
    {
        if (targetToToggle == null)
        {
            return;
        }

        if (toggle == null)
        {
            toggle = this.GetComponent<Toggle>();
        }
        
        toggle.onValueChanged.AddListener(_ => targetToToggle.SetActive(_));
    }
}
