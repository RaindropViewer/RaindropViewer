using System;
using OpenMetaverse;
using Raindrop.Netcom;
using Raindrop.Services;
using UniRx;

namespace Raindrop.Presenters
{
    public class LoadingController : IDisposable
    {
        private LoadingPresenter view;
        private RaindropInstance instance { get { return ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>(); } }
        private RaindropNetcom netcom { get { return instance.Netcom; } }

        public bool isInteractable => view.canvas.interactable;
        
        public LoadingController(LoadingPresenter _view)
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
                    //view.loginButtonIsDisabled = false;
                    //Close_LoginModal_Slow();
                    
                    ServiceLocator.ServiceLocator.Instance.Get<UIService>().ScreensManager.PopAndPush(CanvasType.Game);
                    
                    view.FadeOut();
                    break;

                case LoginStatus.Failed: 
                    if (e.FailReason == "tos")
                    {
                        view.loginMsg.Value = "Must agree to Terms of Service before logging in" + Environment.NewLine;
                        view.enableCloseBtn();
                    }
                    else
                    {
                        view.loginMsg.Value = e.Message;
                        view.enableCloseBtn();
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