using System.Collections;
using System.Collections.Generic;
using Plugins.CommonDependencies;
using Raindrop;
using UnityEngine;

// show the currently-select grid name.
public class GridNameView : MonoBehaviour
{
    private RaindropInstance instance => RaindropInstance.GlobalInstance;

    public TextView TextView;

    void OnEnable()
    {
        if (TextView != null)
        {
            TextView.setText(instance.Netcom.Grid.Name);
        }
    }
}
