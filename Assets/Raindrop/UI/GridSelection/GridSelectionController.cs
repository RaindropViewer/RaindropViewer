using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Raindrop.GridSelection
{
    public class GridSelectionController
    {
        //refernce to view-presenter
        private DropdownViewPresenter viewPresenter;
        private List<string> grids;

        //reference to model
        private RaindropInstance instance => 
            ServiceLocator.ServiceLocator.Instance.Get<RaindropInstance>();
        private bool Ready => (instance != null);
        private bool Connected => instance.Client.Network.Connected;

        public GridSelectionController(DropdownViewPresenter dropdownViewPresenter)
        // instead of injecting from the bootstrapper, we can also use getComponent<DropdownViewPresenter>
        {
            if (!Ready)
            {
                return;
            }

            List<Grid> gridMangerGrids = instance.GridManger.Grids;
            grids = GridList2StringList(gridMangerGrids);
            dropdownViewPresenter.ClearAndSetOptions(grids);
            
            dropdownViewPresenter.DropdownItemSelected += GridSelected;
        }

        private void GridSelected(object sender, DropdownPresenterEventArgs e)
        {
            int selectionIdx = e.DropdownValue;
            // update model:
            // LoginController.updateGrids = grids[selectionIdx];

        }

        private List<string> GridList2StringList(List<Grid> gridMangerGrids)
        {
            var res = gridMangerGrids.Select(e => e.Name).ToList();
            return res;
        }

        public int GetGridsCount()
        {
            return viewPresenter.GetOptionsCount();
        }
    }
}