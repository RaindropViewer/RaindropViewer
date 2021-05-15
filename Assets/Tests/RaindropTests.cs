using NUnit.Framework;
using OpenMetaverse;
using Raindrop.Netcom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;

namespace Raindrop.Tests
{
    [TestFixture()]
    public class RaindropTests
    {
        private bool loggedin;
        private RaindropNetcom netcom { get { return RaindropInstance.GlobalInstance.Netcom; } }

        [UnityTest]
        //login UI-backend-UI test
        public void LoginTest() 
        {
            //RaindropClient MainRaindropInstance;

            //get the ref to scene 
            GameObject gameGameObject =
               MonoBehaviour.Instantiate(Resources.Load<GameObject>("Prefabs/UIMocks"));
            //sub to callback
            netcom.ClientLoginStatus += new EventHandler<LoginProgressEventArgs>(netcom_ClientLoginStatus);


            //get viewmodel to login.
            //getCurrentForeground gives us the loginVM, which we then call onloginbtnclick from.
            LoginVM temp = (LoginVM)RaindropInstance.GlobalInstance.MainCanvas.getCurrentForegroundVM();
            temp.OnLoginBtnClick();
            //await Task.Delay(5000);

            //make sure login was 


        }
        private void netcom_ClientLoginStatus(object sender, LoginProgressEventArgs e)
        {
            if (e.Status == LoginStatus.Failed)
            {
                loggedin = false;
            }
            else if (e.Status == LoginStatus.Success)
            {

                loggedin = true;

            }
        }

    }





}
