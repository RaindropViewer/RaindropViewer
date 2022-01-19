using System.Collections;
using System.Collections.Generic;
using OpenMetaverse;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LandmarkView : MonoBehaviour
{
    public Image image;
    public TMP_Text parcelName;
    public TMP_Text simName;
    public TMP_Text localCoords;
    public TMP_Text parcelDesc;

    private LandmarkPresenter presenter;
    
    // Start is called before the first frame update
    void Start()
    {
        Debug.LogError("landmarkclasses: this class is not ready for use!");
        
        //todo : specify uuid.
        var invLMrequested= new InventoryLandmark(UUID.Zero);
        LandmarkPresenter presenter = new LandmarkPresenter(this, invLMrequested);
        
    }

    // Update is called once per frame
    void Update()
    {
        PollPresenterAndUpdate();
    }

    // access the modified bool in presenter, if it is true, we need to update ourselves.
    private void PollPresenterAndUpdate()
    {
        lock (presenter.mutex)
        {
            if (presenter.modified == false)
            {
                return;
            }
    
            //update ui.
            parcelName.text = presenter.parcelName;
            presenter.modified = false;
        }
        
    }
}