using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MySQL_Handler
{
    public static class MySQLHandler
    {
        public static bool TestConnection(string server, uint port, string name, string user, string pass) => SqlFunc.TestConnection(server, port, name, user, pass);

        public static Type[] Types { get; set; } = null;
    }
}
