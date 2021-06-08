using OpenMetaverse;
using OpenMetaverse.StructuredData;
using Raindrop;
using Raindrop.Netcom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Raindrop
{
    class LoginUtils
    {
        // this function appends (and saves) content of loginoptions to globalsettings file.
        //it is called when the user clicks the login button.
        public static void SaveConfig(LoginOptions loginoptions, Raindrop.Settings globalSettings, bool isSaveCredentials)
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
                Debug.Log("Saving user cred");
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

            //s["login_location_type"] = OSD.FromInteger(cbxLocation.SelectedIndex);
            //s["login_location"] = OSD.FromString(cbxLocation.Text);

            //s["login_grid"] = OSD.FromInteger(gridmgr.);  //note: this was removed as this was literally a magic number (int) that corresponds to the position of the dropbox selection.
            s["login_uri"] = OSD.FromString(loginoptions.GridLoginUri);
            s["remember_login"] = isSaveCredentials; //OSD.FromBoolean (loginoptions.IsSaveCredentials);
        }

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

        //clear the content of disk-cached user and pass. (UI side)
        private void ClearConfig(Settings s)
        {
            //Settings s = settings;
            s["username"] = string.Empty;
            s["password"] = string.Empty;
        }


        public static bool getRememberFromSettings (Settings settings)
        {
            Settings s = settings;
            return (bool) s["remember_login"];

        }
        public static void setRememberToSettings(Settings settings, bool isRemember)
        {
            Settings s = settings;
            settings["remember_login"] = isRemember;

        }



    }
}
