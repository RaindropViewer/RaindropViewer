using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


//a singleton monobehavior that activates modals.
//maintains reference to all modals.
public class ModalManager : MonoBehaviour
{
    Thread mainThread;
    //pool of modals.
    private modalPresenter genericModalPresenter;
    public modalPresenter eulaModal;
    public modalPresenter loggingInStatusModal;


    [SerializeField]
    public GameObject GenericModalPrefab;

    private void Awake()
    {
        //find yo modals in scene.
        //foreach(modalPresenter _ in FindObjectsOfType<modalPresenter>())
        //{

        //    genericModal.Add(_);
        //    _.closeModal();
        //}

        //if (FindObjectsOfType<modalPresenter>().Length != 0)
        //{
        //    genericModal = FindObjectsOfType<modalPresenter>()[0];
        //    genericModal.closeModal();
        //}


        GameObject GenericModal = Instantiate(GenericModalPrefab) as GameObject;
        GenericModal.transform.SetParent(this.transform);
        genericModalPresenter = GenericModal.GetComponent<modalPresenter>();

        if (genericModalPresenter == null)
        {
            Debug.LogError("cannot find the gneric modal");
        }

        mainThread = System.Threading.Thread.CurrentThread;
    }

    public void openEula()
    {
        setVisibleEulaModal(true);
    }
    
    public void closeEula()
    {
        setVisibleEulaModal(false);
    }

    public void setVisibleEulaModal(bool visibility)
    {
        if (isOnMainThread())
        {
            if (eulaModal != null)
            {
                eulaModal.gameObject.SetActive(visibility);
            }
            else
            {
                Debug.Log("unable to get the modalPresenter object!");
            }
        }
        else
        {

            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                Debug.Log(" dispatching of showing modal");
                setVisibleEulaModal(visibility);
            });
        }


    }
    public void setVisibleGenericModal(string title, string content, bool visibility)
    {
        if (isOnMainThread())
        {
            if (genericModalPresenter != null)
            {
                genericModalPresenter.setModal(title, content);
                genericModalPresenter.gameObject.SetActive(visibility);
            }
            else
            {
                Debug.Log("unable to get the modalPresenter object!");
            }
        } else
        {

            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                Debug.Log(" dispatching of showing modal");
                setVisibleGenericModal(title, content, visibility);
            });
        }


    }


    //by default obviously this must be visible; we are updating the login status.
    public void setVisibleLoggingInModal(string content)
    {
        string title = "Logging in status...";

        if (isOnMainThread())
        {
            if (loggingInStatusModal != null)
            {
                loggingInStatusModal.setModal(title, content);
                loggingInStatusModal.gameObject.SetActive(true);
            }
            else
            {
                Debug.Log("unable to get the modalPresenter object!");
            }
        }
        else
        {

            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                Debug.Log(" dispatching of showing modal");
                setVisibleLoggingInModal(content);
            });
        }


    }


    private bool isOnMainThread()
    {
        return mainThread.Equals(System.Threading.Thread.CurrentThread);
    }

    public void showSimpleModalBoxWithActionBtn(string title, string content, string Action)
    {
        if (genericModalPresenter != null)
        {
            genericModalPresenter.setModal(title, content, Action);
            genericModalPresenter.gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("unable to get the modalPresenter object!");
        }
    }
}
