using System;
using System.Collections.Generic;
using System.IO;
using Disk;
using NUnit.Framework;
using OpenMetaverse;

namespace Raindrop.Tests.RaindropIntegrationTests.GridManager
{
    public class GridTests
    {
        /// <summary>
        ///  serialise list of grids into the LLSD xml file.
        /// todo: currently, no check on end result.
        /// </summary>
        [Test]
        public void Serialisation_CustomGrids()
        {
            var instance = new RaindropInstance(new GridClient());
            
            instance.GridManger.SaveCustomGrids( DirectoryHelpers.GetInternalStorageDir());

            instance.CleanUp();
        }
        
        /// <summary>
        ///  Create 2 grids, save them to file.
        /// todo: currently, no check on end result.
        /// </summary>
        [Test]
        public void Serialisation_CustomGrids_AddGrid_PersistentAfterRestart()
        {
            //1. add my custom grid, serialise it to disk.
            var instance = new RaindropInstance(new GridClient());
            
            instance.GridManger.RegisterCustomGrid(
                new Grid("new item"
                    , "custom grid two test"
                    , "https://meow.com")
                );
            
            instance.GridManger.SaveCustomGrids( DirectoryHelpers.GetInternalStorageDir());
            instance.CleanUp();
            instance = null;

            //2. restart raindrop, expect that list of grids, my new grid is present
            instance = new RaindropInstance(new GridClient());
            var foundGridIdx = -1;
            foundGridIdx = instance.GridManger.CustomGrids.FindIndex(grid => grid.Name == "custom grid two test");
            Assert.True(foundGridIdx != -1, "cant find the newly added grid ");
            
            //3. cleanup: delete grid from custom list.
            instance.GridManger.UnregisterGrid(foundGridIdx);
            bool foundGrid = instance.GridManger.CustomGrids.Exists(grid => grid.Name == "custom grid two test");
            Assert.True(foundGrid == false, " newly added grid successfuly purged from disk ");

            instance.CleanUp();
            instance = null;
        }
        
        
    }
}