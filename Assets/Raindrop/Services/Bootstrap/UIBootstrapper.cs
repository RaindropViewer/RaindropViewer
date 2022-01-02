using System;
using System.Threading;
using Raindrop.Map.Model;
using Raindrop.Netcom;
using UnityEngine;

namespace Raindrop.Services.Bootstrap
{
    //show the first UI panel to the user.
    class UIBootstrapper : MonoBehaviour
    {
        public bool UI_Is_on = false;

        private RaindropInstance instance => ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>();

        private RaindropNetcom netcom => instance.Netcom;

        // bootstraps in order to obtain the canvasmanager instance.
        // bootstraps to find all instances of panels in the children tree.
        // has a funny role; in that it will register itself to the UIservice on start/awake.
        // this should really be the other way round - that the UIservice
        // creates/has dependency on the UIrootGO!!!
        private void Awake()
        {
            Main();
        }

        public void Main()
        {
            //OpenMetaverse.Logger.Log("Application Started. Logging Started.", OpenMetaverse.Helpers.LogLevel.Info);

            RaindropBootstrapper.StartServiceLocator();
            
            //skipped if the main bootstrapper is already run.
            RaindropBootstrapper.CreateAndRegister_RaindropInstance();

        }

        private void Start()
        {
            if (!ServiceLocator.ServiceLocator.Instance.IsRegistered<MapFetcher>())
            {
                Debug.Log("UIBootstrapper creating and registering MapBackend.MapFetcher!");
                ServiceLocator.ServiceLocator.Instance.Register<MapFetcher>(new MapFetcher());
                //return;
            }

            bool success = false;
            var cm = GetComponentInChildren<CanvasManager>();
            if (cm == null) Debug.LogError("canvasmanager not present");
            var mm = GetComponentInChildren<ModalManager>();
            if (mm == null) Debug.LogError("modalmanager not present");

            ServiceLocator.ServiceLocator.Instance.Register<UIService>(new UIService(cm, mm));
            
            
            // in start, lauch the ui.
            
            // if (UI_Is_on)
                // ServiceLocator.ServiceLocator.Instance.Get<UIService>().init();
        }


        void OnApplicationQuit()
        {
            Debug.Log("Application ending after " + Time.time + " seconds"); 

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
            //    if (MessageBox.Show("Are you sure you
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
