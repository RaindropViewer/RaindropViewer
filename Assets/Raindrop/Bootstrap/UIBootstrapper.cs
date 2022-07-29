using System;
using Plugins.CommonDependencies;
using Raindrop.Map.Model;
using Raindrop.Netcom;
using Raindrop.Services;
using UnityEngine;
using UnityEngine.Serialization;

namespace Raindrop.Bootstrap
{
    // link all the UI together
    class UIBootstrapper : MonoBehaviour
    {
        private RaindropInstance instance => RaindropInstance.GlobalInstance;
        private RaindropNetcom netcom => instance.Netcom;

        [SerializeField] public References references;

        private UIService _uisrv;

        // bootstraps the UI.
        private void Start()
        // 1. finds all the canvasmanager and modal managers.
        // 2. registers them to service locator
        {
            OpenMetaverse.Logger.Log("UI layer of application Started. Logging Started.", OpenMetaverse.Helpers.LogLevel.Info);
            InitialiseUIVariant();
        }

        private void OnDisable()
        {
            if (ServiceLocator.Instance.IsRegistered<MapService>())
            {
                ServiceLocator.Instance.Unregister<MapService>();
            }
            if (ServiceLocator.Instance.IsRegistered<UIService>())
            {
                ServiceLocator.Instance.Unregister<UIService>();
            }
            _uisrv.Dispose();
            
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

            _uisrv = new UIService(
                instance,
                references.sm,
                references.mm,
                references.ll,
                references.chatPresenter);
            ServiceLocator.Instance.Register<UIService>(_uisrv);

            _uisrv.MapFacade = references.mapUI;
            
            //3. start the chat window right.
            references.chatPresenter.Initialise();
        }
    }
}
