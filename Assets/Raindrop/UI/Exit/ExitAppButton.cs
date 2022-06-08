using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
