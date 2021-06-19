//using System.IO;
//using log4net.Config;
//using UnityEngine;

//public static class LoggingConfiguration
//{
//    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
//    private static void ConfigureLogging()
//    {
//        XmlConfigurator.Configure(new FileInfo($"{Application.dataPath}/log4net.xml"));
//        Debug.Log("xmlconfigurator called.");


//        OpenMetaverse.Logger.Log("It is working!", OpenMetaverse.Helpers.LogLevel.Debug);
//    }
//}