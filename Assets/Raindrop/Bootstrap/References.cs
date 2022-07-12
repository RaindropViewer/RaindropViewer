using Raindrop.Presenters;
using Raindrop.UI.LoadingScreen;
using Raindrop.UI.Views;
using UnityEngine;

namespace Raindrop.Bootstrap
{
    //this class is in the main scene, where you can give it references to monobehaviors in order to inject them.
    // of course, runtime- created monobehaviors cannot be injected this way, so you will need to find some other way for those.
    public class References : Singleton<References>
    {
        [SerializeField] public ModalsManager mm;
        [SerializeField] public ScreenStackManager sm;
        [SerializeField] public LoadingView ll;
        
        [SerializeField] public ChatPresenter chatPresenter;
        [SerializeField] public MapUIView mapUI;
        
    }
}