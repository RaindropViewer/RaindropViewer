using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Raindrop;

public class FakeGlobal : MonoBehaviour
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

        //initialise your 'statics'

        //Get the path of the Game data folder
        //app_data_Path = Application.persistentDataPath;

        //Output the Game data path to the console
        //Debug.Log("dataPath : " + app_data_Path);

        //raindropinstance seems tightly coupled with the applicaiton lifecycle; it seems essentially a subset of the app lifecycle in fact. 
        //make sure you supply the app directory.
        MainRaindropInstance = RaindropInstance.GlobalInstance; 
        //RaindropInstance.GlobalInstance.setAppDataDir(app_data_Path);

        //RaindropVM = new MonoViewModel(MainRaindropInstance);
    }

}
