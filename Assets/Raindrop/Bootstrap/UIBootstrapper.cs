using System;
using System.Threading;
using Raindrop.Map.Model;
using Raindrop.Netcom;
using Raindrop.Presenters;
using UnityEngine;
using UnityEngine.Serialization;

namespace Raindrop.Services.Bootstrap
{
    // link all the UI together
    class UIBootstrapper : MonoBehaviour
    {
        private RaindropInstance instance => ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>();
        private RaindropNetcom netcom => instance.Netcom;

        [FormerlySerializedAs("Injector")] [SerializeField] public References references;
        [SerializeField] public int DelayMs;
        // bootstraps the UI.
        // 1. it first bootstraps the base layer of RaindropInstance
        // 2. then it find all the canvasmanager and modal managers.
        
        // has a funny role; in that it will register itself to the UIservice on start/awake.
        // this should really be the other way round - that the UIservice
        // creates/has dependency on the UIrootGO!!!
        private void Awake()
        {
            RaindropBootstrapper.Start_Raindrop_CoreDependencies(); // hacky - to ensure that the UI's dependencies are ready.
            OpenMetaverse.Logger.Log("UI variant of application Started. Logging Started.", OpenMetaverse.Helpers.LogLevel.Info);
            InitialiseUIVariant();
        }
        
        // initialise the UI items
        private void Start()
        {
            //Invoke("InitialiseUIVariant", DelayMs);
        }

        //warn: if this init method is called too early, there can be issues.
        private void InitialiseUIVariant()
        {
            //1. mapfetcher - logic, not ui. please refactor
            if (!ServiceLocator.ServiceLocator.Instance.IsRegistered<MapService>())
            {
                ServiceLocator.ServiceLocator.Instance.Register<MapService>(new MapService());
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

            ServiceLocator.ServiceLocator.Instance.Register<UIService>(new UIService(
                references.sm, 
                references.mm, 
                references.ll,
                references.chatPresenter));
            
            //3. start the chat window right.
            references.chatPresenter.Initialise();
        }
    }
}
