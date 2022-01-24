using System;
using System.Threading;
using Raindrop.Map.Model;
using Raindrop.Netcom;
using UnityEngine;

namespace Raindrop.Services.Bootstrap
{
    // link all the UI together
    // show the first UI panel to the user.
    class UIBootstrapper : MonoBehaviour
    {
        private RaindropInstance instance => ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>();
        private RaindropNetcom netcom => instance.Netcom;

        // bootstraps the UI.
        // 1. it first bootstraps the base layer of RaindropInstance
        // 2. then it find all the canvasmanager and modal managers.
        
        // has a funny role; in that it will register itself to the UIservice on start/awake.
        // this should really be the other way round - that the UIservice
        // creates/has dependency on the UIrootGO!!!
        private void Awake()
        {
            RaindropBootstrapper.Start_RaindropInstance();
            OpenMetaverse.Logger.Log("UI variant of application Started. Logging Started.", OpenMetaverse.Helpers.LogLevel.Info);
        }
        
        // initialise the UI items
        private void Start()
        {
            //1. mapfetcher - logic, not ui. please refactor
            if (!ServiceLocator.ServiceLocator.Instance.IsRegistered<MapService>())
            {
                Debug.Log("UIBootstrapper creating and registering MapBackend.MapFetcher!");
                ServiceLocator.ServiceLocator.Instance.Register<MapService>(new MapService());
                //return;
            }

            //2. ui services
            var cm = GetComponentInChildren<ScreensManager>();
            if (cm == null)
                Debug.LogError("canvasmanager not present");
            var mm = GetComponentInChildren<ModalManager>();
            if (mm == null)
                Debug.LogError("modalmanager not present");
            ServiceLocator.ServiceLocator.Instance.Register<UIService>(new UIService(cm, mm));
        }

        
    }
}
