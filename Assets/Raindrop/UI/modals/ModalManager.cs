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
    public ModalPresenter genericModalPresenter;
    
    Thread mainThread;
    // private ModalPresenter eulaModalPresenter;
    [SerializeField]
    private ModalPresenter loginStatusModal;

    

    private void Awake()
    {
        CheckModals();

        mainThread = System.Threading.Thread.CurrentThread;
    }

    private void CheckModals()
    {
        if (genericModalPresenter == null)
        {
            Debug.LogError("cannot find the gneric modal");
        }
        if (loginStatusModal == null)
        {
            Debug.LogError("cannot find the login modal");
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
                Debug.LogWarning("unable to get the modalPresenter object!");
            }
        } else
        {

            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                // Debug.Log(" dispatching of showing modal");
                setVisibleGenericModal(title, content, visibility);
            });
        }


    }


    //by default obviously this must be visible; we are updating the login status.
    // public void setVisibleLoggingInModal(string content)
    // {
    //     string title = "Logging in status...";
    //
    //     if (isOnMainThread())
    //     {
    //         if (loginStatusModal != null)
    //         {
    //             loginStatusModal.setModalNoActions(title, content);
    //             loginStatusModal.gameObject.SetActive(true);
    //         }
    //         else
    //         {
    //             Debug.LogWarning("unable to get the modalPresenter object!");
    //         }
    //     }
    //     else
    //     {
    //
    //         UnityMainThreadDispatcher.Instance().Enqueue(() => {
    //             //Debug.Log(" dispatching of showing modal");
    //             setVisibleLoggingInModal(content);
    //         });
    //     }
    //
    //
    // }


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
            Debug.LogWarning("unable to get the modalPresenter object!");
        }
    }
    
    public void setLoginModalText(string title, string content)
    {
        if (loginStatusModal != null)
        {
            loginStatusModal.setModal(
                title,
                content, 
                "OK"
                );
            showLoginModal();
        }
        else
        {
            Debug.LogWarning("unable to get the loginStatusModal object!");
        }
    }

    private void showLoginModal()
    {
        if (loginStatusModal != null)
        {
            loginStatusModal.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("unable to get the login modal object!");
        }
    }

    public void fadeLoginModal()
    {
        if (loginStatusModal != null)
        {
            loginStatusModal.fadeaway();
        }
        else
        {
            Debug.LogWarning("unable to get the login modal object!");
        }
    }

}
