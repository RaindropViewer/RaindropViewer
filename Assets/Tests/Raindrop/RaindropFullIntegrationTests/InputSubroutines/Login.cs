using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Tests.Raindrop.RaindropFullIntegrationTests.InputSubroutines
{
    // Subroutines to perform login. (in full integration mode.)
    public class Login
    {
        // type user creds and click the login button.
        public static IEnumerator StartLogin(string username, string password)
        {
            TypeUserAndPassIntoLoginPanel(username, password);
            yield return new WaitForSeconds(2);
            UIHelpers.Click_ButtonByUnityName("LoginBtn");
            yield return new WaitForSeconds(12);
        }

        public static void TypeUserAndPassIntoLoginPanel(string _username, string _password)
        {
            UIHelpers.Keyboard_TMPInputField_ByUnityName("UserTextField", _username);
            UIHelpers.Keyboard_TMPInputField_ByUnityName("PassTextField", _password);
        }
        
        
        //accept the eula:
        /* 1. toggle true
         * 2. click "next" btn
         */

        // on the welcome screen, click the go button.
        public static IEnumerator accepttheeula()
        {
            string eulaCheckbox = "AgreeToggle";
            var checkboxEULA = GameObject.Find(eulaCheckbox);
            Assert.IsNotNull(checkboxEULA, "Missing checkbox " + eulaCheckbox);
            checkboxEULA.GetComponent<Toggle>().onValueChanged.Invoke(true);
            yield return new WaitForSeconds(2);
            
            string eulaCloseBtn = "NextButton";
            Assert.IsTrue(UIHelpers.Click_ButtonByUnityName(eulaCloseBtn));
            yield return new WaitForSeconds(2);
        }

        
    }
}