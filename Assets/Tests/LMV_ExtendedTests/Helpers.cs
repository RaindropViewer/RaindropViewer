using NUnit.Framework;
using OpenMetaverse;
using Tests;
using UnityEngine;
using UnityScripts.Disk;

namespace Raindrop.Tests.LMV_ExtendedTests
{
    public class Helpers
    {

        // Perform login, using the LMV library only.
        public static void LoginHeadless(
            RaindropInstance instance,
            int userIdx = 0, 
            string startLocation = "Hooper")
        {
            if (instance == null)
            {
                Assert.Fail();
            }
            /* hack: I added this,
             * to prevent the library from throwing the runtime exception :
             *
             * Unhandled log message: '[Error] 22:34:22 [ERROR] - <TanukiDEV
             * Resident>: Setting server side baking failed'. Use
             * UnityEngine.TestTools.LogAssert.Expect   */
            instance.Client.Settings.SEND_AGENT_APPEARANCE = false;

            var fullUsername = Secrets.GridUsers[userIdx];
            var password = Secrets.GridPass[userIdx];
            Assert.IsFalse(string.IsNullOrWhiteSpace(fullUsername),
                "LMVTestAgentUsername is empty. " +
                "Live NetworkTests cannot be performed.");
            Assert.IsFalse(string.IsNullOrWhiteSpace(password),
                "LMVTestAgentPassword is empty. " +
                "Live NetworkTests cannot be performed.");
            var username = fullUsername.Split(' ');

            // Connect to the grid
            string startLoc = 
                NetworkManager.StartLocation(startLocation, 179, 18, 32);
            Debug.Log($"Logging in " +
                      $"User: {fullUsername}, " +
                      $"Loc: {startLoc}");
            bool loginSuccessful = instance.Client.Network.Login(
                username[0],
                username[1],
                password,
                "Unit Test Framework",
                startLoc,
                "raindropcafeofficial@gmail.com");
            Assert.IsTrue(loginSuccessful, 
                $"Client failed to login, reason: " +
                $"{instance.Client.Network.LoginMessage}");
            Debug.Log("Grid returned the login message: " 
                      + instance.Client.Network.LoginMessage);

            Assert.IsTrue(
                instance.Client.Network.Connected, 
                "Client is not connected to the grid");
        }

        // Copies the OMV assets
        // from the StreamingAssets folder, into the runtime folder
        public static void DoStartupCopy()
        {
            var copier = StaticFilesCopier.GetInstance();
            copier.Work();
            Assert.True(copier.CopyIsDoneAndNoErrors);
        }
    }
}