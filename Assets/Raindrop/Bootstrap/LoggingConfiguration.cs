using System;
using System.IO;
using Disk;
using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Raindrop.Services.Bootstrap;
using UnityEngine;

public static class LoggingConfiguration
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Configure()
    {
        // var key = "log4net.Internal.Debug";
        // string value = true.ToString();
        //
        // try  
        // {  
        //     var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);  
        //     var settings = configFile.AppSettings.Settings;  
        //     if (settings[key] == null)  
        //     {  
        //         settings.Add(key, value);  
        //     }  
        //     else  
        //     {  
        //         settings[key].Value = value;  
        //     }  
        //     configFile.Save(ConfigurationSaveMode.Modified);  
        //     ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);  
        // }  
        // catch (ConfigurationErrorsException)  
        // {  
        //     Debug.Log("Error writing app settings");  
        // }  
        //
        
        Debug.Log("Application is configuring the global logger. \n" +
                  "log file is at : " 
                  + $"{Path.Combine(DirectoryHelpers.GetInternalStorageDir())}/log.txt");
        // var res = XmlConfigurator.Configure(new FileInfo($"{DirectoryHelpers.GetInternalCacheDir()}/log4net.xml"));
        ConfigureLoggers(
            Path.Combine(DirectoryHelpers.GetInternalStorageDir(), "log.txt"));


    }
    public static void ConfigureLoggers(string filepath)
    {


        try
        {
            // log4net.Config.XmlConfigurator.Configure();
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            //1. log to unity console.
            var unityDebugAppender = new UnityDebugAppender();
            unityDebugAppender.Layout = new PatternLayout("%date{HH:mm:ss} [%level] - %message");
            //appender.Layout = new PatternLayout("%timestamp [%thread] %-5level - %message%newline");
            // BasicConfigurator.Configure(unityDebugAppender);
            hierarchy.Root.AddAppender(unityDebugAppender);
            Debug.Log("created unity console appender");
        }
        catch (Exception e)
        {
            Debug.LogError("failed to create unity console appender: " + e.ToString());
        }

        try
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

            var logFileAppender = new FileAppender();
            logFileAppender.Layout = new PatternLayout("%date{HH:mm:ss} [%level] - %message%newline");
            logFileAppender.File = filepath;
            logFileAppender.AppendToFile = true;
            //logFileAppender.RollingStyle = RollingFileAppender.RollingMode.Size;
            //logFileAppender.MaxSizeRollBackups = 10;
            //logFileAppender.MaxFileSize = 250 * 1000; // 250KB
            //logFileAppender.StaticLogFileName = true;   
            logFileAppender.ActivateOptions();
            // XmlConfigurator.Configure();
            hierarchy.Root.AddAppender(logFileAppender);
        
            hierarchy.Configured = true;
            Debug.Log("created filelogger");

        }
        catch (Exception e)
        {
            Debug.LogError("failed to create filelogger, error is : " + e.ToString());
        }

    }

}