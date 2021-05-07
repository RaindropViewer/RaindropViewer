using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Raindrop
{
    public enum CanvasType
    {
        Login,
        Game
    }

    public class RaindropViewManager
    {
        public CanvasManager cm;

        //manages all child Viewmodels
        public RaindropViewManager()
        {
            Debug.Log("VM manager setup ok");

            //subscribe all events from raindropclient
            Global.MainRaindropInstance.LoginCompleted += MainRaindropInstance_LoginCompleted;
            Global.MainRaindropInstance.LoginFailed += MainRaindropInstance_LoginFailed;

        }

        //register the canvas manager which is in a GO, who has the responsibility of flipping pages.
        public void registerWithRaindropClient(CanvasManager canvasManager)
        {
            cm = canvasManager;
            //start up the initial login panel
            pushLoginView();
        }
        private void pushLoginView()
        {
            cm.pushCanvas(CanvasType.Login);
        }

        private void MainRaindropInstance_LoginFailed()
        {
            showFailedLoginModal();
        }

        private void showFailedLoginModal()
        {
            throw new NotImplementedException();
        }

        private void MainRaindropInstance_LoginCompleted()
        {
            cm.popCanvas();
            showSuccessLoginModal();
        }

        private void removeLoginView()
        {
            throw new NotImplementedException();
        }

        private void showSuccessLoginModal()
        {
            throw new NotImplementedException();
        }
    }
}
