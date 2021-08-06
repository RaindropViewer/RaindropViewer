using Raindrop;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

//a monobehavior that makes a toggle toggle the eula acceptance in globalSettings 
public class EulaModalPresenter : MonoBehaviour
{
    private RaindropInstance instance { get { return ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>(); } }
    //public string nameOfEulaToggleGO;
    private Toggle EulaToggle;
    public GameObject EulaToggleGO;

    private void Start()
    {
        //var _ = gameObject.transform.Find(nameOfEulaToggleGO); 
        // <EulaToggle>();
        EulaToggle = EulaToggleGO.GetComponent<Toggle>();

        bool is_accepted_in_settings = instance.GlobalSettings["Accept_RaindropEula"];

        if (is_accepted_in_settings)
        {
            EulaToggle.isOn = true;
            return;
        }

        if (EulaToggle != null)
        {
            EulaToggle.onValueChanged.AsObservable().Subscribe(_ => onToggleChanged(_)); //when clicked, runs this method.

        } 

    }

    private void onToggleChanged(bool isEulaAccepted)
    {
        //if (isEulaAccepted)
        //{
            instance.GlobalSettings["Accept_RaindropEula"] = isEulaAccepted;
        //}
    }
     
}
