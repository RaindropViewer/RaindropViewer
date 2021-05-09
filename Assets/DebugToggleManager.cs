using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//attached to panel UI. simply toggles UI panels.
public class DebugToggleManager : MonoBehaviour
{
    public Stack<GameObject> panelStack;

    public GameObject loginPanel;
    
    public GameObject gamePanel;

    public GameObject defaultOnPanel;

    private void Awake()
    {
        panelStack = new Stack<GameObject>();

        defaultOnPanel = loginPanel;
        addPaneltoView(defaultOnPanel);
        removeTopPanelFromView();

        gamePanel.SetActive(false);
    }

    public void togglePanel(GameObject panel)
    {
        if (panel.activeInHierarchy == false)
        {
            addPaneltoView(panel);
        } else
        {
            removeTopPanelFromView();
        }
    }

    
    public void removeTopPanelFromView()
    {
        GameObject toppanel;
        if (panelStack.Count != 0)
        {
            toppanel = panelStack.Peek();
            panelStack.Pop();
            toppanel.SetActive(false);
        }
    }

    public void addPaneltoView(GameObject defaultOnPanel)
    {
        panelStack.Push(defaultOnPanel);
     


        defaultOnPanel.SetActive(true);
    }
}
