using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Raindrop;

public class MainEntryPoint : MonoBehaviour
{
    //public GameObject CanvasManagerObject;
    //public CanvasManager CanvasManagerRef;

    //this static-new is like a globally accessible instance without a singleton! :)
    public RaindropInstance MainRaindropInstance;
    //this one manages the viewmodels and the stacking of UI modals.
    //public MonoViewModel RaindropVM;


    public string app_data_Path { get; private set; }

    private void Awake()
    {

        MainRaindropInstance = RaindropInstance.GlobalInstance; 
    }

}
