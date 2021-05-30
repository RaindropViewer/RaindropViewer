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


//view(unitytext) -- presenter(this) -- controller(this?) -- model (raindropinstance singleton)
namespace Raindrop.Presenters
{

    public class LoginPresenter : MonoBehaviour
    {

        private RaindropInstance instance { get { return RaindropInstance.GlobalInstance; } }
        private RaindropNetcom netcom { get { return instance.Netcom; } }

        #region UI references
        public Button LoginButton;
        public TMP_InputField usernameField;
        public TMP_InputField passwordField;
        public Dropdown gridDropdown;
        public Toggle TOSCheckbox;
        public Toggle RememberCheckbox;

        public ModalManager ModalMgr;
        public Modal loginStatusModal;
        #endregion

        #region internal representations 

        private string username;
        private string password;
        private readonly string INIT_USERNAME = "username";
        private readonly string INIT_PASSWORD = "password";


        public string Username
        {
            get; set;
        }

        public string Password
        {
            get; set;
        }

        public ReactiveProperty<string> Login_msg { get; private set; }

        private List<string> gridDropdownOptions = new List<string> //type is grid
        {
        };

        private int gridSelectedItem;

        private object lblLoginStatus;
        private string login_msg;

        private bool btnLoginEnabled;

        private bool cbTOStrue = true;
        private bool cbRememberBool;
        private string[] options;

        #endregion

        #region init

        // Use this for initialization
        void Start()
        {
            initialiseFields();

            LoginButton.onClick.AsObservable().Subscribe(_ => OnLoginBtnClick()); //when clicked, runs this method.
            usernameField.onValueChanged.AsObservable().Subscribe(_ => Username = _); //change username property.
            passwordField.onValueChanged.AsObservable().Subscribe(_ => Password = _); //change username property.

            RememberCheckbox.OnValueChangedAsObservable().Subscribe(_ => cbRememberBool = _); //when toggle checkbox, set boolean to the same value as the toggle-state
            TOSCheckbox.OnValueChangedAsObservable().Subscribe(_ => cbTOStrue = _); //when toggle checkbox, set boolean to the same value as the toggle-state

            gridDropdown.OnValueChangedAsObservable().Subscribe(_ => gridSelectedItem = _);


            AddNetcomEvents();
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
                    //lblLoginStatus.ForeColor = Color.Black;
                    break;

                case LoginStatus.ConnectingToSim:
                    Login_msg.Value = ("Connecting to region...");
                    //lblLoginStatus.ForeColor = Color.Black;
                    break;

                case LoginStatus.Redirecting:
                    Login_msg.Value = "Redirecting...";
                    //lblLoginStatus.ForeColor = Color.Black;
                    break;

                case LoginStatus.ReadingResponse:
                    Login_msg.Value = "Reading response...";
                    //lblLoginStatus.ForeColor = Color.Black;
                    break;

                case LoginStatus.Success:
                    Login_msg.Value = "Logged in as " + netcom.LoginOptions.FullName;
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
                        Login_msg.Value = "Must agree to Terms of Service before logging in";
                        //pnlTos.Visible = true;
                        //txtTOS.Text = e.Message.Replace("\n", "\r\n");
                        btnLoginEnabled = false;
                    }
                    else
                    {
                        Login_msg.Value = e.Message;
                        btnLoginEnabled = true;
                    }
                    //proLogin.Visible = false;

                    //btnLogin.Text = "Retry";
                    break;
            }
        }

        public void netcom_ClientLoggedOut(object sender, EventArgs e)
        {
            Login_msg.Value = "logged out.";
            //pnlLoginPrompt.Visible = true;
            //pnlLoggingIn.Visible = false;

            //btnLogin.Text = "Exit";
            //btnLogin.Enabled = true;
        }

        public void netcom_ClientLoggingOut(object sender, OverrideEventArgs e)
        {
            btnLoginEnabled = false;

            Login_msg.Value = "Logging out...";
            //lblLoginStatus.ForeColor = Color.FromKnownColor(KnownColor.ControlText);

            //proLogin.Visible = true;
        }

        public void netcom_ClientLoggingIn(object sender, OverrideEventArgs e)
        {
            Login_msg.Value = "Logging in...";
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
            instance.MainCanvas.modalManager.showModal("v", message);
        }



        private void initialiseFields()
        {
            //reset user and pw fields
            username = INIT_USERNAME;
            password = INIT_PASSWORD;

            //modal 
            Login_msg = new ReactiveProperty<string>("???");

            //grids selector
            InitializeConfig();
            //location selector

        }

        //make sure you have done the needful before this, such as reading the grid xml file.
        private void InitializeConfig()
        {
            // Initilize grid dropdown
            int gridIx = -1;


            gridDropdown.ClearOptions();
            for (int i = 0; i < instance.GridManger.Count; i++)
            {
                //gridList.Add(instance.GridManger[i]);
                gridDropdownOptions.Add(instance.GridManger[i].ToString());
                //Debug.Log(instance.GridManger[i].ToString());
            }
            gridDropdownOptions.Add("Custom");

            gridDropdown.AddOptions(gridDropdownOptions);

            //if (gridIx != -1)
            //{
            //    cbxGrid.SelectedIndex = gridIx;
            //}


            Settings s = instance.GlobalSettings;
            RememberCheckbox.isOn = s["remember_login"];

            // Setup login name
            string savedUsername;

            savedUsername = s["username"];

            usernameField.text = (savedUsername);

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
            passwordField.text = s["password"].AsString();
            Debug.Log("pass cache: " + passwordField.text);

            // Setup login location either from the last used or
            // override from the command line
            //if (string.IsNullOrEmpty(MainProgram.s_CommandLineOpts.Location))
            //{
            //    // Use last location as default
            //    if (s["login_location_type"].Type == OSDType.Unknown)
            //    {
            //        cbxLocation.SelectedIndex = 1;
            //        s["login_location_type"] = OSD.FromInteger(1);
            //    }
            //    else
            //    {
            //        cbxLocation.SelectedIndex = s["login_location_type"].AsInteger();
            //        cbxLocation.Text = s["login_location"].AsString();
            //    }
            //}
            //else
            //{
            //    switch (MainProgram.s_CommandLineOpts.Location)
            //    {
            //        case "home":
            //            cbxLocation.SelectedIndex = 0;
            //            break;

            //        case "last":
            //            cbxLocation.SelectedIndex = 1;
            //            break;

            //        default:
            //            cbxLocation.SelectedIndex = -1;
            //            cbxLocation.Text = MainProgram.s_CommandLineOpts.Location;
            //            break;
            //    }
            //}


            //// Set grid dropdown to last used, or override from command line
            //if (string.IsNullOrEmpty(MainProgram.s_CommandLineOpts.Grid))
            //{
            //    cbxGrid.SelectedIndex = s["login_grid"].AsInteger();
            //}
            //else if (gridIx == -1) // --grid specified but not found
            //{
            //    MessageBox.Show($"Grid specified with --grid {MainProgram.s_CommandLineOpts.Grid} not found",
            //        "Grid not found",
            //        MessageBoxButtons.OK,
            //        MessageBoxIcon.Warning
            //        );
            //}

            //// Restore login uri from settings, or command line
            //if (string.IsNullOrEmpty(MainProgram.s_CommandLineOpts.LoginUri))
            //{
            //    txtCustomLoginUri.Text = s["login_uri"].AsString();
            //}
            //else
            //{
            //    txtCustomLoginUri.Text = MainProgram.s_CommandLineOpts.LoginUri;
            //    cbxGrid.SelectedIndex = cbxGrid.Items.Count - 1;
            //}

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
            netcom.LoginOptions.Grid = instance.GridManger.Grids[gridSelectedItem];
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

}