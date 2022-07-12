using System.Collections.Generic;
using Plugins.CommonDependencies;
using Raindrop;
using Raindrop.Services;
using UnityEngine;
using UnityEngine.Serialization;
using Object = System.Object;

public class ModalsManager : MonoBehaviour
{
    [Tooltip("The notification-generic modal prefab")]
    [SerializeField]
    public GameObject genericModalPrefab;
    
    [Tooltip("The connectivity modal prefab")]
    [SerializeField]
    public GameObject connectivityModalPrefab;

    [FormerlySerializedAs("loginStatusModal")]
    [Tooltip("The disconnected/login failure modal prefab")]
    [SerializeField]
    private GenericModalPresenter loginStatusGenericModal;

    [Tooltip("The root transform to put these instantiated modals under")]
    [SerializeField] public Transform ModalRoot;
    
    // [Tooltip("a list of modals being shown ")]
    // [SerializeField] private Dictionary<int, GameObject> OpenModals = new Dictionary<int, GameObject>();
    
    public void Init()
    {
        CheckModals();

    }

    private void CheckModals()
    {
        if (loginStatusGenericModal == null)
        {
            Debug.LogError("cannot find the login modal");
        }
        
    }


    // shows a generic modal, where the only button is "ok" - does nothing.
    public void showModal_NotificationGeneric(string title, string content /*, ModalType type*/)
    {
        if (! UnityMainThreadDispatcher.isOnMainThread())
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                    //Debug.Log(" dispatching of showing modal");
                    showModal_NotificationGeneric(title,content);
                });
        } 
        else 
        {
            var newModal = Instantiate(genericModalPrefab, ModalRoot);

            if (newModal != null)
            {
                var modalPresenter = newModal.GetComponent<GenericModalPresenter>();

                modalPresenter.SetModal(
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
    //
    // public void setLoginModalText(string title, string content)
    // {
    //     if (loginStatusGenericModal != null)
    //     {
    //         loginStatusGenericModal.SetModal(
    //             title,
    //             content, 
    //             "OK"
    //             );
    //         showLoginModal();
    //     }
    //     else
    //     {
    //         Debug.LogWarning("unable to get the loginStatusModal object!");
    //     }
    // }
    //
    // private void showLoginModal()
    // {
    //     if (loginStatusGenericModal != null)
    //     {
    //         loginStatusGenericModal.gameObject.SetActive(true);
    //     }
    //     else
    //     {
    //         Debug.LogWarning("unable to get the login modal object!");
    //     }
    // }


    // show a UI modal. you are free to specify the custom modals via their prefabs. 
    public void Show(GameObject modal)
    {
        var GO_ref = Instantiate(modal, this.transform);
        var ref_id = GO_ref.GetInstanceID();
        // OpenModals.Add(ref_id, GO_ref);
    }
    
    //warn: to be used, by the modal closing itself.
    public void UnShow(GameObject modal)
    {
        var ref_id = modal.GetInstanceID();
        // OpenModals.Remove(ref_id);
        Destroy(modal);
    }

    //shows a generic-notification modal, where the message is "feature not implemented".
    public static void PushModal_NotImplementedYet(Object type)
    {
        var ui = ServiceLocator.Instance.Get<UIService>();
        ui.ModalsManager.showModal_NotificationGeneric(
            "The desired feature is not implemented yet: ",
            type.ToString() + " UI + \n \n + Stay tuned for updates!");
        var instance = ServiceLocator.Instance.Get<RaindropInstance>();
        instance.MediaManager.PlayUISound(UISounds.Warning);
    }
}

public enum ModalType
{
    Generic,
}
