using System;
using System.Collections.Generic;
using System.Text;

namespace Hangfire.Simplify.Exceptions
{
    public class ManyHanfireServersFound: Exception
    {
        public ManyHanfireServersFound(string serverName): base ($"Many servers with name {serverName} was found.")
        {

        }
    }
}
