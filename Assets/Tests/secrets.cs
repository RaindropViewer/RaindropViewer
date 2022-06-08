using System.Collections.Generic;

namespace Tests
{
    //store the passwords used in integration test.
    public static class Secrets
    {
        // the names of the grid service, defined in the friendlyname attribute in the grids.xml file.
        public static List<string> _gridFriendlyNames = new List<string>()
        {
            "Second Life (agni)",
            "Metropolis Metaversum",
            "Local Host"
            // "https://login.agni.lindenlab.com/cgi-bin/login.cgi",
            // "login.metro.land"
        };
        
        // user name
        public static List<string> GridUsers = new List<string>()
        {
            "***REMOVED*** Resident",
            "Raindrop Raindrop",
            "Test User"
        };
        
        // password
        public static List<string> GridPass = new List<string>()
        {
            "I am a little ",
            "silly to put the password ",
            "into source control "
        };
    }
}