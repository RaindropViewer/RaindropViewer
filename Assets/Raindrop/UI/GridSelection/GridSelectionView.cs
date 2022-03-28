using System;
using System.Collections.Generic;
using Raindrop;
using Raindrop.GridSelection;
using Raindrop.Presenters;
using Raindrop.ServiceLocator;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

//http://engineering.socialpoint.es/MVC-pattern-unity3d-ui.html
// https://github.com/iMemento/UMVC

public class GridSelectionView : MonoBehaviour
{
    #region dropdown
    public TMPro.TMP_Dropdown dropdown;
    //controller shall subscribe to this for user inputs.
    public event EventHandler<DropdownPresenterEventArgs> DropdownItemSelected;

    public List<string> Options;
    private GridSelectionController _controller;
    #endregion

    #region SelectedGrid URL 
    [FormerlySerializedAs("urltext")] public TextView uritext;

    
    #endregion
    
    public void Start() //take care to only link up dependencies on start.
    {
        //creates a controller. injects the presenter reference to the controller.
        _controller = new GridSelectionController(this);
    }
    
    private void Awake()
    {
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
        Options = items;
        dropdown.AddOptions(Options);
    }
    #endregion

    public int GetOptionsCount()
    {
        return dropdown.options.Count;
    }
}

public class DropdownPresenterEventArgs : System.EventArgs
{
    public int DropdownValue {get;private set;}

    public DropdownPresenterEventArgs(TMP_Dropdown dropdown)
    {
        DropdownValue = dropdown.value;
    }

}