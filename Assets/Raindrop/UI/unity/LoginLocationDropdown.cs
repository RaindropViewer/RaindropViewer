using System.Collections.Generic;
//using UnityEngine.UI;
using TMPro;

namespace Raindrop.Presenters
{
    internal class LoginLocationDropdown
    {
        TMP_Dropdown dropdown;
        List<TMP_Dropdown.OptionData> data;

        public string selected => data[dropdown.value].text;
        public int selectedId => dropdown.value;

        //List<string> items;
        public LoginLocationDropdown(TMP_Dropdown dd)
        {
            dropdown = dd;
            data = new List<TMP_Dropdown.OptionData>();
            string[] init = { "My Home", "My Last Location" };
            setDropdownItems(new List<string>(init));
            dropdown.options = data;
        }

        public void addLast(string item)
        {
            data.Add( new TMP_Dropdown.OptionData(item) );
        }

        public void setDropdownItems(List<string> items)
        {
            data.Clear();
            foreach (string item in items)
            {
                data.Add(new TMP_Dropdown.OptionData(item));
            }
        }
        public void clear()
        {
            data.Clear();
        }

        public void select(int id)
        {
            dropdown.value = id;
        }


    }
}