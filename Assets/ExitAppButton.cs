using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ExitAppButton : MonoBehaviour
{

    private Button button;
    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnAppExitRequested);
    }

    private void OnAppExitRequested()
    {
        if (Application.isEditor)
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
        
        Application.Quit();
    }
}
