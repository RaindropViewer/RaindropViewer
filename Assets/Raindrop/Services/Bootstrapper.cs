//using log4net.Config;
//using Raindrop;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using UnityEngine.SceneManagement;

//namespace ServiceLocator
//{
//    //This is like the main() function. it runs before everything else.
//    // inspired by https://medium.com/medialesson/simple-service-locator-for-your-unity-project-40e317aad307
//    //<Deprecated>
//    public static class Bootstrapper
//    {
//        //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
//        public static void Initiailze()
//        {
//            //if (!log4net.LogManager.GetRepository().Configured)
//            //{
//            //    // log4net not configured
//            //    foreach (log4net.Util.LogLog message in
//            //             log4net.LogManager.GetRepository()
//            //                    .ConfigurationMessages
//            //                    .Cast < log4net.Util.LogLog())
//            //    {
//            //        // evaluate configuration message
//            //    }
//            //}

//            //setup log4net 
//            //XmlConfigurator.Configure(new FileInfo($"{Application.dataPath}/log4net.xml")); //this cause freeze
//            //Debug.Log("xmlconfigurator called.");
//            OpenMetaverse.Logger.Log("Logger.Log is working.", OpenMetaverse.Helpers.LogLevel.Info);

//            // Initialize default service locator.
//            ServiceLocator.Initiailze();

//            // Register all your services next.
//            ServiceLocator.Instance.Register<RaindropInstance>(new RaindropInstance(new OpenMetaverse.GridClient()));
//            ServiceLocator.Instance.Register<UIService>(new UIService());

//            // Application is ready to start, load your main scene.
//            Scene[] currentScenes = SceneManager.GetAllScenes();
//            bool loadUI = true;
//            bool load3D = true;
//            foreach(var scene in currentScenes)
//            {
//                if (scene.name == "UIscene")
//                {
//                    loadUI = false;
//                }
//                if (scene.name == "3Dscene")
//                {
//                    load3D = false;
//                }
//            } 
    
//            if (loadUI)
//                SceneManager.LoadScene("UIscene", LoadSceneMode.Additive);
//            if (load3D)
//                SceneManager.LoadScene("3Dscene", LoadSceneMode.Additive);

//            Debug.Log("Bootstrap finished, all scenes loaded.!");

//            ServiceLocator.Instance.Get<UIService>().startUIInitialView();

//            Debug.Log("UI should be appeared in front of you");
//        }



//    }
//}
