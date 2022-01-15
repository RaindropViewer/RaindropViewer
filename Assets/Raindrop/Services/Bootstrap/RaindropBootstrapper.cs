using System.Threading;
using OpenMetaverse;
using Raindrop.Netcom;
using UnityEngine;
using Logger = OpenMetaverse.Logger;

namespace Raindrop.Services.Bootstrap
{
    //This sets up and tears down the raindrop instance service.
    //This is attached to a scene as the SOLE gameobject. 
    // inspired by https://medium.com/medialesson/simple-service-locator-for-your-unity-project-40e317aad307

    //please attach this script as a component of the game manager
    public class RaindropBootstrapper : MonoBehaviour
    {
        public GameObject UIBootstrapper;

        private RaindropInstance instance => ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>();
        private RaindropNetcom netcom => instance.Netcom;
        private void Start()
        {
            
            Start_RaindropInstance();
            
        }
        private static void StartLogger()
        {
            Logger.Log("OpenMetaverse.Helpers.LogLevel.Info : Application Started.", Helpers.LogLevel.Info);
        }

        // //find the unity objects that this script depends on.
        // turns out we don't want the base layer to depend on the ui layer, or any unity objects for that matter.
        // private void LinkUnityObjects()
        // {
        //     var ui = this.gameObject.GetComponent<UIBootstrapper>();
        //     if (ui == null)
        //     {
        //         Debug.LogError("the UI script is not found.");
        //         Application.Quit();
        //         return;
        //     }
        // }

        public static void Start_RaindropInstance()
        {
            // LinkUnityObjects();

            StartLogger();
            
            //0. start servicelocator pattern.
            StartServiceLocator();
            
            //1. main instance.
            CreateAndRegister_RaindropInstance();
            
            //2. construct raindrop instance and register it
            var client = ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>().Client;
            
            Logger.Log("Start_RaindropInstance Bootstrap Success", Helpers.LogLevel.Info);
        }

        public static void CreateAndRegister_RaindropInstance()
        {
            if (ServiceLocator.ServiceLocator.Instance.IsRegistered<RaindropInstance>()) 
                return;
            
            Debug.Log("creating and registering raindropinstance!");
            var rdi = new RaindropInstance(new GridClient());
            ServiceLocator.ServiceLocator.Instance.Register<RaindropInstance>(rdi);
        }

        public static void StartServiceLocator()
        {
            if (ServiceLocator.ServiceLocator.Instance == null)
            {
                ServiceLocator.ServiceLocator.Initiailze();
            }
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
                netcom.Dispose();
            }
            //
            // if (instance.Client != null)
            // {
            //     instance.UnregisterClientEvents(client);
            // }

            //if (instance?.Names != null)
            //{
            //    instance.Names.NameUpdated -= new EventHandler<UUIDNameReplyEventArgs>(Names_NameUpdated);
            //}

            instance.CleanUp();
        }

    }
}
