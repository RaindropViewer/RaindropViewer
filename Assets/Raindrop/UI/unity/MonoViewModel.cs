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

    public class MonoViewModel
    {
        public MonoViewController rvc;




        public CanvasManager canvasManager;

        //manages all child Viewmodels
        public MonoViewModel(RaindropInstance instance)
        {

            //Debug.Log("VM manager setup ok");

            //subscribe all events from raindropclient
            //Client.LoginCompleted += MainRaindropInstance_LoginCompleted;
            //Client.LoginFailed += MainRaindropInstance_LoginFailed;

        }

        //register the canvas manager which is in a GO, who has the responsibility of flipping pages.
        //public void registerWithRaindropClient(CanvasManager canvasManager)
        //{
        //    this.canvasManager = canvasManager;
        //    //start up the initial login panel
        //    pushLoginView();
        //}
        private void pushLoginView()
        {
            canvasManager.pushCanvas(CanvasType.Login);
        }

        //private void MainRaindropInstance_LoginFailed()
        //{
        //    Debug.Log("failed login TODO: why?");
        //}

        private void showFailedLoginModal()
        {
            throw new NotImplementedException();
        }

        //private void MainRaindropInstance_LoginCompleted()
        //{
        //    cm.popCanvas();//this lince causes error, as the function was called from the login thread!
        //    cm.pushCanvas(CanvasType.Game);
        //    //showSuccessLoginModal();
        //}

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
