using OpenMetaverse;
using OpenMetaverse.StructuredData;
using Raindrop;
using Raindrop.Netcom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using System;
using Settings = Raindrop.Settings;
using UnityEngine.UI;
using UniRx;
using TMPro;
using static Raindrop.LoginUtils;
using ServiceLocator;


//view(unitytext) -- presenter(this) -- controller(this?) -- model (raindropinstance singleton)
namespace Raindrop.Presenters
{
    //the main logic(presenter) for the login=panel.
    //it saves and loads (on init) the last known user.
    //it handles loggin in, inaddition to showing login messages 
    //We are to revert to this UI whenever the user is disconnected. (on disconnect)

    //custom URL is not supported.
    //grid selection is supported.

    //not support custom input into dropboxes yet.

    public class LoginPresenter : MonoBehaviour
    {
        private RaindropInstance instance { get { return ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>(); } }
        private RaindropNetcom netcom { get { return instance.Netcom; } }
        private UIService uimanager;


        #region UI elements - the 'view' in MVP
        public Button LoginButton;
        public TMP_InputField usernameField;
        public TMP_InputField passwordField;
        public Toggle TOSCheckbox;
        public Toggle RememberCheckbox;

        public TMP_Dropdown locationDropdown;
        private LoginLocationDropdown loginLocationDropdown; //wraps the above
        #endregion

        #region internal representations 

        private readonly string INIT_USERNAME = "username";
        private readonly string INIT_PASSWORD = "password";
        private Settings s;

        public string Username
        {
            get; set;
        }

        public string Password
        {
            get; set;
        }

        public ReactiveProperty<string> Login_msg { get; private set; }
        private ReactiveProperty<bool> btnLoginEnabled;

        public string uninitialised = "(unknown)";

        private object lblLoginStatus;

        private bool cbTOStrue = true;
        private bool cbRememberBool;

        #endregion

        #region init
         
        void Start()
        {
            //0 create dropdown for login location 
            loginLocationDropdown = new LoginLocationDropdown(locationDropdown);

            //1 load default UI fields.
            initialiseFields();

            //2 load various loginInformation from the settings.
            InitializeConfig();
             

            //GridDropdownGO = this.gameObject.GetComponent<GenericDropdown>().gameObject;
            //genericDropdown = GridDropdownGO.GetComponent<GenericDropdown>();
            //GridDropdownView.DropdownSelectionChanged += GenericDropdown_DropdownSelectionChanged;

            //3 hookup reactive UIs.
            LoginButton.onClick.AsObservable().Subscribe(_ => OnLoginBtnClick()); //when clicked, runs this method.

            btnLoginEnabled = new ReactiveProperty<bool>(true);
            btnLoginEnabled.AsObservable().Subscribe(_ => LoginButton.gameObject.SetActive(_)); //update the login button availabilty according to this boolean.
            
            usernameField.onValueChanged.AsObservable().Subscribe(_ => Username = _); //change username property.
            passwordField.onValueChanged.AsObservable().Subscribe(_ => Password = _);

            RememberCheckbox.OnValueChangedAsObservable().Subscribe(_ => cbRememberBool = _); //when toggle checkbox, set boolean to the same value as the toggle-state
            TOSCheckbox.OnValueChangedAsObservable().Subscribe(_ => cbTOStrue = _); //when toggle checkbox, set boolean to the same value as the toggle-state

            Login_msg.AsObservable().Where(_ => ! _.Equals(uninitialised)).Subscribe(_ => UpdateModalText(_)); //no bug?

            //gridDropdown.OnValueChangedAsObservable().Subscribe(_ => gridSelectedItem = _);
            //customURLCheckbox.onValueChanged.AsObservable().Subscribe(_ => cbCustomURL = _); //change username property.

            //DropdownMenuWithEntry = UserDropdownMenu.GetComponent<DropdownMenuWithEntry>();

            //4subscribe to events.
            AddNetcomEvents();

            //get uimanager service
            uimanager = ServiceLocator.ServiceLocator.Instance.Get<UIService>();

        }

        private void UpdateModalText(string _)
        {
            uimanager.modalManager.setVisibleLoggingInModal(_);
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
                    Login_msg.GetType().GetProperty("Value").SetValue(Login_msg, "Connecting to login server...");
                    uimanager.modalManager.showSimpleModalBoxWithActionBtn("Logging in process...", Login_msg.Value, "close modal");
                    //lblLoginStatus.ForeColor = Color.Black;
                    break;

                case LoginStatus.ConnectingToSim:
                    Login_msg.Value = ("Connecting to region...");
                    uimanager.modalManager.showSimpleModalBoxWithActionBtn("Logging in process...", Login_msg.Value, "close modal");
                    //lblLoginStatus.ForeColor = Color.Black;
                    break;

                case LoginStatus.Redirecting:
                    Login_msg.Value = "Redirecting...";
                    uimanager.modalManager.showSimpleModalBoxWithActionBtn("Logging in process...", Login_msg.Value, "close modal");
                    //lblLoginStatus.ForeColor = Color.Black;
                    break;

                case LoginStatus.ReadingResponse:
                    Login_msg.Value = "Reading response...";
                    uimanager.modalManager.showSimpleModalBoxWithActionBtn("Logging in process...", Login_msg.Value, "close modal");
                    //lblLoginStatus.ForeColor = Color.Black;
                    break;

                case LoginStatus.Success:
                    Login_msg.Value = "Logged in as " + netcom.LoginOptions.FullName;
                    //lblLoginStatus.ForeColor = Color.FromArgb(0, 128, 128, 255);
                    //proLogin.Visible = false;

                    //btnLogin.Text = "Logout";
                    btnLoginEnabled.Value = false;
                    instance.Client.Groups.RequestCurrentGroups();

                    uimanager.modalManager.showSimpleModalBoxWithActionBtn("Logging in process...","Logged in !", "yay!");
                    uimanager.canvasManager.pushCanvasWithOrWithoutPop("Game", true); //refactor needed: better way to schedule push and pop as we are facing some issues here.
                    //instance.UI.canvasManager.popCanvas();
                    LoginButton.interactable = true;
                    break;

                case LoginStatus.Failed: 
                    //lblLoginStatus.ForeColor = Color.Red;
                    if (e.FailReason == "tos")
                    {
                        Login_msg.Value = "Must agree to Terms of Service before logging in";
                        uimanager.modalManager.showSimpleModalBoxWithActionBtn("Logging in failed",Login_msg.Value, "ok");
                        //pnlTos.Visible = true;
                        //txtTOS.Text = e.Message.Replace("\n", "\r\n");
                        btnLoginEnabled.Value = true;
                        LoginButton.interactable = true;
                    }
                    else
                    {
                        Login_msg.Value = e.Message;
                        uimanager.modalManager.showSimpleModalBoxWithActionBtn("Logging in failed", Login_msg.Value, "ok");
                        btnLoginEnabled.Value = true;
                        LoginButton.interactable = true;
                    }
                    //proLogin.Visible = false;

                    //btnLogin.Text = "Retry";
                    break;
            }
        }

        public void netcom_ClientLoggedOut(object sender, EventArgs e)
        {
            Login_msg.Value = "logged out.";
            uimanager.modalManager.showSimpleModalBoxWithActionBtn("Login status", Login_msg.Value, "ok");
            //pnlLoginPrompt.Visible = true;
            //pnlLoggingIn.Visible = false;

            //btnLogin.Text = "Exit";
            //btnLogin.Enabled = true;
        }

        public void netcom_ClientLoggingOut(object sender, OverrideEventArgs e)
        {
            btnLoginEnabled.Value = false;

            Login_msg.Value = "Logging out...";
            uimanager.modalManager.showSimpleModalBoxWithActionBtn("Login status", Login_msg.Value, "ok");
            //lblLoginStatus.ForeColor = Color.FromKnownColor(KnownColor.ControlText);

            //proLogin.Visible = true;
        }

        public void netcom_ClientLoggingIn(object sender, OverrideEventArgs e)
        {
            Login_msg.Value = "Logging in...";
            uimanager.modalManager.showSimpleModalBoxWithActionBtn("Login status", Login_msg.Value, "ok");
            //lblLoginStatus.ForeColor = Color.FromKnownColor(KnownColor.ControlText);

            //proLogin.Visible = true;
            //pnlLoggingIn.Visible = true;
            //pnlLoginPrompt.Visible = false;

            btnLoginEnabled.Value = false;
        }
          


        private void initialiseFields()
        {
            //reset user and pw fields
            Username = INIT_USERNAME;
            Password = INIT_PASSWORD;

            //login status message for the modal 
            Login_msg = new ReactiveProperty<string>(uninitialised);
            
        }

        //make sure you have done the needful before this, such as reading the grid xml file.
        private void InitializeConfig()
        {
            Settings s = instance.GlobalSettings;
            RememberCheckbox.isOn = LoginUtils.getRememberFromSettings(s);

            string savedUsername = s["username"];
            usernameField.text = (savedUsername);
            Username = (savedUsername);

            //try to get saved username
            try
            {
                if (s["saved_logins"] is OSDMap)
                {
                    OSDMap savedLogins = (OSDMap)s["saved_logins"];
                    foreach (string loginKey in savedLogins.Keys)
                    {
                        SavedLogin sl = SavedLogin.FromOSD(savedLogins[loginKey]);
                        Debug.Log("username cache: " + sl.ToString());
                        //cbxUsername.Items.Add(sl);
                        //usernameDropdownMenuWithEntry.Items.Add(sl);
                    }
                }
            }
            catch
            {
                Debug.LogError("username cache catch block!");
                //cbxUsername.Items.Clear();
                //cbxUsername.Text = string.Empty;
            }

            //cbxUsername.SelectedIndex = 0;

            // Fill in saved password or use one specified on the command line
            var pass = s["password"].AsString();
            passwordField.text = pass;
            Password = (pass);
            Debug.Log("password cache (MD5-ed): " + passwordField.text);

            // Setup login location either from the last used  //home = 0; last = 1
            //    Use last location as default
            if (s["login_location_type"].Type == OSDType.Unknown) //if not in cache file?
            {
                loginLocationDropdown.select(1);
                s["login_location_type"] = OSD.FromInteger(1); //default set to last
            }
            else
            {
                //not supported: custom login locations. onyl support home or last. 

                int loginLocationId = s["login_location_type"].AsInteger();
                loginLocationDropdown.select(loginLocationId);
                //loginLocationDropdown.select(loginLocationId);
                //locationDropdown.Text = s["login_location"].AsString();
            } 
             
            //txtCustomLoginUri.Text = s["login_uri"].AsString();
             
        }


        public void OnLoginBtnClick()
        {
            LoginButton.interactable = false;
            //instance.MediaManager.PlayUISound(UISounds.Click);
            //sanity 
            if (Username == INIT_USERNAME || Password == INIT_PASSWORD)
            {
                Debug.LogError("Either username and password is not defined!");

                LoginButton.interactable = true;
                return;
            }

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

            string[] parts = System.Text.RegularExpressions.Regex.Split(username.Trim(), @"[. ]+");

            _ = netcom;

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

            //startlocation parsing


            switch (locationDropdown.value)
            {
                case 0: //Custom
                    netcom.LoginOptions.StartLocation = StartLocationType.Custom;
                    netcom.LoginOptions.StartLocationCustom = "placeholder text";
                    break;

                case 1: //Home
                    netcom.LoginOptions.StartLocation = StartLocationType.Home;
                    break;

                case 2: //Last
                    netcom.LoginOptions.StartLocation = StartLocationType.Last;
                    break;
            }

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
            //netcom.LoginOptions.Grid = instance.GridManger.Grids[gridSelectedItem];
            //}

            //placeholder to select SL as grid.
            netcom.LoginOptions.Grid = instance.GridManger.Grids[0]; //0 means sl i think
             

            //note: setting this ridiculously low yields log: "< >: Login status: Failed: A task was canceled."

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

            var temp = netcom.LoginOptions;

            netcom.Login();
            SaveConfig(netcom.LoginOptions, instance.GlobalSettings, cbRememberBool);
        }


        #endregion

        #region Settings

        // this function appends (and saves) content of loginoptions to globalsettings file.
        // it is called when the user clicks the login button.
        public void SaveConfig(LoginOptions loginoptions, Raindrop.Settings globalSettings, bool isSaveCredentials)
        {
            Raindrop.Settings s = globalSettings;
            SavedLogin sl = new SavedLogin();

            string username = loginoptions.FirstName + " " + loginoptions.LastName;
            string Password = loginoptions.Password;

            //checks if the username selected is a dropdown option. this means you use the username from the settings instead of the text box.
            //if (cbxUsername.SelectedIndex > 0 && cbxUsername.SelectedItem is SavedLogin)
            //{
            //    username = ((SavedLogin)cbxUsername.SelectedItem).Username;
            //}

            if (loginoptions.GridLoginUri != string.Empty) // custom login uri
            {
                sl.GridID = "custom_login_uri";
                sl.CustomURI = loginoptions.GridLoginUri;
            }
            else
            {
                sl.GridID = loginoptions.Grid.ID; //GridID;//netcom.LoginOptions.Grid.ID; //(GridDropdownGO.SelectedItem as Grid).ID;
                sl.CustomURI = string.Empty;
            }

            string savedLoginsKey = string.Format("{0}%{1}", username, sl.GridID);

            if (!(s["saved_logins"] is OSDMap))
            {
                s["saved_logins"] = new OSDMap();
            }

            if (isSaveCredentials)
            {
                OpenMetaverse.Logger.Log("saving user credientials to disk: ", Helpers.LogLevel.Info);
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

                //apparently the startlocation is also part of saveCredentials?

                //if (cbxLocation.SelectedIndex == -1)
                //{
                //    sl.CustomStartLocation = cbxLocation.Text;
                //}
                //else
                //{
                //    sl.CustomStartLocation = string.Empty;
                //}
                //sl.StartLocationType = cbxLocation.SelectedIndex;
                ((OSDMap)s["saved_logins"])[savedLoginsKey] = sl.ToOSD();   //settings_map["saved_logins"] -> savedloginsmap;  savedloginsmap[savedLoginsKey] ->  set: sl.ToOSD(); where savedLoginsKey is username+gridID
            }
            else if (((OSDMap)s["saved_logins"]).ContainsKey(savedLoginsKey))
            {
                ((OSDMap)s["saved_logins"]).Remove(savedLoginsKey);
            }

            s["login_location_type"] = OSD.FromInteger(loginLocationDropdown.selectedId);
            //s["login_location"] = OSD.FromString(cbxLocation.Text);

            //s["login_grid"] = OSD.FromInteger(gridmgr.);  //note: this was removed as this was literally a magic number (int) that corresponds to the position of the dropbox selection.
            s["login_uri"] = OSD.FromString(loginoptions.GridLoginUri);
            s["remember_login"] = isSaveCredentials; //OSD.FromBoolean (loginoptions.IsSaveCredentials);
        }

        #endregion



    }

}