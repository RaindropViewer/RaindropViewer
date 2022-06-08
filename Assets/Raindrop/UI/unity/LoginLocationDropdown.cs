using System;
using System.Collections.Generic;
//using UnityEngine.UI;
using TMPro;
using UnityEngine;

namespace Raindrop.Presenters
{
    public class LoginLocationDropdown : MonoBehaviour
    {
        public TMP_Dropdown dropdown;
        List<TMP_Dropdown.OptionData> options => dropdown.options;

        public string selected => options[dropdown.value].text;
        public int value
        {
            get => dropdown.value;
            set => dropdown.value = value;
        }

        private void Awake()
        {
            string[] init = { "My Home", "My Last Location" }; //home, last, custom.
            setDropdownItems(new List<string>(init));
        }

        public void addLast(string item)
        {
            options.Add( new TMP_Dropdown.OptionData(item) );
        }

        public void setDropdownItems(List<string> items)
        {
            options.Clear();
            foreach (string item in items)
            {
                options.Add(new TMP_Dropdown.OptionData(item));
            }
        }
        public void clear()
        {
            options.Clear();
        }

        public void select(int id)
        {
            dropdown.value = id;
        }


    }
}