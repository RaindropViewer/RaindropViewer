using System;
using UnityEngine.Assertions;

namespace Tests
{
    //store the passwords used in integration test.
    public static class Secrets
    {
        public static string GetUsername()
        {
            string res =
                Environment.GetEnvironmentVariable("USERNAME_SECONDLIFE");
            Assert.IsTrue(
                res != null,
                "Env: USERNAME_SECONDLIFE undefined.");
            return res;
        }

        public static string GetPassword()
        {
            string res =
                Environment.GetEnvironmentVariable("PASSWORD_SECONDLIFE");
            Assert.IsTrue(
                res != null,
                "Env: PASSWORD_SECONDLIFE undefined.");
            return res;
        }
        public static string GetGridFriendlyName()
        {
            string res =
                Environment.GetEnvironmentVariable("GRIDFRIENDLYNAME_SECONDLIFE");
            Assert.IsTrue(
                res != null,
                "Env: GRIDFRIENDLYNAME_SECONDLIFE undefined.");
            return res;
        }
    }
}