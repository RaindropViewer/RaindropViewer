using Raindrop.Netcom;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Raindrop.Presenters
{
    //attach this directly to the dropdown on your login screen :)

    [RequireComponent(typeof(Dropdown))]
    public class GenericDropdown : MonoBehaviour
    {

        private RaindropInstance instance { get { return RaindropInstance.GlobalInstance; } }
        private RaindropNetcom netcom { get { return instance.Netcom; } }


        void Start()
        {

            this.clear();
            this.set(instance.GridManger.Grids);
            this.Add("Custom");

            dd = this.gameObject.GetComponent<Dropdown>();
            dd.onValueChanged.AddListener(delegate {
                DropdownValueChanged(dd);
            });

        }

        private void DropdownValueChanged(Dropdown dd)
        { 
            SelectedIndex = dd.value;
            SelectedItem = instance.GridManger.Grids[SelectedIndex];

            netcom.LoginOptions.Grid = instance.GridManger.Grids[SelectedIndex];

        }

        private List<string> gridDropdownOptions = new List<string> //type is grid
        {
        };
          
        public Dropdown dd;
        public  int SelectedIndex;
        public Grid SelectedItem;

        public void clear ()
        {
            gridDropdownOptions.Clear();
            dd.ClearOptions();
        }

        public void set<T>(List<T> grids)
        {
            foreach(var _ in grids)
            {
                gridDropdownOptions.Add(_.ToString());
            }
            dd.AddOptions(gridDropdownOptions);

        }

        internal void Add(string v)
        {
            gridDropdownOptions.Add(v);

            dd.AddOptions(gridDropdownOptions);
        }
         
    }


}