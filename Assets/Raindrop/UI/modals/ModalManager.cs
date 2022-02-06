using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Serialization;

public class ModalManager : MonoBehaviour
{
    [Tooltip("The generic modal gameobject")]
    [SerializeField]
    public GameObject genericModalPrefab;
    
    Thread mainThread;
    [SerializeField]
    private ModalPresenter loginStatusModal;

    

    private void Awake()
    {
        CheckModals();

        mainThread = System.Threading.Thread.CurrentThread;
    }

    private void CheckModals()
    {
        if (loginStatusModal == null)
        {
            Debug.LogError("cannot find the login modal");
        }
        
    }
    // public void setVisibleGenericModal(string title, string content, bool visibility)
    // {
    //     if (isOnMainThread())
    //     {
    //         if (genericModalPresenter != null)
    //         {
    //             genericModalPresenter.setModalNoActions(title, content);
    //             genericModalPresenter.gameObject.SetActive(visibility);
    //         }
    //         else
    //         {
    //             Debug.LogWarning("unable to get the modalPresenter object!");
    //         }
    //     } else
    //     {
    //
    //         UnityMainThreadDispatcher.Instance().Enqueue(() => {
    //             // Debug.Log(" dispatching of showing modal");
    //             setVisibleGenericModal(title, content, visibility);
    //         });
    //     }
    //
    //
    // }



    private bool isOnMainThread()
    {
        return mainThread.Equals(System.Threading.Thread.CurrentThread);
    }

    // the default reply is "ok"
    public void showModalNotification(string title, string content /*, ModalType type*/)
    {
        if (!isOnMainThread())
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                    //Debug.Log(" dispatching of showing modal");
                    showModalNotification(title,content);
                });
        } else 
        {
            var newModal = Instantiate(genericModalPrefab);

            if (newModal != null)
            {
                var modalPresenter = newModal.GetComponent<ModalPresenter>();

                modalPresenter.setModal(
                    title,
                    content,
                    "Ok"
                );
                newModal.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("unable to get the modalPresenter object!");
            }
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

public enum ModalType
{
    Generic,
}
