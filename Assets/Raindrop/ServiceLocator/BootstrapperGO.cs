using log4net.Config;
using Raindrop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ServiceLocator
{
    //This sets up and tears down the raindrop instance and other servies.
    //This is attached to a scene as the SOLE gameobject. 
    // inspired by https://medium.com/medialesson/simple-service-locator-for-your-unity-project-40e317aad307

    public class BootstrapperGO : MonoBehaviour
    {
        
        private void Start()
        {

            Debug.LogError("BootstrapperGO is probably not working well. please refactor or remove from scene.");
            //if (!log4net.LogManager.GetRepository().Configured)
            //{
            //    // log4net not configured
            //    foreach (log4net.Util.LogLog message in
            //             log4net.LogManager.GetRepository()
            //                    .ConfigurationMessages
            //                    .Cast < log4net.Util.LogLog())
            //    {
            //        // evaluate configuration message
            //    }
            //}

            //setup log4net 
            //XmlConfigurator.Configure(new FileInfo($"{Application.dataPath}/log4net.xml")); //this cause freeze
            //Debug.Log("xmlconfigurator called.");
            OpenMetaverse.Logger.Log("Logger.Log is working, as proven by the existence of this Log message.", OpenMetaverse.Helpers.LogLevel.Info);

            // Initialize default service locator.
            //edit: move to ui scene - uibootstrapper.
            //ServiceLocator.Initiailze();

            // Register all your services next.
            //edit: move to ui scene - uibootstrapper.
            //ServiceLocator.Instance.Register<RaindropInstance>(new RaindropInstance(new OpenMetaverse.GridClient()));
            //ServiceLocator.Instance.Register<UIService>(new UIService());

            // Application is ready to start, load your main UI. 
            //if (enableUI)
            SceneManager.LoadScene("UIscene", LoadSceneMode.Additive); //blocking load required as the UIService will be requested a few lines from now.
            //if (enable3D)
            SceneManager.LoadScene("3Dscene", LoadSceneMode.Additive);

            Debug.Log("Bootstrap finished, all scenes loaded.!");

            //edit: move to ui scene - uibootstrapper.
            //ServiceLocator.Instance.Get<UIService>().startUIInitialView();

            //Debug.Log("UI should be appeared in front of you");
        }



    }
}
