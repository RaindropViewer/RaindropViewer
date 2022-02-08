using System;
using System.Collections.Generic;
using Raindrop;
using Raindrop.ServiceLocator;
using TMPro;
using UnityEngine;
// using UnityEngine.UI;

//http://engineering.socialpoint.es/MVC-pattern-unity3d-ui.html
// https://github.com/iMemento/UMVC

public class DropdownPresenterEventArgs : System.EventArgs
{
    public int DropdownValue {get;private set;}

    public DropdownPresenterEventArgs(TMP_Dropdown dropdown)
    {
        DropdownValue = dropdown.value;
    }

}

[RequireComponent(typeof(TMP_Dropdown))]
public class DropdownViewPresenter : MonoBehaviour
{
    private TMPro.TMP_Dropdown dropdown;
    //controller to subscribe to this for user inputs.
    public event EventHandler<DropdownPresenterEventArgs> DropdownItemSelected;

    private void Awake()
    {
        //1. get DD
        dropdown = GetComponent<TMPro.TMP_Dropdown>();

        //2 setup outbound (user input)
        dropdown.onValueChanged.AddListener(
            delegate { OnValueChanged(dropdown); }
            );
        //3 setup inbound (computer notify user; output to user)
    }

    private void OnValueChanged(TMP_Dropdown dd)
    {
        DropdownItemSelected(this, new DropdownPresenterEventArgs(dd));
    }

    #region UpdateDropdownOptions

    public void ClearAndSetOptions(List<string> items)
    {
        dropdown.ClearOptions();
        dropdown.AddOptions(items);
    }
    
    public void AppendOption(string item)
    {
        dropdown.AddOptions(new List<string>() {
            item
        });
 
    }
    #endregion

    public int GetOptionsCount()
    {
        return dropdown.options.Count;
    }
}
