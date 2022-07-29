using System;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
using Plugins.CommonDependencies;
using Raindrop.Bootstrap;
using Raindrop.Netcom;

namespace Raindrop.UI.Login
{
    public class LoginController
    {
        private RaindropInstance instance => RaindropInstance.GlobalInstance;
        public RaindropNetcom netcom => instance.Netcom;
        private LoginPresenter view;
        public bool isDoingLogin;

        public LoginController(LoginPresenter viewPresenter)
        {
            view = viewPresenter;
            
            //1 load default UI fields.
            // initialiseFields();

            //2 load various loginInformation from the settings.
            InitializeConfig();
            
            //GridDropdownGO = this.gameObject.GetComponent<GenericDropdown>().gameObject;
            //genericDropdown = GridDropdownGO.GetComponent<GenericDropdown>();
            //GridDropdownView.DropdownSelectionChanged += GenericDropdown_DropdownSelectionChanged;

            //4subscribe to events.
            AddNetcomEvents();
            
        }
        
        //
        // private void initialiseFields()
        // {
        //     //reset user and pw fields
        //     view.Username = "";
        //     view.Password = "";
        //     
        //     
        // }

        // initialise UI state to previous state
        private void InitializeConfig()
        {
            Settings s = instance.GlobalSettings;
            
            //retrieve previous checkbox value
            Init_RememberLogin_Checkbox();

            //retrieve last logins.
            Init_UserAndPassword_Fields();

            //try to get saved username
            try
            {
                if (s["saved_logins"] is OSDMap)
                {
                    OSDMap savedLogins = (OSDMap)s["saved_logins"];
                    foreach (string loginKey in savedLogins.Keys)
                    {
                        LoginUtils.SavedLogin sl = LoginUtils.SavedLogin.FromOSD(savedLogins[loginKey]);
                        OpenMetaverse.Logger.Log("username cache entry: " + sl.ToString(), Helpers.LogLevel.Info);
                        //cbxUsername.Items.Add(sl);
                        //usernameDropdownMenuWithEntry.Items.Add(sl);
                    }
                }
            }
            catch
            {
                //Debug.LogError("username cache catch block!");
                //cbxUsername.Items.Clear();
                //cbxUsername.Text = string.Empty;
            }

            //cbxUsername.SelectedIndex = 0;
            
            var pass = s["password"].AsString();
            view.Password = pass;
            //Debug.Log("password cache (MD5-ed): " + pass);

            // Setup login location either from the last used  //home = 0; last = 1
            //    Use last location as default
            if (s["login_location_type"].Type == OSDType.Unknown) //if not in cache file?
            {
                // humble.loginLocation = humble.loginLocation; //loginLocationDropdown.select(1);
                s["login_location_type"] = OSD.FromInteger(2); //default set to last
            }else
            {
                //not supported: custom login locations. onyl support home or last. 

                int loginLocationId = s["login_location_type"].AsInteger();
                view.loginLocationDropdown.value = loginLocationId;
                //locationDropdown.Text = s["login_location"].AsString();
            } 
             
            //txtCustomLoginUri.Text = s["login_uri"].AsString();

            void Init_RememberLogin_Checkbox()
            {
                // if (!instance.GlobalSettings.ContainsKey("remember_login"))
                // {
                //     instance.GlobalSettings["remember_login"] = true;
                // }

                // view.isSaveCredentials = instance.GlobalSettings["remember_login"];
            }

            void Init_UserAndPassword_Fields()
            {
                //only retrieve if the grid is matching.
                // if (s["username"])
                //
                // string lastUsername = s["username"];
                // view.Username = lastUsername;
                // string lastPassword = s["password"];
                // view.Password = lastPassword;
            }
        }

        
        
        
        
        
        
        
        #region Login functions

        // do login with the credentials.
        public void BeginLogin(string first, 
            string last, 
            string password,
            bool agreeToTos)
        {
            
            _ = netcom;

            netcom.LoginOptions.FirstName = first;
            netcom.LoginOptions.LastName = last;

            netcom.LoginOptions.Password = password;
            netcom.LoginOptions.Channel = Globals.SimpleAppName; // "Raindrop"
            netcom.LoginOptions.Version = Globals.FullAppName; // "Raindrop 5.1.2"
            netcom.AgreeToTos = agreeToTos;

            //startlocation parsing
            switch (view.loginLocation)
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
            //netcom.LoginOptions.Grid = instance.GridManger.Grids[0]; //0 means sl i think
             

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
            SaveConfig(netcom.LoginOptions, instance.GlobalSettings);
        }
        
        
        // input: ***REMOVED*** resident
        //
        // result:
        // first: ***REMOVED***    last: resident
        // return: true if success.
        public bool splitUserName(string username, out string firstname, out string lastname)
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
                return false;
            }

            if (firstname == "")
            {
                firstname = "";
                lastname = "";
                return false;
                
            }
            
            return true;
        }
        
        #endregion

        #region Settings

        // this function appends (and saves) content of loginoptions to globalsettings file.
        // it is called when the user clicks the login button.
        public void SaveConfig(LoginOptions loginoptions, Raindrop.Settings globalSettings)
        {
            Raindrop.Settings s = globalSettings;
            LoginUtils.SavedLogin sl = new LoginUtils.SavedLogin();

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

            if (true /*isSaveCredentials*/)
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

            s["login_location_type"] = OSD.FromInteger(view.loginLocation);
            //s["login_location"] = OSD.FromString(cbxLocation.Text);

            //s["login_grid"] = OSD.FromInteger(gridmgr.);  //note: this was removed as this was literally a magic number (int) that corresponds to the position of the dropbox selection.
            s["login_uri"] = OSD.FromString(loginoptions.GridLoginUri);
            // s["remember_login"] = isSaveCredentials; //OSD.FromBoolean (loginoptions.IsSaveCredentials);
        }
        #endregion
        
        private void AddNetcomEvents()
        {
            netcom.ClientLoginStatus += new EventHandler<LoginProgressEventArgs>(netcom_ClientLoginStatus);
            netcom.ClientLoggingOut += new EventHandler<OverrideEventArgs>(netcom_ClientLoggingOut);
            netcom.ClientLoggedOut += new EventHandler(netcom_ClientLoggedOut);
        }

        private void RemoveNetcomEvents()
        {
            netcom.ClientLoginStatus -= new EventHandler<LoginProgressEventArgs>(netcom_ClientLoginStatus);
            netcom.ClientLoggingOut -= new EventHandler<OverrideEventArgs>(netcom_ClientLoggingOut);
            netcom.ClientLoggedOut -= new EventHandler(netcom_ClientLoggedOut);
        }


        public void netcom_ClientLoginStatus(object sender, LoginProgressEventArgs e)
        {
            //update the interactivity of the login button.
            switch (e.Status)
            {
                case LoginStatus.ConnectingToLogin:
                    break;
                case LoginStatus.ConnectingToSim:
                    break;
                case LoginStatus.Redirecting:
                    break;
                case LoginStatus.ReadingResponse:
                    break;
                case LoginStatus.Success:
                    break;
                case LoginStatus.Failed:
                    view.loginButtonIsClickable.Value = true;
                    break;
            }
        }

        public void netcom_ClientLoggedOut(object sender, EventArgs e)
        {
            //view.loginMsg.Value = "logged out.";
            // view.loginButtonIsDisabled = false;
            view.loginButtonIsClickable.Value = true;
        }

        public void netcom_ClientLoggingOut(object sender, OverrideEventArgs e)
        {
            // view.loginButtonIsDisabled = false;
            //view.loginMsg.Value = "Logging out...";
        }

    }
}