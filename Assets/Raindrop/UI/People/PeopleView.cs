using System;
using System.Collections.Generic;
using OpenMetaverse;
using Raindrop;
using Raindrop.ServiceLocator;
using Raindrop.Services;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

//renders a UI that contains a user's name, image.
public class PeopleView : MonoBehaviour, IPointerClickHandler
{
    public GridClient client => Instance.Client;
    public RaindropInstance Instance => ServiceLocator.Instance.Get<RaindropInstance>();

    
    [FormerlySerializedAs("userID")] public UUID agentID;
    
    [FormerlySerializedAs("textview")] public TextView TextView;
    [FormerlySerializedAs("image")] public Texturable_RawImage ImageView;
    public TextView Distance;
    private bool bReady = false; //if this view is initialised.
    // [SerializeField] private OnlineView OnlineView;

    private AddAChatView callback;

    public void Init(UUID userID, string name, UUID imageID, bool isOnline, int distance, AddAChatView chosenCallback)
    {
        this.agentID = userID;
        TextView.setText(name);
        ImageView.SetImageID(imageID);
        // OnlineView.SetOnline(isOnline);
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