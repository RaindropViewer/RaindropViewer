using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using OpenMetaverse;
using OpenMetaverse.ImportExport.Collada14;
using Raindrop;
using Raindrop.Media;
using Raindrop.Rendering;
using Raindrop.ServiceLocator;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
using Raindrop.Utilities;

namespace Tests.Raindrop
{
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
        //takes 5 mins.
        [Test]
        public void Handle2MapSpace_IntraTile_Test()
        {
            for (uint i = 0; i < 256; i += 1)
            {
                for (uint j = 0; j < 256; j += 1)
                {
                    ulong handle = Utils.UIntsToLong((1000 * 256) + i,(1000 * 256) + j); //daboom's handle @ origin.
                    var mapSpace = MapSpaceConverters.Handle2MapSpace(handle, 0);
                    Assert.True(mapSpace == new Vector3(1000 + (i/256),1000 + (j/256),0) , 
                        "error: " + i +  " " + j);
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