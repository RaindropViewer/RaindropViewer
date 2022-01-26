﻿using System.IO;
using System.Reflection;
using System.Threading;
using Disk;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;
using OpenMetaverse;
using Raindrop.Netcom;
using UnityEngine;
using Logger = OpenMetaverse.Logger;

namespace Raindrop.Services.Bootstrap
{
    //This sets up and tears down the raindrop instance service.
    // inspired by https://medium.com/medialesson/simple-service-locator-for-your-unity-project-40e317aad307

    //Just attach this script as a component of the game manager. You don't need anything else, unless you want the UI.
    public class RaindropBootstrapper : MonoBehaviour
    {
        //private RaindropInstance instance => ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>();
        //private RaindropNetcom netcom => instance.Netcom;
        private void Awake()
        {
            Start_Raindrop_CoreDependencies();
        }

        //create the gameobject that helps to create the filesystem
        private static void Init_StaticFileSystem()
        {
            // start the startupCopier
            var startupCopierObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var startupCopier = startupCopierObj.AddComponent<CopyStreamingAssetsToPersistentDataPath>();
        }

        private static void SendStartupMessageToLogger()
        {
            Logger.Log("RaindropBootstrapper.cs : RaindropInstance Started and succesfully linked to servicelocator.", Helpers.LogLevel.Info);
        }

        public static void Start_Raindrop_CoreDependencies()
        {
            //0. start rolling log file
            // ConfigureRollingLogFile();
            
            Init_StaticFileSystem();
            
            //0. start servicelocator pattern.
            StartServiceLocator();
            
            //1. construct raindrop instance and register it
            CreateAndRegister_RaindropInstance();
        }

        private static void ConfigureRollingLogFile()
        {
            // log4net.Config.XmlConfigurator.Configure();
            
            //1. log to unity console.
            var unityDebugAppender = new UnityDebugAppender();
            unityDebugAppender.Layout = new PatternLayout("%date{HH:mm:ss} [%level] - %message");
            //appender.Layout = new PatternLayout("%timestamp [%thread] %-5level - %message%newline");
            BasicConfigurator.Configure(unityDebugAppender);

            
            var logFileAppender = new RollingFileAppender();
            logFileAppender.Layout = new PatternLayout("%date{HH:mm:ss} [%level] - %message");
            logFileAppender.File = Path.Combine(
                DirectoryHelpers.GetInternalCacheDir(),
                "log.txt");
            logFileAppender.AppendToFile = true;
            logFileAppender.RollingStyle = RollingFileAppender.RollingMode.Size;
            logFileAppender.MaxSizeRollBackups = 10;
            logFileAppender.MaxFileSize = 250 * 1000; // 250KB
            logFileAppender.StaticLogFileName = true;
            XmlConfigurator.Configure();

            Logger.Log("Logger is ready", Helpers.LogLevel.Debug);
            Logger.Log("Logger is logging to : "+  Path.Combine(
                DirectoryHelpers.GetInternalCacheDir(),
                "log.txt")
                , Helpers.LogLevel.Debug);
        }

        public static void CreateAndRegister_RaindropInstance()
        {
            if (ServiceLocator.ServiceLocator.Instance.IsRegistered<RaindropInstance>()) 
                return;
            
            var rdi = new RaindropInstance(new GridClient());
            ServiceLocator.ServiceLocator.Instance.Register<RaindropInstance>(rdi);
            SendStartupMessageToLogger();
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
            RaindropInstance instance;
            try
            {
                instance = ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>();
            }
            catch
            {
                return;
            }

            RaindropNetcom netcom = instance.Netcom;
            
            OpenMetaverse.Logger.Log("Application ending after " + Time.time + " seconds"
                , Helpers.LogLevel.Debug); 

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

            frmMain_Disposed(instance);
            Debug.Log("disposed mainform! :)");
        }

        //wraps up the netcom and client.
        void frmMain_Disposed(RaindropInstance instance)
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

            //if (instance?.Names != null)
            //{
            //    instance.Names.NameUpdated -= new EventHandler<UUIDNameReplyEventArgs>(Names_NameUpdated);
            //}

            instance.CleanUp();
        }

    }
}