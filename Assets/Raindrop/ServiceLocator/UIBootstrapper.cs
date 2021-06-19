using Raindrop.Netcom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Raindrop
{
    //get the UI manger and show the first UI panel to the user.
    class UIBootstrapper : MonoBehaviour
    {
        private RaindropInstance instance => Raindrop.RaindropInstance.GlobalInstance;

        private RaindropNetcom netcom => instance.Netcom;

        private void Awake()
        {
            ServiceLocatorSample.ServiceLocator.ServiceLocator.Current.Get<UIManager>().initialiseUI();
        }

        void OnApplicationQuit()
        {
            Debug.Log("Application ending after " + Time.time + " seconds");
            Debug.Log("logging out");

            //if (instance.GlobalSettings["confirm_exit"].AsBoolean())
            //{
            //    if (MessageBox.Show("Are you sure you want to exit Radegast?", "Confirm Exit",
            //            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            //    {
            //        e.Cancel = true;
            //        return;
            //    }
            //}

            //if (statusTimer != null)
            //{
            //    statusTimer.Stop();
            //    statusTimer.Dispose();
            //    statusTimer = null;
            //}

            //if (MediaConsole != null)
            //{
            //    if (TabConsole.TabExists("media"))
            //    {
            //        TabConsole.Tabs["media"].AllowClose = true;
            //        TabConsole.Tabs["media"].Close();
            //    }
            //    else
            //    {
            //        MediaConsole.Dispose();
            //    }
            //    MediaConsole = null;
            //}

            if (!netcom.IsLoggedIn) return;

            Thread saveInvToDisk = new Thread(delegate ()
            {
                instance.Client.Inventory.Store.SaveToDisk(instance.InventoryCacheFileName);
            })
            {
                Name = "Save inventory to disk"
            };
            saveInvToDisk.Start();

            netcom.Logout();

            Debug.Log("Logged out! :)");
        }
    }
}
