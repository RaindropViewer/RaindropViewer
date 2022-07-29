using System;
using OpenMetaverse;
using Plugins.CommonDependencies;
using Raindrop.Netcom;
using Raindrop.Services;

namespace Raindrop.UI.LoadingScreen
{
    public class LoadingController : IDisposable
    {
        private LoadingView view;
        private RaindropInstance instance { get { return RaindropInstance.GlobalInstance; } }
        private RaindropNetcom netcom { get { return instance.Netcom; } }

        public bool isInteractable => view.canvas.interactable;
        
        public LoadingController(LoadingView _view)
        {
            view = _view;

            AddLoginEvents();
        }

        public void Dispose()
        {
            RemoveLoginEvents();
        }

        private void AddLoginEvents()
        {
            netcom.ClientLoggingIn += new EventHandler<OverrideEventArgs>(netcom_ClientLoggingIn);
            netcom.ClientLoginStatus += new EventHandler<LoginProgressEventArgs>(netcom_ClientLoginStatus);
            
            // netcom.Teleporting
        }

        private void RemoveLoginEvents()
        {
            if (netcom == null)
                return;
            netcom.ClientLoggingIn -= new EventHandler<OverrideEventArgs>(netcom_ClientLoggingIn);
            netcom.ClientLoginStatus -= new EventHandler<LoginProgressEventArgs>(netcom_ClientLoginStatus);
        }

        
        public void netcom_ClientLoginStatus(object sender, LoginProgressEventArgs e)
        {
            switch (e.Status)
            {
                case LoginStatus.ConnectingToLogin:
                    view.loginMsg.Value += "Connecting to login server..." + Environment.NewLine;
                    break;

                case LoginStatus.ConnectingToSim:
                    view.loginMsg.Value += ("Connecting to region..." + Environment.NewLine);
                    break;

                case LoginStatus.Redirecting:
                    view.loginMsg.Value += "Redirecting..." + Environment.NewLine;
                    break;

                case LoginStatus.ReadingResponse:
                    view.loginMsg.Value += "Reading response..." + Environment.NewLine;
                    break;

                case LoginStatus.Success:
                    view.loginMsg.Value += "Logged in as " + netcom.LoginOptions.FullName + Environment.NewLine;
                    
                    ServiceLocator.Instance.Get<UIService>().PopAndPush(CanvasType.Game);
                    
                    view.FadeOut();
                    break;

                case LoginStatus.Failed: 
                    if (e.FailReason == "tos")
                    {
                        view.loginMsg.Value = "Must agree to Terms of Service before logging in" + Environment.NewLine;
                        view.appearCloseBtn();
                    }
                    else
                    {
                        view.loginMsg.Value = e.Message;
                        view.appearCloseBtn();
                    }
                    break;
            }
        }
        
        public void netcom_ClientLoggingIn(object sender, OverrideEventArgs e)
        {
            view.FadeIn();
            
            view.loginMsg.Value += "Start to Logging in...";
            //view.loginButtonIsDisabled = true;
        }
    }
}