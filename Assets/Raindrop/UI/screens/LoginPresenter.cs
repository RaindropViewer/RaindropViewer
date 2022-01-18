using OpenMetaverse;
using OpenMetaverse.StructuredData;
using Raindrop;
using Raindrop.Netcom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using System;
using Raindrop.Services;
using Settings = Raindrop.Settings;
using UnityEngine.UI;
using UniRx;
using TMPro;
using static Raindrop.LoginUtils;


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
        private UIService uimanager => ServiceLocator.ServiceLocator.Instance.Get<UIService>();


        #region UI elements - the 'view' in MVP
        public Button LoginButton;
        public TMP_InputField usernameField;
        public TMP_InputField passwordField;
        public Toggle TOSCheckbox;
        public Toggle RememberCheckbox;

        public TMP_Dropdown locationDropdown;
        private LoginLocationDropdown loginLocationDropdown; //wraps the above
        
        
        public TMP_Text credError;
        #endregion

        #region internal representations
        private Settings s;

        public string Username
        {
            get; set;
        }

        public string Password
        {
            get; set;
        }

        public ReactiveProperty<string> loginMsg = new ReactiveProperty<string>(""); //"" is a magic value that is required to prevent showing the modal immediately on load.
        private ReactiveProperty<bool> btnLoginEnabled = new ReactiveProperty<bool>(true);

        //is empty string if there is no error.
        public ReactiveProperty<string> credParserErrorString = new ReactiveProperty<string>();

        //public string uninitialised = "(unknown)";

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

            btnLoginEnabled.AsObservable().Subscribe(_ => LoginButton.gameObject.SetActive(_)); //update the login button availabilty according to this boolean.

            usernameField.onValueChanged.AsObservable().Subscribe(_ =>
            {
                Username = _;
                credParserErrorString.Value = "";
            });  //change username property.
            passwordField.onValueChanged.AsObservable().Subscribe(_ =>
            {
                Password = _;
                
                credParserErrorString.Value = "";
            });  //change password.

            RememberCheckbox.OnValueChangedAsObservable().Subscribe(_ => cbRememberBool = _); //when toggle checkbox, set boolean to the same value as the toggle-state
            TOSCheckbox.OnValueChangedAsObservable().Subscribe(_ => cbTOStrue = _); //when toggle checkbox, set boolean to the same value as the toggle-state

            loginMsg.AsObservable().Where(_ => ! _.Equals("")).Subscribe(_ => UpdateLoginModalContent("Logging in process...", _));

            credParserErrorString.AsObservable().Subscribe(_ => OnCredParseError(_));
            
            //gridDropdown.OnValueChangedAsObservable().Subscribe(_ => gridSelectedItem = _);
            //customURLCheckbox.onValueChanged.AsObservable().Subscribe(_ => cbCustomURL = _); //change username property.

            //DropdownMenuWithEntry = UserDropdownMenu.GetComponent<DropdownMenuWithEntry>();

            //4subscribe to events.
            AddNetcomEvents();
        }

        // show the error text.
        private void OnCredParseError(string _)
        {
            credError.text = _;
            if (String.IsNullOrEmpty(_))
            {
                credError.gameObject.SetActive(false);
            }
            else
            {
                credError.gameObject.SetActive(true);
            }
        }

        //updates the text in the login modal.
        private void UpdateLoginModalContent(string header, string message)
        {
            if (uimanager == null)
                return;
            if (!uimanager.ready)
                return;

            if (header == "")
            {
                header = "Logging in... ";
            }
            uimanager.modalManager.setLoginModalText(header, message);
        }

        
        private void Close_LoginModal_Slow()
        {
            uimanager.modalManager.fadeLoginModal();
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
            string header = "Logging in process...";
            
            switch (e.Status)
            {
                case LoginStatus.ConnectingToLogin:
                    loginMsg.Value += "Connecting to login server...";
                    break;

                case LoginStatus.ConnectingToSim:
                    loginMsg.Value += ("Connecting to region...");
                    break;

                case LoginStatus.Redirecting:
                    loginMsg.Value += "Redirecting...";
                    break;

                case LoginStatus.ReadingResponse:
                    loginMsg.Value += "Reading response...";
                    break;

                case LoginStatus.Success:
                    loginMsg.Value += "Logged in as " + netcom.LoginOptions.FullName;

                    btnLoginEnabled.Value = false;
                    // instance.Client.Groups.RequestCurrentGroups();

                    Close_LoginModal_Slow();

                    uimanager.canvasManager.PopAndPush(CanvasType.Game);
                    //instance.UI.canvasManager.popCanvas();
                    LoginButton.interactable = true;
                    break;

                case LoginStatus.Failed: 
                    if (e.FailReason == "tos")
                    {
                        loginMsg.Value = "Must agree to Terms of Service before logging in";
                        uimanager.modalManager.showModalNotification("Logging in failed",loginMsg.Value);
                        //pnlTos.Visible = true;
                        //txtTOS.Text = e.Message.Replace("\n", "\r\n");
                        btnLoginEnabled.Value = true;
                        LoginButton.interactable = true;
                    }
                    else
                    {
                        loginMsg.Value = e.Message;
                        uimanager.modalManager.showModalNotification("Logging in failed", loginMsg.Value);
                        btnLoginEnabled.Value = true;
                        LoginButton.interactable = true;
                    }
                    break;
            }
        }

        public void netcom_ClientLoggedOut(object sender, EventArgs e)
        {
            loginMsg.Value = "logged out.";
            uimanager.modalManager.showModalNotification("Login status", loginMsg.Value);
            //pnlLoginPrompt.Visible = true;
            //pnlLoggingIn.Visible = false;

            //btnLogin.Text = "Exit";
            //btnLogin.Enabled = true;
        }

        public void netcom_ClientLoggingOut(object sender, OverrideEventArgs e)
        {
            btnLoginEnabled.Value = false;

            loginMsg.Value = "Logging out...";
            // uimanager.modalManager.showModalNotification("Login status", Login_msg.Value);
        }

        public void netcom_ClientLoggingIn(object sender, OverrideEventArgs e)
        {
            loginMsg.Value = "Start to Logging in...";
            // uimanager.modalManager.showModalNotification("Login status", Login_msg.Value);
            
            btnLoginEnabled.Value = false;
        }
          


        private void initialiseFields()
        {
            //reset user and pw fields
            // Username = INIT_USERNAME;
            // Password = INIT_PASSWORD;

            //login status message for the modal 
            // Login_msg = new ReactiveProperty<string>("");
            
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


        // slowly turns the button back on again.
        IEnumerator EnableButtonCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay / 1000f);
            LoginButton.interactable = true;
        }
        
        // 1. open the login status modal.
        // 2. create a timeout timer -> on time out, allow user to close the login status modal.
        public void OnLoginBtnClick()
        {
            LoginButton.interactable = false;
            //instance.MediaManager.PlayUISound(UISounds.Click);

            //guard username
            string parsedFirstname = "";
            string parsedLastname = "";
            bool isValidUser = splitUserName(Username, out parsedFirstname, out parsedLastname);
            if (!isValidUser)
            {
                credParserErrorString.Value = "bad username. Allowable examples: Kitty Graves , FlyingFox Resident , FlyingFox ";
                Debug.LogError(" username error; invalid username input : " + Username);
                StartCoroutine(EnableButtonCoroutine(1500));
                return;
            }
            
            //guard password
            if (Password.Length <= 0)
            {                
                credParserErrorString.Value = "bad password";
                Debug.LogError(" password error!");
                StartCoroutine(EnableButtonCoroutine(1500));
                return;
            }
            
            //do login.
            BeginLogin(parsedFirstname, parsedLastname, Password);
        }

        // input: ***REMOVED*** resident
        //
        // result:
        // first: ***REMOVED***    last: resident
        // return: true if success.
        private bool splitUserName(string username, out string firstname, out string lastname)
        {
            //split by the dot or space character.
            string[] parts = System.Text.RegularExpressions.Regex.Split(username.Trim(), @"[. ]+");

            if (parts.Length == 2)
            {
                firstname = parts[0];
                lastname = parts[1];
            }
            else if (parts.Length == 1)
            {
                firstname = username.Trim();
                lastname = "Resident";
                //first name might be empty string ""
            }
            else
            {
                firstname = "";
                lastname = "";
                credParserErrorString.Value = "bad username";
                Debug.Log("bad username:  " + username );
                return false;
            }

            if (firstname == "")
            {
                firstname = "";
                lastname = "";
                credParserErrorString.Value = "bad username";
                Debug.Log("bad username:  " + username );
                return false;
                
            }
            
            return true;
        }

        #endregion

        #region Login functions


        private void BeginLogin(string first, string last, string password)
        {
            
            _ = netcom;

            netcom.LoginOptions.FirstName = first;
            netcom.LoginOptions.LastName = last;

            netcom.LoginOptions.Password = password;
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
                // instance.Client.Settings.HTTP_INVENTORY = !instance.GlobalSettings["disable_http_inventory"];
            }
            else
            {
                // UDP inventory is deprecated as of 2015-03-30 and no longer supported.
                // https://community.secondlife.com/t5/Second-Life-Server/Deploy-for-the-week-of-2015-03-30/td-p/2919194
                // instance.Client.Settings.HTTP_INVENTORY = true;
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