using NUnit.Framework;
using OpenMetaverse;
using Raindrop.Netcom;
using Raindrop.Presenters;
using System;
using System.Collections;
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
        private bool testfail = false;

        private RaindropNetcom netcom { get { return RaindropInstance.GlobalInstance.Netcom; } }
        private RaindropInstance instance { get { return RaindropInstance.GlobalInstance; } }

        [UnityTest]
        //login UI-backend-UI test
        public IEnumerator LoginTest() 
        {
            //RaindropClient MainRaindropInstance;

            //get the ref to scene 
            //GameObject gameGameObject =
            //   MonoBehaviour.Instantiate(Resources.Load<GameObject>("Prefabs/UIMocks"));
            //sub to callback
            netcom.ClientLoginStatus += new EventHandler<LoginProgressEventArgs>(netcom_ClientLoginStatus);


            yield return new WaitForSeconds(1);

            //get viewmodel to login.
            //getCurrentForeground gives us the loginVM, which we then call onloginbtnclick from.
            GameObject temp = (GameObject)instance.UI.getCurrentForegroundPresenter();
            if (temp == null)
            {

                testfail = true;
            }

            Assert.AreEqual(testfail, false); 

            if (temp.GetComponent<CanvasIdentifier>() != null)
            { //WHATTTTTT IS THIS FUCING SPAHGETTE
                temp.GetComponent<LoginPresenter>().Username = "***REMOVED*** Resident";
                temp.GetComponent<LoginPresenter>().Password = "***REMOVED***"; 
                temp.GetComponent<LoginPresenter>().OnLoginBtnClick();

            }
            else
            {
                testfail = true;
            }

            //Task.Run(async () =>
            //{
            //    await GetTestTaskAsync(5000);
            //}).GetAwaiter().GetResult();

             
            yield return new WaitForSeconds(5);

            Assert.AreEqual(loggedin, true);
            Assert.AreEqual(testfail, false);


            //make sure login was 


        }
        public async Task GetTestTaskAsync(int delay)
        {
            Debug.Log("1");
            await Task.Delay(TimeSpan.FromMilliseconds(delay));
            Debug.Log("2");
            //await Task.Run(async () => await Task.Delay(TimeSpan.FromSeconds(delay)));
            //Debug.Log("3");
            //await Task.Delay(TimeSpan.FromMilliseconds(200));
            //Debug.Log("4");
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
