using OpenMetaverse;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityWeld.Binding;

[Binding]
public class LoginVM : MonoBehaviour, INotifyPropertyChanged
{

    #region state

    private string username;
    private string password;
    private enum State
    {
        init,
        connected,
        disconnected,
    }
    private State state = State.init;
    private readonly string INIT_USERNAME = "username";
    private readonly string INIT_PASSWORD = "password";

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

            OnPropertyChanged(nameof(Username));
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

            OnPropertyChanged(nameof(Password));
        }
    }

    private string[] options = new string[]
    {
        "Options-TODO",
        "Option-B",
        "Option-C",
        "Option-F"
    };

    private string selectedItem = "Options-TODO";

    #endregion

    [Binding]
    public string SelectedItem
    {
        get
        {
            return selectedItem;
        }
        set
        {
            if (selectedItem == value)
            {
                return; // No change.
            }

            selectedItem = value;

            OnPropertyChanged(nameof(SelectedItem));
        }
    }


    public string[] Options
    {
        get
        {
            return options;
        }
    }




    #region behavior
    /// <summary>
    /// Event to raise when a property's value has changed.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    // Use this for initialization
    void Start()
    {
        initialise();
        Global.MainRaindropInstance.LoginCompleted += cb_LoginCompleted;
        Global.MainRaindropInstance.LoginFailed += cb_LoginFailed;

    }

    private void cb_LoginFailed()
    {
        string message = "meow";
        showModal("Login failed.", message);
    }

    private void showModal(string v, string message)
    {

        Debug.Log("MODAL:\n" + v + "\n" +message);
    }

    public void cb_LoginCompleted()
    {
        string message = "meow";
        showModal("Login success!", message);

    }

    private static void showModalLoginSuccess()
    {


    }

 

    private void initialise()
    {
        //reset user and pw fields
        username = INIT_USERNAME;
        password = INIT_PASSWORD;

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

        //sanity 
        if (username == INIT_USERNAME || password == INIT_PASSWORD)
        {
            Debug.LogError("loggin button but username and password are not defined!");
            return;
        }
        Global.MainRaindropInstance.connectTo(Username,Password);
        
    }
    
    [Binding]
    public void OnLogoutBtnClick()
    {
        Debug.Log("logout btn");

        // Logout of simulator
        Global.MainRaindropInstance.Client.Network.Logout();
    }

    #endregion
}
