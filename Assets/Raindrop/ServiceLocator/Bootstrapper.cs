using Raindrop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ServiceLocatorSample.ServiceLocator
{
    //This is like the main() function. it runs before everything else.
    // inspired by https://medium.com/medialesson/simple-service-locator-for-your-unity-project-40e317aad307

    public static class Bootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Initiailze()
        {
            // Initialize default service locator.
            ServiceLocator.Initiailze();

            // Register all your services next.
            ServiceLocator.Current.Register<RaindropInstance>(new RaindropInstance( new OpenMetaverse.GridClient() ));
            ServiceLocator.Current.Register<UIManager>(new UIManager( ));
            //ServiceLocator.Current.Register<IMyGameServiceB>(new MyGameServiceB());
            //ServiceLocator.Current.Register<IMyGameServiceC>(new MyGameServiceC());

            // Application is ready to start, load your main scene.
            SceneManager.LoadSceneAsync("UIscene", LoadSceneMode.Additive);


        }
    }
}
