using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Raindrop.Presenters
{

    public class TabGroup : MonoBehaviour
    {
        // These must be 1 to 1, same order in hierarchy
        [HideInInspector]
        public List<TabButtonPresenter> tabButtons = new List<TabButtonPresenter>();
        public List<GameObject> tabPages = new List<GameObject>();

        //In case I need to sort the lists by GetSiblingIndex
        //objListOrder.Sort((x, y) => x.OrderDate.CompareTo(y.OrderDate));

        public Color tabIdleColor;
        public Color tabHoverColor;
        public Color tabSelectedColor;
        private TabButtonPresenter selectedTab;

        public void Start()
        {
            // Select first tab
            foreach (TabButtonPresenter tabButton in tabButtons)
            {
                if (tabButton.transform.GetSiblingIndex() == 0)
                    OnTabSelected(tabButton);
            }
        }

        public void Subscribe(TabButtonPresenter tabButton)
        {
            tabButtons.Add(tabButton);
            // Sort by order in hierarchy
            tabButtons.Sort((x, y) => x.transform.GetSiblingIndex().CompareTo(y.transform.GetSiblingIndex()));
        }

        public void OnTabEnter(TabButtonPresenter tabButton)
        {
            ResetTabs();
            if ((selectedTab == null) || (tabButton != selectedTab))
                tabButton.background.color = tabHoverColor;
        }

        public void OnTabExit(TabButtonPresenter tabButton)
        {
            ResetTabs();
        }

        public void OnTabSelected(TabButtonPresenter tabButton)
        {
            if (selectedTab != null)
            {
                selectedTab.Deselect();
            }

            selectedTab = tabButton;

            selectedTab.Select();

            ResetTabs();
            tabButton.background.color = tabSelectedColor;
            int index = tabButton.transform.GetSiblingIndex();
            for (int i = 0; i < tabPages.Count; i++)
            {
                if (i == index)
                {
                    tabPages[i].SetActive(true);
                }
                else
                {
                    tabPages[i].SetActive(false);
                }
            }
        }

        public void ResetTabs()
        {
            foreach (TabButtonPresenter tabButton in tabButtons)
            {
                if ((selectedTab != null) && (tabButton == selectedTab))
                    continue;
                tabButton.background.color = tabIdleColor;
            }
        }

        public void NextTab()
        {
            int currentIndex = selectedTab.transform.GetSiblingIndex();
            int nextIndex = currentIndex < tabButtons.Count - 1 ? currentIndex + 1 : tabButtons.Count - 1;
            OnTabSelected(tabButtons[nextIndex]);
        }

        public void PreviousTab()
        {
            int currentIndex = selectedTab.transform.GetSiblingIndex();
            int previousIndex = currentIndex > 0 ? currentIndex - 1 : 0;
            OnTabSelected(tabButtons[previousIndex]);
        }
    }
}
