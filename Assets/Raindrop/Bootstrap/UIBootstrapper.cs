﻿using System;
using System.Threading;
using Raindrop.Map.Model;
using Raindrop.Netcom;
using Raindrop.Presenters;
using UnityEngine;

namespace Raindrop.Services.Bootstrap
{
    // link all the UI together
    // show the first UI panel to the user.
    class UIBootstrapper : MonoBehaviour
    {
        private RaindropInstance instance => ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>();
        private RaindropNetcom netcom => instance.Netcom;

        [SerializeField] public ModalManager mm;
        [SerializeField] public ScreensManager sm;
        [SerializeField] public LoadingCanvasPresenter ll;
        
        
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
        }
        
        // initialise the UI items
        private void Start()
        {
            //1. mapfetcher - logic, not ui. please refactor
            if (!ServiceLocator.ServiceLocator.Instance.IsRegistered<MapService>())
            {
                ServiceLocator.ServiceLocator.Instance.Register<MapService>(new MapService());
            }

            //2. ui services
            //getcomponent only can handle active objects.
            // var cm = GetComponentInChildren<ScreensManager>();
            if (sm == null)
                Debug.LogError("ScreensManager not present");
            // var mm = GetComponentInChildren<ModalManager>();
            if (mm == null)
                Debug.LogError("ModalManager not present");
            // var ll = GetComponentInChildren<LoadingCanvasPresenter>();
            if (ll == null)
                Debug.LogError("loadingscreen not present");
            ServiceLocator.ServiceLocator.Instance.Register<UIService>(new UIService(sm, mm, ll));
        }

        
    }
}
