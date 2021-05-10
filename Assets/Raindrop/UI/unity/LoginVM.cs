using OpenMetaverse;
using OpenMetaverse.StructuredData;
using Raindrop;
using Raindrop.Netcom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using System;
using UnityWeld.Binding;
using Settings = Raindrop.Settings;
using UnityEngine.UI;


//view(unitytext) -- presenter(this) -- controller(this?) -- model (raindropinstance singleton)

public class LoginVM : MonoBehaviour, INotifyPropertyChanged
{

    private RaindropInstance instance;
    private RaindropNetcom netcom { get { return instance.Netcom; } }

    #region references to UI elements
    public Button LoginButton;
    public Button LogoutButton;
    public InputField usernameField;
    public InputField passwordField;
    #endregion

    #region state

    private string username;
    private string password;
    private readonly string INIT_USERNAME = "username";
    private readonly string INIT_PASSWORD = "password";

    private string login_msg;

    
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

    public string Login_msg
    {
        get
        {
            return login_msg;
        }
        set
        {
            if (login_msg == value)
            {
                return; // No change.
            }

            login_msg = value;

            OnPropertyChanged(nameof(Login_msg));
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
    private object lblLoginStatus;
    private bool btnLoginEnabled;
    private bool cbTOStrue = true;
    private bool cbRememberBool;

    #endregion

    
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


    }


    private void AddNetcomEvents()
    {
        netcom.ClientLoggingIn += new EventHandler<OverrideEventArgs>(netcom_ClientLoggingIn);
        netcom.ClientLoginStatus += new EventHandler<LoginProgressEventArgs>(netcom_ClientLoginStatus);
        netcom.ClientLoggingOut += new EventHandler<OverrideEventArgs>(netcom_ClientLoggingOut);
        netcom.ClientLoggedOut += new EventHandler(netcom_ClientLoggedOut);
    }

    private void RemoveNetcomEvents()
    {
        netcom.ClientLoggingIn -= new EventHandler<OverrideEventArgs>(netcom_ClientLoggingIn);
        netcom.ClientLoginStatus -= new EventHandler<LoginProgressEventArgs>(netcom_ClientLoginStatus);
        netcom.ClientLoggingOut -= new EventHandler<OverrideEventArgs>(netcom_ClientLoggingOut);
        netcom.ClientLoggedOut -= new EventHandler(netcom_ClientLoggedOut);
    }


    public void netcom_ClientLoginStatus(object sender, LoginProgressEventArgs e)
    {
        switch (e.Status)
        {
            case LoginStatus.ConnectingToLogin:
                Login_msg = ("Connecting to login server...");
                //lblLoginStatus.ForeColor = Color.Black;
                break;

            case LoginStatus.ConnectingToSim:
                Login_msg = ("Connecting to region...");
                //lblLoginStatus.ForeColor = Color.Black;
                break;

            case LoginStatus.Redirecting:
                Login_msg = "Redirecting...";
                //lblLoginStatus.ForeColor = Color.Black;
                break;

            case LoginStatus.ReadingResponse:
                Login_msg = "Reading response...";
                //lblLoginStatus.ForeColor = Color.Black;
                break;

            case LoginStatus.Success:
                Login_msg = "Logged in as " + netcom.LoginOptions.FullName;
                //lblLoginStatus.ForeColor = Color.FromArgb(0, 128, 128, 255);
                //proLogin.Visible = false;

                //btnLogin.Text = "Logout";
                btnLoginEnabled = true;
                instance.Client.Groups.RequestCurrentGroups();
                break;

            case LoginStatus.Failed:
                //lblLoginStatus.ForeColor = Color.Red;
                if (e.FailReason == "tos")
                {
                    Login_msg = "Must agree to Terms of Service before logging in";
                    //pnlTos.Visible = true;
                    //txtTOS.Text = e.Message.Replace("\n", "\r\n");
                    btnLoginEnabled = false;
                }
                else
                {
                    Login_msg= e.Message;
                    btnLoginEnabled = true;
                }
                //proLogin.Visible = false;

                //btnLogin.Text = "Retry";
                break;
        }
    }

    public void netcom_ClientLoggedOut(object sender, EventArgs e)
    {
        Login_msg = "logged out.";
        //pnlLoginPrompt.Visible = true;
        //pnlLoggingIn.Visible = false;

        //btnLogin.Text = "Exit";
        //btnLogin.Enabled = true;
    }

    public void netcom_ClientLoggingOut(object sender, OverrideEventArgs e)
    {
        btnLoginEnabled = false;

        Login_msg = "Logging out...";
        //lblLoginStatus.ForeColor = Color.FromKnownColor(KnownColor.ControlText);

        //proLogin.Visible = true;
    }

    public void netcom_ClientLoggingIn(object sender, OverrideEventArgs e)
    {
        Login_msg = "Logging in...";
        //lblLoginStatus.ForeColor = Color.FromKnownColor(KnownColor.ControlText);

        //proLogin.Visible = true;
        //pnlLoggingIn.Visible = true;
        //pnlLoginPrompt.Visible = false;

        btnLoginEnabled = false;
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

    //public void cb_LoginCompleted()
    //{
    //    string message = "meow";
    //    showModal("Login success!", message);

    //}

    //private static void showModalLoginSuccess()
    //{


    //}

 

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

    
    public void OnLoginBtnClick()
    {
        LoginButton.interactable = false;
        //sanity 
        if (username == INIT_USERNAME || password == INIT_PASSWORD)
        {
            Debug.LogError("Either username and password is not defined!");

            LoginButton.interactable = true;
            return;
        }
        //globalRef.MainRaindropInstance.connectTo(Username,Password);

        
        BeginLogin();

    }
    
    public void OnLogoutBtnClick()
    {

        Debug.Log("logout btn");

        // Logout of simulator
        //globalRef.MainRaindropInstance.Client.Network.Logout();

        netcom.Logout();
    }

    #endregion

    #region Login functions


    private void BeginLogin()
    {
        string username = Username;

        //if (Username.SelectedIndex > 0 && cbxUsername.SelectedItem is SavedLogin)
        //{
        //    username = ((SavedLogin)cbxUsername.SelectedItem).Username;
        //}

        string[] parts = System.Text.RegularExpressions.Regex.Split(username.Trim(), @"[. ]+");

        if (parts.Length == 2)
        {
            netcom.LoginOptions.FirstName = parts[0];
            netcom.LoginOptions.LastName = parts[1];
        }
        else
        {
            netcom.LoginOptions.FirstName = username.Trim();
            netcom.LoginOptions.LastName = "Resident";
        }

        netcom.LoginOptions.Password = Password;
        netcom.LoginOptions.Channel = "Channel"; // Channel
        netcom.LoginOptions.Version = "Version"; // Version
        netcom.AgreeToTos = cbTOStrue;

        //switch (cbxLocation.SelectedIndex)
        //{
        //    case -1: //Custom
        //        netcom.LoginOptions.StartLocation = StartLocationType.Custom;
        //        netcom.LoginOptions.StartLocationCustom = cbxLocation.Text;
        //        break;

        //    case 0: //Home
        //        netcom.LoginOptions.StartLocation = StartLocationType.Home;
        //        break;

        //    case 1: //Last
        //        netcom.LoginOptions.StartLocation = StartLocationType.Last;
        //        break;
        //}

        //if (cbxGrid.SelectedIndex == cbxGrid.Items.Count - 1) // custom login uri
        //{
        //    if (txtCustomLoginUri.TextLength == 0 || txtCustomLoginUri.Text.Trim().Length == 0)
        //    {
        //        MessageBox.Show("You must specify the Login Uri to connect to a custom grid.", Properties.Resources.ProgramName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        return;
        //    }

        //    netcom.LoginOptions.Grid = new Grid("custom", "Custom", txtCustomLoginUri.Text);
        //    netcom.LoginOptions.GridCustomLoginUri = txtCustomLoginUri.Text;
        //}
        //else
        //{
            //netcom.LoginOptions.Grid = cbxGrid.SelectedItem as Grid;
        //}

        if (netcom.LoginOptions.Grid.Platform != "SecondLife")
        {
            instance.Client.Settings.MULTIPLE_SIMS = true;
            instance.Client.Settings.HTTP_INVENTORY = !instance.GlobalSettings["disable_http_inventory"];
        }
        else
        {
            // UDP inventory is deprecated as of 2015-03-30 and no longer supported.
            // https://community.secondlife.com/t5/Second-Life-Server/Deploy-for-the-week-of-2015-03-30/td-p/2919194
            instance.Client.Settings.HTTP_INVENTORY = true;
        }

        netcom.Login();
        SaveConfig();
    }


    private void SaveConfig()
    {
        Settings s = instance.GlobalSettings;
        SavedLogin sl = new SavedLogin();

        string username = Username;

        //if (cbxUsername.SelectedIndex > 0 && cbxUsername.SelectedItem is SavedLogin)
        //{
        //    username = ((SavedLogin)cbxUsername.SelectedItem).Username;
        //}

        //if (cbxGrid.SelectedIndex == cbxGrid.Items.Count - 1) // custom login uri
        //{
        //    sl.GridID = "custom_login_uri";
        //    sl.CustomURI = txtCustomLoginUri.Text;
        //}
        //else
        //{
        //    sl.GridID = (cbxGrid.SelectedItem as Grid).ID;
        //    sl.CustomURI = string.Empty;
        //}

        string savedLoginsKey = string.Format("{0}%{1}", username, sl.GridID);

        if (!(s["saved_logins"] is OSDMap))
        {
            s["saved_logins"] = new OSDMap();
        }

        if (cbRememberBool)
        {

            sl.Username = s["username"] = username;

            if (LoginOptions.IsPasswordMD5(Password))
            {
                sl.Password = Password;
                s["password"] = Password;
            }
            else
            {
                sl.Password = Utils.MD5(Password);
                s["password"] = Utils.MD5(Password);
            }
            //if (cbxLocation.SelectedIndex == -1)
            //{
            //    sl.CustomStartLocation = cbxLocation.Text;
            //}
            //else
            //{
            //    sl.CustomStartLocation = string.Empty;
            //}
            //sl.StartLocationType = cbxLocation.SelectedIndex;
            ((OSDMap)s["saved_logins"])[savedLoginsKey] = sl.ToOSD();
        }
        else if (((OSDMap)s["saved_logins"]).ContainsKey(savedLoginsKey))
        {
            ((OSDMap)s["saved_logins"]).Remove(savedLoginsKey);
        }

        //s["login_location_type"] = OSD.FromInteger(cbxLocation.SelectedIndex);
        //s["login_location"] = OSD.FromString(cbxLocation.Text);

        //s["login_grid"] = OSD.FromInteger(cbxGrid.SelectedIndex);
        //s["login_uri"] = OSD.FromString(txtCustomLoginUri.Text);
        //s["remember_login"] = cbRemember.Checked;
    }


    #endregion








    public class SavedLogin
    {
        public string Username;
        public string Password;
        public string GridID;
        public string CustomURI;
        public int StartLocationType;
        public string CustomStartLocation;

        public OSDMap ToOSD()
        {
            OSDMap ret = new OSDMap(4);
            ret["username"] = Username;
            ret["password"] = Password;
            ret["grid"] = GridID;
            ret["custom_url"] = CustomURI;
            ret["location_type"] = StartLocationType;
            ret["custom_location"] = CustomStartLocation;
            return ret;
        }

        public static SavedLogin FromOSD(OSD data)
        {
            if (!(data is OSDMap)) return null;
            OSDMap map = (OSDMap)data;
            SavedLogin ret = new SavedLogin();
            ret.Username = map["username"];
            ret.Password = map["password"];
            ret.GridID = map["grid"];
            ret.CustomURI = map["custom_url"];
            if (map.ContainsKey("location_type"))
            {
                ret.StartLocationType = map["location_type"];
            }
            else
            {
                ret.StartLocationType = 1;
            }
            ret.CustomStartLocation = map["custom_location"];
            return ret;
        }

        public override string ToString()
        {
            RaindropInstance instance = RaindropInstance.GlobalInstance;
            string gridName;
            if (GridID == "custom_login_uri")
            {
                gridName = "Custom Login URI";
            }
            else if (instance.GridManger.KeyExists(GridID))
            {
                gridName = instance.GridManger[GridID].Name;
            }
            else
            {
                gridName = GridID;
            }
            return string.Format("{0} -- {1}", Username, gridName);
        }

    }
}
