using Raindrop.Presenters;
using Raindrop.UI.LoadingScreen;
using Raindrop.UI.Views;
using UnityEngine;

namespace Raindrop.Bootstrap
{
    //this monobehavior works like a control panel - you plug each major UI
    //component into each slot.
    // of course, monobehaviors created at runtime cannot be plugged-in
    // this way, so you will need to find some other way for those.
    public class References : MonoBehaviour
    {
        [SerializeField] public ModalsManager mm;
        [SerializeField] public ScreenStackManager sm;
        [SerializeField] public LoadingView ll;
        
        [SerializeField] public ChatPresenter chatPresenter;
        [SerializeField] public MapUIView mapUI;
        
    }
}