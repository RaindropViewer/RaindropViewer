using OpenMetaverse;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityWeld.Binding;

[Binding]
public class LoginVM : MonoBehaviour, INotifyPropertyChanged
{

    private string username = "username";
    private string password = "password";

    [Binding]
    public string Username
    {
        get
        {
            return username;
        }
        set
        {
            if (username == value)
            {
                return; // No change.
            }

            username = value;

            OnPropertyChanged("username");
        }
    }
    [Binding]
    public string Password
    {
        get
        {
            return password;
        }
        set
        {
            if (password == value)
            {
                return; // No change.
            }

            password = value;

            OnPropertyChanged("password");
        }
    }

    /// <summary>
    /// Event to raise when a property's value has changed.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnPropertyChanged(string propertyName)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [Binding]
    public void OnLoginBtnClick()
    {
        Debug.Log("loggin in TODO" + username + password);
        
        GridClient Client = new GridClient();
    }


}
