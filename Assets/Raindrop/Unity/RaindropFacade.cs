//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using Raindrop;

////this contains functions that help UnityUI directly call the internal functions. just like a facade.
//public class RaindropFacade : MonoBehaviour
//{
//    //public GameObject EULApanel;
//    //public CanvasManager CanvasManagerRef;

//    //this static-new is like a globally accessible instance without a singleton! :)
//    public RaindropInstance MainRaindropInstance;
//    //this one manages the viewmodels and the stacking of UI modals.
//    //public MonoViewModel RaindropVM;

//    public void UIShowEULAPanel()
//    {
//        MainRaindropInstance.MainCanvas.canvasManager.pushCanvas(CanvasType.EULA);
//    }
//    public void UIPushPanel(CanvasType ct)
//    {
//        MainRaindropInstance.MainCanvas.canvasManager.pushCanvas(ct);
//    }

//    public void UIPopTopPanel()
//    {
//        MainRaindropInstance.MainCanvas.canvasManager.popCanvas();
//    }

//    public string app_data_Path { get; private set; }

//    private void Awake()
//    {

//        MainRaindropInstance = RaindropInstance.GlobalInstance;
//    }

//}
