using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Raindrop.Presenters
{
    //all stack-based UI windows/views inherit this.
    public class BasePresenter : MonoBehaviour
    {
        private void Start()
        {
            
        }

        public void registerWithUIService()
        {
            ServiceLocator.ServiceLocator.Instance.Get<UIService>();
        }

    }
}
