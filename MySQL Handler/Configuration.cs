using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace MySQL_Handler
{
    public static class Configuration
    {
        /// <summary>
        /// Database Address
        /// </summary>
        public static string MySqlServer { internal get; set; }

        /// <summary>
        /// Database Port - default 3306.
        /// </summary>
        public static uint MySqlPort { internal get; set; } = 3306;

        /// <summary>
        /// Database User Username
        /// </summary>
        public static string MySqlUsername { internal get; set; }

        /// <summary>
        /// Database User Password
        /// </summary>
        public static string MySqlPassword { internal get; set; }

        /// <summary>
        /// Database Name
        /// </summary>
        public static string MySqlDatabase { internal get; set; }

        /// <summary>
        /// Table prefix
        /// </summary>
        public static string DatabasePrefix { internal get; set; } = string.Empty;

        /// <summary>
        /// Table trailing suffix
        /// </summary>
        public static string DatabaseSuffix { internal get; set; } = string.Empty;

        /// <summary>
        /// Logs Enabled / Disabled
        /// </summary>
        public static bool LogsEnabled { internal get; set; } = true;

        /// <summary>
        /// Enabled / Disable Verbose Mode
        /// </summary>
        public static bool VerboseMode { internal get; set; } = true;

        /// <summary>
        /// Out TextWriter for all log messages
        /// </summary>
        public static TextWriter Out { internal get; set; }
    }
}