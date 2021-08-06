using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

//a monobehavior that sets and gives modals logic onclick..
public class modalPresenter : MonoBehaviour
{

    public TMP_Text titletext;
    public TMP_Text contenttext;
    public TMP_Text actionText;
    public GameObject modal;


    public Button CloseButton;
    public Button BackgroundButton;

    private void Start()
    {
        if (CloseButton!= null)
        {
            CloseButton.onClick.AsObservable().Subscribe(_ => closeModal()); //when clicked, runs this method.

        }
        if (BackgroundButton != null)
        {
            BackgroundButton.onClick.AsObservable().Subscribe(_ => closeModal()); //when clicked, runs this method.
        }
        if (titletext.GetComponent<TMP_Text>() == null)
        {
            Debug.LogError("titletext.GetComponent<TMP_Text>() failed");
        }
        if (contenttext.GetComponent<TMP_Text>() == null)
        {
            Debug.LogError("titletext.GetComponent<TMP_Text>() failed");
        }

    }

    //sets the textual contents of the ui
    public void setModal(string title, string content, string ActionText)
    {
        titletext.GetComponent<TMP_Text>().text = title;
        contenttext.GetComponent<TMP_Text>().text = content;
        if (actionText != null)
        {
            if (ActionText == "")
            {
                actionText.gameObject.SetActive(false);
            } else
            {
                actionText.GetComponent<TMP_Text>().text = ActionText;
                actionText.gameObject.SetActive(true);
            }
        }
        modal.SetActive(true);
    }
    public void setModal(string title, string content)
    {
        setModal(title, content, "");
    }

    public void closeModal()
    {
        modal.SetActive(false);
    }
}
