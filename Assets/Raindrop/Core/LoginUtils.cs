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
        // An instance of a saved login from previous successful logins. 
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
                RaindropInstance instance = ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>();
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

        //
        // public static bool getRememberFromSettings (Settings settings)
        // {
        //     Settings s = settings;
        //     return (bool) s["remember_login"];
        //
        // }
        // public static void setRememberToSettings(Settings settings, bool isRemember)
        // {
        //     Settings s = settings;
        //     settings["remember_login"] = isRemember;
        //
        // }



    }
}
