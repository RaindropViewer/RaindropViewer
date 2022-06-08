using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using File = System.IO.File;

[RequireComponent(typeof(TMP_Text))]
public class EULA_Printer : MonoBehaviour
{
    private string Eulatext => 
        System.IO.File.ReadAllText(
            Path.Combine(
                Disk.DirectoryHelpers.GetInternalStorageDir(),
                "RD_Eula.txt")
            )
        ;
    
    private void OnEnable()
    {
        var text = this.GetComponent<TMP_Text>();
        text.text = Eulatext;
    }
}
