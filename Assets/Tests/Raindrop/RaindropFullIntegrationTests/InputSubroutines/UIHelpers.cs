using Lean.Gui;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tests.Raindrop.RaindropFullIntegrationTests.InputSubroutines
{
    //static methods to help you click buttons!
    static internal class UIHelpers
    {
        public static bool Click_ButtonByUnityName(string gameObjectName)
        {
            var btn = GameObject.Find(gameObjectName);
            Assert.IsNotNull(btn, "Missing button supposed to have name: " + gameObjectName);

            if (btn.GetComponent<Button>())
            {
                btn.GetComponent<Button>().onClick.Invoke();
                return true;
            }
            if (btn.GetComponent<LeanButton>())
            {
                btn.GetComponent<LeanButton>().OnClick.Invoke();
                return true;
            }

            return false;
        }

        public static void Keyboard_TMPInputField_ByUnityName(string go_name, string input)
        {
            var go = GameObject.Find(go_name);
            Assert.True(go != null, "unable to find gameobject of name " + go_name);
            var tmp = go.GetComponent<TMP_InputField>();
            Assert.True(tmp != null, "Gameobject + " + go_name + " does not have TMP inputfield component ");
            tmp.ActivateInputField();
            tmp.text = input;
            tmp.onValueChanged.Invoke(input);
            tmp.onSubmit.Invoke(input);
        }

    }
}