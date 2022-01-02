using OpenMetaverse;
using UnityEngine;
using Logger = OpenMetaverse.Logger;

namespace Raindrop.Services.Bootstrap
{
    //This sets up and tears down the raindrop instance and other servies.
    //This is attached to a scene as the SOLE gameobject. 
    // inspired by https://medium.com/medialesson/simple-service-locator-for-your-unity-project-40e317aad307

    //please attach this script as a component of the game manager
    public class RaindropBootstrapper : MonoBehaviour
    {
        public GameObject UIBootstrapper;

        public static RaindropInstance rd_instance;
        private void Start()
        {
            LinkUnityObjects();

            StartLogger();
            
            Main();

            StartUI();

            
            Logger.Log("Instance initialisation finished. ", Helpers.LogLevel.Info);
        }

        private void StartUI()
        {
            
            
            
        }

        private void StartLogger()
        {
            Logger.Log("OpenMetaverse.Helpers.LogLevel.Info : Application Started.", Helpers.LogLevel.Info);

        }

        //find the unity objects that this script depends on.
        private void LinkUnityObjects()
        {
            var ui = this.gameObject.GetComponent<UIBootstrapper>();
            if (ui == null)
            {
                Debug.LogError("the UI script is not found.");
                Application.Quit();
                return;
            }
            
        }

        private void Main()
        {
           
            //0. servicelocator pattern.
            StartServiceLocator();
            
            //1. main instance.
            CreateAndRegister_RaindropInstance();
            
            // Application is ready to start, load your main UI. 
            // Instantiate(UIBootstrapper);
            
            // SceneManager.LoadScene("UIscene", LoadSceneMode.Additive); //blocking load required as the UIService will be requested a few lines from now.
            // SceneManager.LoadScene("3Dscene", LoadSceneMode.Additive);

            //Debug.Log("main Bootstrap finished");

            
            //Debug.Log("UI should be appeared in front of you");

        }

        public static void CreateAndRegister_RaindropInstance()
        {
            if (ServiceLocator.ServiceLocator.Instance.IsRegistered<RaindropInstance>()) 
                return;
            
            Debug.Log("creating and registering raindropinstance!");
            ServiceLocator.ServiceLocator.Instance.Register<RaindropInstance>(
                new RaindropInstance(new GridClient()));
            rd_instance = ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>();
        }

        public static void StartServiceLocator()
        {
            if (ServiceLocator.ServiceLocator.Instance == null)
            {
                ServiceLocator.ServiceLocator.Initiailze();
            }
        }
    }
}
