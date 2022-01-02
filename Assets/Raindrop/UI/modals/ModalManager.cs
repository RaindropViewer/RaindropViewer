using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Serialization;


//a singleton monobehavior that activates modals.
//maintains reference to all modals.
public class ModalManager : MonoBehaviour
{
    [Header("Provide a reference to your modals:")]
    
    [Tooltip("The generic modal gameobject")]
    [SerializeField]
    public GameObject GenericModal;
    // [SerializeField]
    // public GameObject EulaModal;
    [SerializeField]
    public GameObject LoginStatusModal;
    
    Thread mainThread;
    //pool of modals.
    private ModalPresenter genericModalPresenter;
    // private ModalPresenter eulaModalPresenter;
    private ModalPresenter loginStatusModal;



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
        
        linkModals();

        // GameObject GenericModal = Instantiate(GenericModalPrefab) as GameObject;
        // GenericModal.transform.SetParent(this.transform);
        // modalPresenter = GenericModal.GetComponent<modalPresenter>();


        mainThread = System.Threading.Thread.CurrentThread;
    }

    private void linkModals()
    {


        genericModalPresenter = GenericModal.GetComponent<ModalPresenter>();
        if (genericModalPresenter == null)
        {
            Debug.LogError("cannot find the gneric modal");
        }
        
    }
    //
    // public void openEula()
    // {
    //     setVisibleEulaModal(true);
    // }
    //
    // public void closeEula()
    // {
    //     setVisibleEulaModal(false);
    // }
    
    //
    // public void setVisibleEulaModal(bool visibility)
    // {
    //     if (isOnMainThread())
    //     {
    //         if (eulaModalPresenter != null)
    //         {
    //             eulaModalPresenter.gameObject.SetActive(visibility);
    //         }
    //         else
    //         {
    //             Debug.Log("unable to get the modalPresenter object!");
    //         }
    //     }
    //     else
    //     {
    //
    //         UnityMainThreadDispatcher.Instance().Enqueue(() => {
    //             Debug.Log(" dispatching of showing modal");
    //             setVisibleEulaModal(visibility);
    //         });
    //     }
    //
    //
    // }
    public void setVisibleGenericModal(string title, string content, bool visibility)
    {
        if (isOnMainThread())
        {
            if (genericModalPresenter != null)
            {
                genericModalPresenter.setModalNoActions(title, content);
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
            if (loginStatusModal != null)
            {
                loginStatusModal.setModalNoActions(title, content);
                loginStatusModal.gameObject.SetActive(true);
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

    public void showModalNotification(string title, string content)
    {
        if (genericModalPresenter != null)
        {
            genericModalPresenter.setModal(
                title,
                content, 
                "Ok"
                );
            genericModalPresenter.gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("unable to get the modalPresenter object!");
        }
    }
}
