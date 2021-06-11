using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Raindrop
{
    class UIBootstrapper : MonoBehaviour
    {
        private void Awake()
        {
            ServiceLocatorSample.ServiceLocator.ServiceLocator.Current.Get<UIManager>().initialiseUI();
        }


    }
}
