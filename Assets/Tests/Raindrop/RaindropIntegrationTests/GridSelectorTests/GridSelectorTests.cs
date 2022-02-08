﻿using System;
using System.Collections;
using NUnit.Framework;
using Raindrop;
using Raindrop.GridSelection;
using Raindrop.ServiceLocator;
using Raindrop.Tests;
using Tests.Raindrop.RaindropIntegrationTests.Helpers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Tests.Raindrop.RaindropIntegrationTests.GridSelectorTests
{
    [TestFixture()]
    public class GridSelectorTests
    {
        //https://forum.unity.com/threads/add-coroutine-version-of-onetimesetup.890092/
        private bool initialized = false;
        [UnitySetUp]
        public IEnumerator UnitySetup()
        {
            if (!initialized)
            {
                initialized = true;
                
                //+++++++++++++load test scene+++++++
                // load the base scene 
                yield return Helpers.SceneLoader.LoadHeadlessScene();
            
                //at this point, raindropInstance is ready.
                var instance = ServiceLocator.Instance.Get<RaindropInstance>();
                Assert.True(instance!=null, "instance is null");
            
                //open up the dropdown small ui.
                string pathOfPrefabDirectory = "LoginUI/";
                string prefabName = "GridSelection";
                var dropdownUI = GameObject.Instantiate(
                    Resources.Load(pathOfPrefabDirectory+prefabName)
                ) as GameObject;
                Assert.True(dropdownUI!=null, "dropdown prefab not found in resources");
                //+++++++++++++finish load test scene+++++++
                yield return null;

            }
        }
     
        [OneTimeTearDown]
        public static void OneTimeTearDown()
        {
            SceneLoader.UnloadHeadlessScene();
        }

        
        [UnityTest]
        public IEnumerator GridDropdown_HasManyGrids()
        {
            //click dropdown UI to reveal its full content
            var dropdown_GO = GameObject.Find("Dropdown");
            Assert.True(dropdown_GO,"the GO with name Dropdown is not found");
            DropdownViewPresenter gridDropdown = dropdown_GO.GetComponent<DropdownViewPresenter>();
            Assert.True(gridDropdown,"the dropdown doesnt have DropdownViewPresenter");

            yield return new WaitForSeconds(2); // seems if you don't wait, the options count are not updated yet.
            
            //Assert that there is more than 5 grids in that list.
            Assert.True(gridDropdown.GetOptionsCount() > 5
                , "dropdown has insufficient options : "+ gridDropdown.GetOptionsCount());
        }
    }
}