using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Raindrop;

public class Global : MonoBehaviour
{
    //this static-new is like a globally accessible instance without a singleton! :)
    public static RaindropClient MainRaindropInstance = new RaindropClient();
    //this one manages the viewmodels and the stacking of UI modals.
    public static RaindropViewManager RaindropVM = new RaindropViewManager();


    public string app_data_Path { get; private set; }

    private void Awake()
    {
        //initialise your 'statics'

        //Get the path of the Game data folder
        app_data_Path = Application.persistentDataPath;

        //Output the Game data path to the console
        Debug.Log("dataPath : " + app_data_Path);

        MainRaindropInstance.setConfigPath(app_data_Path);
    }

}
