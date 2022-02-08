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
                  "We are using configuration file at: " 
                  + $"{DirectoryHelpers.GetInternalCacheDir()}/log4net.xml");
        // var res = XmlConfigurator.Configure(new FileInfo($"{DirectoryHelpers.GetInternalCacheDir()}/log4net.xml"));
        ConfigureLoggers(
            Path.Combine(DirectoryHelpers.GetInternalCacheDir(), "log.txt"));


    }
    public static void ConfigureLoggers(string filepath)
    {
        // log4net.Config.XmlConfigurator.Configure();
        Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
        
        //1. log to unity console.
        var unityDebugAppender = new UnityDebugAppender();
        unityDebugAppender.Layout = new PatternLayout("%date{HH:mm:ss} [%level] - %message");
        //appender.Layout = new PatternLayout("%timestamp [%thread] %-5level - %message%newline");
        // BasicConfigurator.Configure(unityDebugAppender);
        hierarchy.Root.AddAppender(unityDebugAppender);

            
        var logFileAppender = new RollingFileAppender();
        logFileAppender.Layout = new PatternLayout("%date{HH:mm:ss} [%level] - %message%newline");
        logFileAppender.File = filepath;
        logFileAppender.AppendToFile = true;
        logFileAppender.RollingStyle = RollingFileAppender.RollingMode.Size;
        logFileAppender.MaxSizeRollBackups = 10;
        logFileAppender.MaxFileSize = 250 * 1000; // 250KB
        logFileAppender.StaticLogFileName = true;   
        logFileAppender.ActivateOptions();
        // XmlConfigurator.Configure();
        hierarchy.Root.AddAppender(logFileAppender);
        
        hierarchy.Configured = true;
    }

}