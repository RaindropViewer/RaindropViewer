using UnityEngine;

namespace Raindrop.GridSelection
{
    [RequireComponent(typeof(DropdownViewPresenter))]
    public class GridSelectionBootstrapper : MonoBehaviour
    {
        GridSelectionController Controller;
        private DropdownViewPresenter presenter;
        
        void Start() //take care to only link up dependencies on start.
        {
            presenter = this.GetComponent<DropdownViewPresenter>();
            //creates a controller. injects the presenter reference to the controller.
            Controller = new GridSelectionController(presenter);

            // set up all the options from model:
            
        }
    
    }
}