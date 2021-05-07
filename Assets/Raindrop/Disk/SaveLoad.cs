using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raindrop.Disk
{
    public static class SaveLoad
    {
        //return: TRUE for saving success.
        public static bool saveCred(string user, string pass, string m_Path)
        {
            var myCred = new Raindrop.Types.Credential();
            myCred.User = user;
            myCred.Pass = pass;

            if (Disk.ReadWrite.saveCredentials(myCred, m_Path) == true)
            {
                return true;
            }
            else
                return false;

        }

        //return: the credentials as list
        public static List<Raindrop.Types.Credential> loadCred(string m_Path)
        {

            var myCred = Disk.ReadWrite.ReadCredentials(m_Path);

            return myCred;
        }
    }
}
