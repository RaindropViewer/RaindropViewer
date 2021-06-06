using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

//A very generic tab button. shoud use observer pattern to notify the 'parent' that the tab was cliked.
namespace Raindrop.Presenters
{

    [RequireComponent(typeof(Image))]
    public class TabButtonPresenter : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        public TabGroup tabGroup; //the 'parent' to notify when clicked..
        public UnityEvent onTabSelected;
        public UnityEvent onTabDeselected;

        //[HideInInspector]
        public Image background;

        void Start()
        {
            background = GetComponent<Image>();
            if (tabGroup != null)
                tabGroup.Subscribe(this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            tabGroup.OnTabSelected(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            tabGroup.OnTabEnter(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            tabGroup.OnTabExit(this);
        }

        public void Select()
        {
            if (onTabSelected != null)
            {
                onTabSelected.Invoke();
            }
        }

        public void Deselect()
        {
            if (onTabDeselected != null)
            {
                onTabSelected.Invoke();
            }

        }


    }
}
