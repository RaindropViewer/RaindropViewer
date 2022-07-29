using System;
using System.IO;
using System.Threading;
using Disk;
using OpenMetaverse;
using Plugins.CommonDependencies;
using Raindrop.Netcom;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityScripts.Disk;
using Logger = OpenMetaverse.Logger;

namespace Raindrop.Bootstrap
{
    // This sets up and tears down the raindrop instance service.

    // To use,
    // Just attach this script as a component of a gameobject in bootstrap
    // scene.
    // Then, you can disable/enable UI-scene loading using the boolean
    
    // Disable UI scene loading for headless tests
    
    public class RaindropBootstrapper : ScriptableObject
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Start()
        {
            RegisterToAppTeardownEvent();
            Start_Raindrop_CoreDependencies();
        }

        private static void RegisterToAppTeardownEvent()
        {
#if !UNITY_EDITOR
            Application.quitting += Quit_Application;
#endif
#if UNITY_EDITOR
            
            EditorApplication.playModeStateChanged -=
                InternalPlayModeStateChanged;
            EditorApplication.playModeStateChanged +=
                InternalPlayModeStateChanged;
#endif
        }

        /// <summary>For in-editor usage only.</summary>
        public static void InternalPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingPlayMode:
                    Quit_Application();
                    break;
                case PlayModeStateChange.ExitingEditMode:
                case PlayModeStateChange.EnteredPlayMode:
                case PlayModeStateChange.EnteredEditMode:
                    break;
            }
        }

        #region Bootstrap functions
        
        private void StartUIScene()
        {
            SceneManager.LoadScene(
                "Raindrop/Bootstrap/MainScene", 
                LoadSceneMode.Additive);
        }

        // prep user file system for LMV's usage, by copying static files.
        private static void CheckAndUpdate_StaticFileSystem()
        {
            var copier = StaticFilesCopier.GetInstance();
            var copy_result = copier.Work();
            if (copy_result == -1)
            {
                Logger.Log(
                    "static files copier failed", 
                    Helpers.LogLevel.Error);
            }
        }

        private static void SendStartupMessageToLogger()
        {
            Logger.Log("RaindropBootstrapper.cs : " +
                       "RaindropInstance Started and " +
                       "succesfully linked to servicelocator.", 
                Helpers.LogLevel.Info);
        }

        public static void Start_Raindrop_CoreDependencies()
        {
            //0. start rolling log file
            // ConfigureRollingLogFile();

            StartupPrintLogger();
            
            Init_Globals();
            
            CheckAndUpdate_StaticFileSystem();
            
            //0. start servicelocator pattern.
            StartServiceLocator();
            
            //1. construct raindrop instance and register it
            CreateAndRegister_RaindropInstance();
        }

        private static void Init_Globals()
        {
            Globals.Init();
        }

       
        public static void StartupPrintLogger()
        {
            Logger.Log("Logger is ready", Helpers.LogLevel.Debug);
            Logger.Log("Logger is logging to : "+  Path.Combine(
                    DirectoryHelpers.GetInternalStorageDir(),
                    "log.txt")
                , Helpers.LogLevel.Debug);
        }

        public static void CreateAndRegister_RaindropInstance()
        {
            var instance = RaindropInstance.GlobalInstance;
            SendStartupMessageToLogger();
        }

        public static void StartServiceLocator()
        {
            if (ServiceLocator.Instance == null)
            {
                ServiceLocator.Initiailze();
            }
        }
        
        

        // globally-accessible quit method.
        public static void Quit_Application()
        {
            void SaveInventoryToDiskAndLogout(RaindropInstance raindropInstance, RaindropNetcom raindropNetcom)
            {
                Thread saveInvToDisk = new Thread(delegate()
                {
                    raindropInstance.Client.Inventory.Store.SaveToDisk(raindropInstance.InventoryCacheFileName);
                })
                {
                    Name = "Save inventory to disk"
                };
                saveInvToDisk.Start();

                raindropNetcom.Logout();
                Debug.Log("Logged out! :)");
            }

            RaindropInstance instance;
            try
            {
                instance = RaindropInstance.GlobalInstance;
            }
            catch
            {
                return;
            }

            RaindropNetcom netcom = instance.Netcom;

            OpenMetaverse.Logger.Log("Application ending after " + Time.time + " seconds"
                , Helpers.LogLevel.Debug);

            // if (statusTimer != null)
            // {
            //     statusTimer.Stop();
            //     statusTimer.Dispose();
            //     statusTimer = null;
            // }

            if (netcom.IsLoggedIn)
            {
                SaveInventoryToDiskAndLogout(instance, netcom);
            }

            frmMain_Disposed(instance);

            Debug.Log("disposed netcom and client! :) This marks the end of the app.");
        }

        // wraps up the netcom (synchronisation context) ,
        // followed by the client instance.
        static void frmMain_Disposed(RaindropInstance instance)
        {
            RaindropNetcom netcom = instance.Netcom;

            if (netcom != null)
            {
                netcom.Dispose();
            }
            //
            // if (instance.Client != null)
            // {
            //     instance.UnregisterClientEvents(client);
            // }

            // if (instance?.Names != null)
            // {
            //     instance.Names.NameUpdated -= new EventHandler<UUIDNameReplyEventArgs>(Names_NameUpdated);
            // }

            instance.CleanUp();
        }
        #endregion

    }
}
