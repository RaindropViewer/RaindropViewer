using OpenMetaverse;
using Plugins.CommonDependencies;
using Raindrop;
using Raindrop.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

//a UI that contains a user's name, image.
// usage: 
// Set: Set(person UUID)
public class PeopleView : MonoBehaviour, IPointerClickHandler
{
    public GridClient client => Instance.Client;
    public RaindropInstance Instance => RaindropInstance.GlobalInstance;

    
    [SerializeField] public UUID agentID;
    
    [FormerlySerializedAs("textview")] public TextView TextView;
    [FormerlySerializedAs("image")] public Texturable_RawImage ImageView;
    public TextView Distance;
    private bool bReady = false; //if this view is initialised.

    private AddAChatView callback;

    public void Init(UUID userID, string name, UUID imageID, bool isOnline, int distance, AddAChatView chosenCallback)
    {
        agentID = userID;
        TextView.setText(name);
        ImageView.SetImageID(imageID);
        Distance.setText_DistanceInt(distance);

        callback = chosenCallback;
        
        bReady = true;
    }

    // on click, we open the chat with this agent
    public void OnPointerClick(PointerEventData eventData)
    {
        if (bReady == false)
        {
            return;
        }
        //raise the event of 'wish to talk to this user'
        var UI = ServiceLocator.Instance.Get<UIService>();
        UI.chatFacade.AddIMTab(agentID, client.Self.AgentID ^ agentID, TextView.getText());
        
        //todo:close the modal in a less retarded way.
        callback.CloseModal();
    }

}