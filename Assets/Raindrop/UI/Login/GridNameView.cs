using System.Collections;
using System.Collections.Generic;
using Raindrop;
using Raindrop.ServiceLocator;
using UnityEngine;

// show the currently-select grid name.
public class GridNameView : MonoBehaviour
{
    private RaindropInstance instance => ServiceLocator.Instance.Get<RaindropInstance>();

    public TextView TextView;

    void OnEnable()
    {
        if (TextView != null)
        {
            TextView.setText(instance.Netcom.Grid.Name);
        }
    }
}
