using OpenMetaverse;
using OpenMetaverse.StructuredData;
using Raindrop;
using Raindrop.Netcom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using System;
using Settings = Raindrop.Settings;
using UnityEngine.UI;
using UniRx;
using TMPro;


//view(unitytext) -- presenter(this) -- controller(this?) -- model (raindropinstance singleton)

public class MapPresenter : MonoBehaviour
{

    private RaindropInstance instance { get { return RaindropInstance.GlobalInstance; } }
    private RaindropNetcom netcom { get { return instance.Netcom; } }
    bool Active => instance.Client.Network.Connected;


    #region references to UI elements
    public Button ChatButton;
    public Button MapButton;
    public Image MapPane;
    public TMP_Text locationText;

    public Canvas joyCanvas;

    #endregion

    #region internal representations 
      
     
    #endregion



     


    #region behavior

    // Use this for initialization
    void Start()
    {
        initialiseFields();

        ChatButton.onClick.AsObservable().Subscribe(_ => OnChatBtnClick()); //when clicked, runs this method.
        MapButton.onClick.AsObservable().Subscribe(_ => OnChatBtnClick()); //when clicked, runs this method.

    }



    private void showModal(string v, string message)
    {

        Debug.Log("MODAL:\n" + v + "\n" +message);
    }

 

    private void initialiseFields()
    {


    }

    public void OnChatBtnClick()
    {
        instance.MainCanvas.canvasManager.pushCanvas(CanvasType.Chat);


    }
    public void OnMapBtnClick()
    {
        instance.MainCanvas.canvasManager.pushCanvas(CanvasType.Map);


    }


    #endregion
     
     


     
}
