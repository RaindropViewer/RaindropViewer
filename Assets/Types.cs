using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raindrop
{
    class Types
    {
        [Serializable]
        public class Credential
        {
            public Credential()
            {
            }

            public Credential(String user, String pass)
            {
                this.User = user;
                this.Pass = pass;
            }

            public String User { get; set; }
            public String Pass { get; set; }
        };
    }
}
