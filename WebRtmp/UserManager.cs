using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebRtmp
{
    public class UserManager
    {
        public bool AuthPass(string name, string password)
        {
            return name == "coredx" && password == "123456";
        }
    }
}
