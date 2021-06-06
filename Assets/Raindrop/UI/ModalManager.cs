using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


//a singleton monobehavior that activates modals.
//maintains reference to all modals.
public class ModalManager : Singleton<ModalManager>
{
    Thread mainThread;
    //pool of modals.
    modalPresenter modalsAvailable;
    
    protected override void Awake()
    {
        base.Awake();
        //find yo modals in scene.
        if (FindObjectsOfType<modalPresenter>().Length != 0)
        {
            modalsAvailable = FindObjectsOfType<modalPresenter>()[0];
            modalsAvailable.closeModal();
        }

        mainThread = System.Threading.Thread.CurrentThread;
    }

    public void showModal(string title, string content)
    {
        if (isOnMainThread())
        {
            if (modalsAvailable != null)
            {
                modalsAvailable.setModal(title, content);
                modalsAvailable.gameObject.SetActive(true);
            }
            else
            {
                Debug.Log("unable to get the modalPresenter object!");
            }
        } else
        {

            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                Debug.Log(" dispatching of showing modal");
                showModal(title, content);
            });
        }


    }


    private bool isOnMainThread()
    {
        return mainThread.Equals(System.Threading.Thread.CurrentThread);
    }

    public void showSimpleModalBoxWithActionBtn(string title, string content, string Action)
    {
        if (modalsAvailable != null)
        {
            modalsAvailable.setModal(title, content, Action);
            modalsAvailable.gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("unable to get the modalPresenter object!");
        }
    }
}
