using System.Collections;
using System.Collections.Generic;
using OpenMetaverse;
using OpenMetaverse.Stats;
using Raindrop.Presenters;
using Raindrop.ServiceLocator;
using Raindrop.Services;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Logger = OpenMetaverse.Logger;

[RequireComponent(typeof(Button))]
public class ButtonTriggerModal : MonoBehaviour
{
    private Button button;
    [SerializeField]
    public GameObject modal;
    // Start is called before the first frame update
    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonDown);
    }

    private void OnButtonDown()
    {
        if (modal == null)
        {
            Logger.Log("modal not implemented/link", Helpers.LogLevel.Error);
            ModalsManager.PushModal_NotImplementedYet(this.gameObject.name.ToString() + " modal");
            return;
        }
        ServiceLocator.Instance.Get<UIService>().ModalsManager.Show(modal);
    }
}
