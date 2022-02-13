using System;
using System.Collections.Generic;
using System.IO;
using Disk;
using NUnit.Framework;
using OpenMetaverse;
using Raindrop;

namespace Raindrop.Tests.Raindrop.RaindropIntegrationTests.GridManager
{
    public class GridTests
    {
        /// <summary>
        ///  serialise list of grids into the LLSD xml file.
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
        /// </summary>
        [Test]
        public void Serialisation_CustomGrids_Two()
        {
            var instance = new RaindropInstance(new GridClient());
            
            instance.GridManger.RegisterCustomGrid(
                new Grid("lollll"
                    , "Second Life (agni)"
                    , "https://login.agni.lindenlab.com/cgi-bin/login.cgi")
                );

            
            instance.GridManger.SaveCustomGrids( DirectoryHelpers.GetInternalStorageDir());
            
            instance.CleanUp();
        }
        
        
    }
}