using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using OpenMetaverse;
using Vector3 = UnityEngine.Vector3;
using Raindrop.Utilities;
using UnityEngine;
using Quaternion = OpenMetaverse.Quaternion;

namespace Raindrop.Tests.Map3D
{
    // the location of the mapEntities in the scene hierachy is defined as 
    // "global" coordinates / 256 - that is, an entity existing in the sim Daboom (1000, 1000)
    // at the sim coordinates 0,0,0, will be shown at the Unityscene position of
    // (1000,1000, mapItemDepthConstant) 

    [TestFixture()]
    public class MapCoordinatesConversionTests
    {


        //test handle can convert to map space correctly.
        //handle: some large ulong. Mapspace: a vector 3 that is ready to use for moving/creating unity objects.
        [Test]
        public void Handle2MapSpace_Test()
        {
            for (uint i = 0; i < 65536; i += 301)
            {
                for (uint j = 0; j < 65536; j += 301)
                {
                    ulong handle = Utils.UIntsToLong(i * 256,j * 256); 
                    var mapSpace = MapSpaceConverters.Handle2MapSpace(handle, 0);
                    Assert.True(mapSpace == new Vector3(i,j,0) , 
                        "error: " + i +  " " + j);
                }
            }

        }

        //test handle can convert to map space correctly, within the tile of 1000x1000.
        // important idea is that you can obtain the accuracy up to 1m. sufficient for minimap purpose.
        // each handle int is a meter ; 1/256 of a sim's width.
        [Test]
        public void Handle2MapSpace_IntraTile_Test()
        {
            // in sims:
            ulong daboom = OpenMetaverse.Utils.UIntsToLong( 1000 , 1000 );
            ulong zero = OpenMetaverse.Utils.UIntsToLong(0, 0 );
            ulong largest = OpenMetaverse.Utils.UIntsToLong(65535 , 65535);
            List<ulong> handlesToTest_ingrids = new List<ulong>() {daboom, zero, largest};
            
            foreach (ulong _ in handlesToTest_ingrids)
            {
                Test_Handle2MapSpace_WithinSimulator(_);                
            }

            void Test_Handle2MapSpace_WithinSimulator(ulong simHandle_InGrids)
            {
                uint SimX_ingrids; 
                uint SimY_ingrids;
                OpenMetaverse.Utils.LongToUInts(simHandle_InGrids , out SimX_ingrids, out SimY_ingrids);
                
                for (uint i = 0; i < 256; i += 1)
                {
                    for (uint j = 0; j < 256; j += 1)
                    {
                        uint handle_x_meters = (uint) (SimX_ingrids * 256 + i);
                        uint handle_y_meters = (uint) (SimY_ingrids * 256 + j);
                        ulong handle = Utils.UIntsToLong(handle_x_meters, handle_y_meters);
                        var mapSpace = MapSpaceConverters.Handle2MapSpace(handle, 0);
                        
                        float expected_mapSpace_x = SimX_ingrids + (i / 256.0f);
                        float expected_mapSpace_y = SimY_ingrids + (j / 256.0f);
                        
                        Assert.True(mapSpace == new Vector3(expected_mapSpace_x, expected_mapSpace_y, 0),
                            "error: " + i + " " + j + " " + " @ " +  SimX_ingrids +" " + SimY_ingrids );
                    }
                }
            }
        }
        
        [Test]
        public void MapSpace2Handle_Test()
        {
            var mapSpaceEntityPosition = new Vector3(1000, 1000, 0);
            var handle = MapSpaceConverters.MapSpace2Handle(mapSpaceEntityPosition);

            Assert.True(handle == Utils.UIntsToLong((1000 * 256), (1000 * 256)));

        }
        
        [Test]
        public void OMVRot2MapRot_Test()
        {
            //lets say the entity is looking north-wards in the map.
            var OMV_north = Quaternion.Identity; 
            var MapspaceRot_north = MapSpaceConverters.GlobalRot2MapRot(OMV_north);
            Debug.Log(MapspaceRot_north.eulerAngles.ToString());
            Assert.True(MapspaceRot_north.eulerAngles.x == 0);
            Assert.True(MapspaceRot_north.eulerAngles.z == 0);
            Assert.True(MapspaceRot_north.eulerAngles.y == 0);

            
            //lets say the entity is looking to the right-wards in the map.
            var OMV_east = Quaternion.CreateFromEulers(0, 0, -1.57f); //right handed screwdriver. 
            var MapspaceRot = MapSpaceConverters.GlobalRot2MapRot(OMV_east);

            Debug.Log(MapspaceRot.eulerAngles.ToString());
            Assert.True(MapspaceRot.eulerAngles.x == 0);
            Assert.True(MapspaceRot.eulerAngles.y == 0);
            Assert.True(Math.Abs(MapspaceRot.eulerAngles.z - 270) < 0.1f); //left handed screwdriver );

        }

        [Test]
        public void GlobalSpaceToMapSpace_Test()
        {
            // Create the global space of daboom, plus a bit of local offset...
            var LocalCoordinates_x = 34.0f;
            var LocalCoordinates_y = 253.0f;
            var GridCoordinates_meters_x = 1000 * 256;
            var GridCoordinates_meters_y = 1000 * 256;
            Vector3d globalCoordinates = 
                new Vector3d(
                    LocalCoordinates_x + GridCoordinates_meters_x,
                    LocalCoordinates_y + GridCoordinates_meters_y,
                    0);

            UnityEngine.Vector3 mapSpace = MapSpaceConverters.GlobalSpaceToMapSpace(globalCoordinates);
            Assert.True(mapSpace.x > 0, "vector3d -> unity conversion of x is wrongfully negative");
            Assert.True(mapSpace.y > 0, "vector3d -> unity conversion of y is wrongfully negative");
            Assert.True(mapSpace.x > 999, "vector3d -> unity conversion of x is clearly out of bounds");
            Assert.True(mapSpace.y > 999, "vector3d -> unity conversion of y is clearly out of bounds");
            Assert.True(mapSpace.x < 1001, "vector3d -> unity conversion of x is clearly out of bounds");
            Assert.True(mapSpace.y < 1001, "vector3d -> unity conversion of y is clearly out of bounds");
            
            // should convert to the desired grid position: 1000.xx,1000.xx, 0
            Vector3 expectedMapSpace = new Vector3(1000,1000, 0);
            Assert.True(
                UnityEngine.Vector3.Distance(
                    mapSpace, 
                    expectedMapSpace
                )  < 1,
                "converted grid coordinates is not within 1 grid of the expected grid coordinates.");

            
            
        }
    }
}