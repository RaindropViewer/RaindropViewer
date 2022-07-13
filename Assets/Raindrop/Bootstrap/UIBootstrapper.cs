using System;
using Plugins.CommonDependencies;
using Raindrop.Map.Model;
using Raindrop.Netcom;
using Raindrop.Services;
using UnityEngine;

namespace Raindrop.Bootstrap
{
    // link all the UI together
    class UIBootstrapper : MonoBehaviour
    {
        private RaindropInstance instance => ServiceLocator.Instance.Get<RaindropInstance>();
        private RaindropNetcom netcom => instance.Netcom;

        [SerializeField] public References references;
        // bootstraps the UI.
        // 1. it first bootstraps the base layer of RaindropInstance
        // 2. then it find all the canvasmanager and modal managers.
        
        // has a funny role; in that it will register itself to the UIservice on start/awake.
        // this should really be the other way round - that the UIservice
        // creates/has dependency on the UIrootGO!!!
        private void Start()
        {
            //RaindropBootstrapper.Start_Raindrop_CoreDependencies(); // hacky - to ensure that the UI's dependencies are ready.
            OpenMetaverse.Logger.Log("UI layer of application Started. Logging Started.", OpenMetaverse.Helpers.LogLevel.Info);
            InitialiseUIVariant();
        }

        private void OnDestroy()
        {
            if (ServiceLocator.Instance.IsRegistered<MapService>())
            {
                ServiceLocator.Instance.Unregister<MapService>();
            }
            if (ServiceLocator.Instance.IsRegistered<UIService>())
            {
                ServiceLocator.Instance.Unregister<UIService>();
            }
            
        }

        //warn: if this init method is called too early, there can be issues.
        private void InitialiseUIVariant()
        {
            //1. mapfetcher - logic, not ui. please refactor
            if (!ServiceLocator.Instance.IsRegistered<MapService>())
            {
                ServiceLocator.Instance.Register<MapService>(new MapService());
            }

            //2. ui services
            if (references.sm == null)
                Debug.LogError("ScreensManager not present");
            references.sm.Init();
            if (references.mm == null)
                Debug.LogError("ModalManager not present");
            references.mm.Init();

            if (references.ll == null)
                Debug.LogError("loadingscreen not present");
            references.ll.Init();

            var uisrv = new UIService(
                instance,
                references.sm,
                references.mm,
                references.ll,
                references.chatPresenter);
            ServiceLocator.Instance.Register<UIService>(uisrv);

            uisrv.MapFacade = references.mapUI;
            
            //3. start the chat window right.
            references.chatPresenter.Initialise();
        }
    }
}
