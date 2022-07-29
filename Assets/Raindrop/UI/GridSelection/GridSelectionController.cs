using System.Collections.Generic;
using System.Linq;
using OpenMetaverse;
using Plugins.CommonDependencies;
using UnityEngine;

namespace Raindrop.GridSelection
{
    public class GridSelectionController
    {
        //refernce to view-presenter
        private GridSelectionView _view;
        private List<string> grids;

        //reference to model
        private RaindropInstance instance => 
            RaindropInstance.GlobalInstance;
        private bool Ready => !(instance is null);
        private bool Connected => instance.Client.Network.Connected;

        public GridSelectionController(GridSelectionView gridSelectionView)
        {
            if (!Ready)
            {
                OpenMetaverse.Logger.Log("instance not ready yet", Helpers.LogLevel.Error);
                return;
            }

            _view = gridSelectionView;
            List<Grid> gridMangerGrids = instance.GridManger.Grids;
            grids = GridList2StringList(gridMangerGrids);
            _view.ClearAndSetOptions(grids);
            
            _view.DropdownItemSelected += OnGridSelected; //todo: unsub?
            
            //set default-selected grid.
            OnGridSelected(null, new DropdownPresenterEventArgs(_view.dropdown));
        }


        private void OnGridSelected(object sender, DropdownPresenterEventArgs e)
        {
            int selectionIdx = e.DropdownValue;
            // update model:
            var chosen = instance.GridManger.Grids[selectionIdx];
            instance.Netcom.LoginOptions.Grid = chosen;
            // update front end url display:
            _view.uritext.setText(chosen.LoginURI);
        }

        private List<string> GridList2StringList(List<Grid> gridMangerGrids)
        {
            var res = gridMangerGrids.Select(e => e.Name).ToList();
            return res;
        }

        public int GetGridsCount()
        {
            return _view.GetOptionsCount();
        }
    }
}