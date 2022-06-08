using System.ComponentModel;
using Lean.Gui;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Raindrop.Tests.RaindropFullIntegrationTests.InputSubroutines
{
    //static methods to help you click buttons!
    static internal class UIHelpers
    {
        #region Clicking primitives

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
        
        
        // just click it.
        // WARN: only support TMP Dropdowns
        public static TMP_Dropdown Click_Dropdown_ByUnityName(string gameObjectName)
        {
            var dd = GameObject.Find(gameObjectName);
            Assert.IsNotNull(dd, "Missing dropdown supposed to have name: " + gameObjectName);

            if (dd.GetComponent<TMP_Dropdown>())
            {
                TMP_Dropdown dd_class = dd.GetComponent<TMP_Dropdown>();
                dd_class.OnPointerClick(null);
                // dd.GetComponent<TMP_Dropdown>().OnClick.Invoke();
                return dd_class;
            }

            return null;
        }
        
        // when the dropdown is open, click one of the entrues by the name.
        // WARN: Dropdown entry Names to be unique.
        // return: is successful
        public static bool Click_DropdownEntry_ByName(TMP_Dropdown dropdown, string targetDropdownEntry)
        {
            //find index of the target in the dropdown list
            var options = dropdown.options;
            int newIdx = -1; 
            for (int i = 0; i <  options.Count; i++)
            {
                if (options[i].text == targetDropdownEntry)
                {
                    newIdx = i;
                    break;
                }
            }

            if (newIdx == -1)
            {
                // no value found.
                return false;
            }

            dropdown.value = newIdx;
            return true;
        }

        private static string printParentTree(Transform tf)
        {
            string res = tf.name + "\n";
            while (tf.parent != null)
            {
                tf = tf.parent;
                res += tf.name + "\n";
            }

            return res;
        }
        private static string printChildTree(Transform tf, int nestingLevel)
        {
            string left_empty_space = "--";

            string res = "";

            //print self at the correct nest level.
            while (nestingLevel != 0)
            {
                res += left_empty_space;
                nestingLevel--;
            }
            res += tf.name + "\n";
            
            //call recursive funcion on each child...
            // base case: no children - skip this loop
            foreach (Transform child in tf)
            {
                res += printChildTree(child, nestingLevel + 1);
            }

            return res;
        }

        //recusive finding by TMP_Text content.
        private static TMP_Text Find_TMPText_ByContent(
            string target_string,
            Transform root_tf)
        {
            
            //1. call on oneself and return if is found.
            var val = root_tf.GetComponent<TMP_Text>();
            if (val != null)
            {
                //handle special case: current selection is already the target we want to select. //todo
                if (val.gameObject.name == "Label")
                {
                    return null;
                }
                
                //check TMPtext value:
                if (val.text == target_string)
                {
                   return val;
                }    
            }
            
            //2. call recurse to children.
            foreach (Transform child in root_tf)
            {
                var childresult = Find_TMPText_ByContent(target_string, child);
                if (childresult != null)
                {
                    return childresult;
                }
            }
            
            //3. unable to find at all.
            return null;
        }

        #endregion


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