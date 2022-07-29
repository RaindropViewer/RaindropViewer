using System;
using System.Collections;
using NUnit.Framework;
using Plugins.CommonDependencies;
using Raindrop.Tests.RaindropIntegrationTests.Helpers;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Raindrop.Tests.RaindropIntegrationTests.GridSelectorTests
{
    [TestFixture()]
    public class GridSelectorTests
    {
        //https://forum.unity.com/threads/add-coroutine-version-of-onetimesetup.890092/
        [UnitySetUp]
        public IEnumerator UnitySetup()
        {
            //+++++++++++++load test scene+++++++
            // load the base scene 
            yield return SceneLoader.LoadHeadlessScene();
        
            //at this point, raindropInstance is ready.
            var instance = RaindropInstance.GlobalInstance;
            Assert.True(instance!=null, "instance is null");
        
            //+++++++++++++finish load test scene+++++++
            yield return null;
        }
     
        [OneTimeTearDown]
        public static void OneTimeTearDown()
        {
            SceneLoader.UnloadHeadlessScene();
        }

        
        [UnityTest]
        public IEnumerator GridDropdown_HasManyGrids()
        {
            //load and open up the dropdown small ui.
            string pathOfPrefabDirectory = "Assets/Raindrop/UI/Login/";
            string prefabName = "GridSelection.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath(pathOfPrefabDirectory+prefabName, typeof(GameObject));
            GameObject dropdownUI = (GameObject) GameObject.Instantiate(prefab);
            Assert.True(dropdownUI!=null, "dropdown prefab not found in resources");

            //click dropdown UI to reveal its full content
            GridSelectionView gridGridSelection = dropdownUI.GetComponent<GridSelectionView>();
            Assert.True(gridGridSelection,"the dropdown doesnt have GridSelectionView");

            yield return new WaitForSeconds(2); // seems if you don't wait, the options count are not updated yet.
            
            //Assert that there is more than 5 grids in that list.
            Assert.True(gridGridSelection.GetOptionsCount() > 5
                , "dropdown has insufficient options : "+ gridGridSelection.GetOptionsCount());
        }
    }
}