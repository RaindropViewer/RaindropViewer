using System.Collections;
using System.Collections.Generic;
using OpenMetaverse.Stats;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonTriggerModal : MonoBehaviour
{
    private Button button;
    // Start is called before the first frame update
    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonDown);
    }

    private void OnButtonDown()
    {
        throw new System.NotImplementedException();
    }
}
