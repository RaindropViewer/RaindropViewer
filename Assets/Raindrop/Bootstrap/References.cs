using System.Collections.Generic;
using Raindrop.Presenters;
using Raindrop.UI.LoadingScreen;
using UnityEngine;

namespace Raindrop.Services.Bootstrap
{
    //this class is in the main scene, where you can give it references to monobehaviors in order to inject them.
    // of course, runtime- created monobehaviors cannot be injected this way, so you will need to find some other way for those.
    public class References : Singleton<References>
    {
        [SerializeField] public ModalsManager mm;
        [SerializeField] public ScreenStackManager sm;
        [SerializeField] public LoadingView ll;
        
        [SerializeField] public ChatPresenter chatPresenter;
        
    }
}