using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameframe.GUI.PanelSystem.Demo
{
    public class DemoPanelViewController : PanelViewControllerBehaviour<DemoPanelView>
    {
        [Multiline] 
        public string infoText = "Some info text from the panel view controller";
        
        protected override void ViewDidLoad()
        {
            base.ViewDidLoad();
            LogEvent($"ViewDidLoad [active{gameObject.activeSelf}]");
        }

        protected override void ViewWillAppear()
        {
            base.ViewWillAppear();
            LogEvent($"ViewWillAppear [active{gameObject.activeSelf}]");
            View.infoLabel.text = infoText;
        }

        protected override void ViewDidAppear()
        {
            base.ViewDidAppear();
            LogEvent($"ViewDidAppear [active:{gameObject.activeSelf}]");
        }

        protected override void ViewWillDisappear()
        {
            base.ViewWillDisappear();
            LogEvent($"ViewWillDisappear [active:{gameObject.activeSelf}]");
        }

        protected override void ViewDidDisappear()
        {
            base.ViewDidAppear();
            LogEvent($"ViewDidDisappear [active:{gameObject.activeSelf}]");
        }
        
        private void LogEvent(string msg)
        {
            Debug.LogFormat("DemoPanelViewController: {0}",msg);
        }
    }   
}


