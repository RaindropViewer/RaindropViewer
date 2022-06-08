using Raindrop;
using Plugins.CommonDependencies;
using Raindrop.Services;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

//a monobehavior that makes a toggle toggle the eula acceptance in globalSettings 
public class EulaView : MonoBehaviour
{
    private RaindropInstance instance { get { return ServiceLocator.Instance.Get<RaindropInstance>(); } }
    
    [Tooltip("the Acceptance toggle that allows us to continue past this window")]
    public Toggle EulaToggle;
    
    [FormerlySerializedAs("closeBtn")] public Button NextBtn;

    private void Start()
    {
        FindAndLinkUIComponents();
    }

    //link all children UI components to the reactive events.
    private void FindAndLinkUIComponents()
    {
        if (EulaToggle == null)
        {
            Debug.LogWarning("eula toggle UI is not present.");
        }
        if (EulaToggle != null)
        {
            EulaToggle.onValueChanged.AsObservable().Subscribe(_ => onToggleChanged(_)); //when clicked, runs this method.
        }

        //initialise button/toggle state
        bool isAcceptedEULA = instance.GlobalSettings["EulaAccepted"];
        EulaToggle.isOn = isAcceptedEULA;
        onToggleChanged(isAcceptedEULA);
        
        NextBtn.onClick.AddListener(closeEula);
    }

    private void closeEula()
    {
        if (instance.GlobalSettings["EulaAccepted"] == null)
            return;
        if (instance.GlobalSettings["EulaAccepted"] == false)
            return;
        
        var ui = ServiceLocator.Instance.Get<UIService>();
        ui.ResetToInitialScreen();
    }

    private void onToggleChanged(bool isEulaAccepted)
    {
        instance.GlobalSettings["EulaAccepted"] = isEulaAccepted;
        
        if (isEulaAccepted)
        {
            NextBtn.gameObject.SetActive(true);
            return;
        }
        else
        {
            NextBtn.gameObject.SetActive(false);
        }
        
    }
     
}
