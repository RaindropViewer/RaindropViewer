using Raindrop;
using System;
using System.Collections;
using System.Collections.Generic;
using Raindrop.ServiceLocator;
using Raindrop.Services;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

//a monobehavior that makes a toggle toggle the eula acceptance in globalSettings 
public class EulaView : MonoBehaviour
{
    private RaindropInstance instance { get { return ServiceLocator.Instance.Get<RaindropInstance>(); } }
    
    [Tooltip("the Acceptance toggle that allows us to continue past this window")]
    private Toggle EulaToggle;
    public GameObject EulaToggleGO;

    public Button closeBtn;

    private void Start()
    {

        FindAndLinkUIComponents();
    }

    //link all children UI components to the reactive events.
    private void FindAndLinkUIComponents()
    {
        EulaToggle = EulaToggleGO.GetComponent<Toggle>();
        if (EulaToggle == null)
        {
            Debug.LogWarning("eula toggle UI is not present.");
        }
        if (EulaToggle != null)
        {
            EulaToggle.onValueChanged.AsObservable().Subscribe(_ => onToggleChanged(_)); //when clicked, runs this method.
        }

        bool isAcceptedEULA = instance.GlobalSettings["EulaAccepted"];
        onToggleChanged(isAcceptedEULA);
        
        closeBtn.onClick.AddListener(closeEula);
    }

    private void closeEula()
    {
        if (instance.GlobalSettings["EulaAccepted"] == null)
            return;
        if (instance.GlobalSettings["EulaAccepted"] == false)
            return;
        
        ServiceLocator.Instance.Get<UIService>().ScreensManager.resetToInitialScreen();
    }

    private void onToggleChanged(bool isEulaAccepted)
    {
        instance.GlobalSettings["EulaAccepted"] = isEulaAccepted;
        
        if (isEulaAccepted)
        {
            closeBtn.gameObject.SetActive(true);
            return;
        }
        else
        {
            closeBtn.gameObject.SetActive(false);
        }
        
    }
     
}
