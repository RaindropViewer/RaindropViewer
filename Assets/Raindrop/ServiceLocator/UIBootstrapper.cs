using Raindrop.Netcom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using ServiceLocator;
using Raindrop.Map.Model;

namespace Raindrop
{
    //get the UI manger and show the first UI panel to the user.
    class UIBootstrapper : MonoBehaviour
    {
        private RaindropInstance instance => ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>();

        private RaindropNetcom netcom => instance.Netcom;

        // bootstraps in order to obtain the canvasmanager instance.
        // bootstraps to find all instances of panels in the children tree.
        // has a funny role; in that it will register itself to the UIservice on start/awake. this should really be the other way round - that the UIservice creates/has dependency on the UIrootGO!!!
        private void Awake()
        {
            ServiceLocator.ServiceLocator.Initiailze();

            if (ServiceLocator.ServiceLocator.Instance.IsRegistered<UIService>())
            {
                Debug.LogError("Attempted to register UI Service again! ");
                return;
            }

            if (! ServiceLocator.ServiceLocator.Instance.IsRegistered<RaindropInstance>())
            {
                Debug.LogWarning("UIBootstrapper creating and registering raindropinstance!");
                ServiceLocator.ServiceLocator.Instance.Register<RaindropInstance>(new RaindropInstance(new OpenMetaverse.GridClient()));
                //return;
            }

            if (! ServiceLocator.ServiceLocator.Instance.IsRegistered<MapBackend>())
            {
                Debug.LogWarning("UIBootstrapper creating and registering MapBackend.MapFetcher!");
                ServiceLocator.ServiceLocator.Instance.Register<MapBackend>(new MapBackend());
                //return;
            }

            var cm = GetComponentInChildren<CanvasManager>();
            var mm = GetComponentInChildren<ModalManager>();
            
            ServiceLocator.ServiceLocator.Instance.Register<UIService>(new UIService(cm,mm));
        }

        private void Start()
        {
            ServiceLocator.ServiceLocator.Instance.Get<UIService>().startUIInitialView();
            Debug.Log("UI should be appeared");
            
        }

        void OnApplicationQuit()
        {
            Debug.Log("Application ending after " + Time.time + " seconds"); 

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

            frmMain_Disposed();
            Debug.Log("disposed mainform! :)");
        }

        //wraps up the netcom and client.
        void frmMain_Disposed( )
        {
            if (netcom != null)
            {
                //netcom = null;
                //netcom.ClientLoginStatus -= new EventHandler<LoginProgressEventArgs>(netcom_ClientLoginStatus);
                //netcom.ClientLoggedOut -= new EventHandler(netcom_ClientLoggedOut);
                //netcom.ClientDisconnected -= new EventHandler<DisconnectedEventArgs>(netcom_ClientDisconnected);
            }

            //if (instance.Client != null)
            //{
            //    UnregisterClientEvents(client);
            //}

            //if (instance?.Names != null)
            //{
            //    instance.Names.NameUpdated -= new EventHandler<UUIDNameReplyEventArgs>(Names_NameUpdated);
            //}

            instance.CleanUp();
        }
    }
}
