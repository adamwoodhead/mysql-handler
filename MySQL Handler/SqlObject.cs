using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace MySQL_Handler
{
    /// <summary>
    /// Final converted C# object for retrieving MySQL Data
    /// </summary>
    internal class SqlObject
    {
        internal SqlObject(object attachedObject = null)
        {
            AttachedObject = attachedObject;
        }

        /// <summary>
        /// True Object Reference
        /// </summary>
        internal object AttachedObject { get; set; }

        /// <summary>
        /// Table Name
        /// </summary>
        internal string Table { get; set; } = "";

        /// <summary>
        /// Full Query String
        /// </summary>
        internal string CommandText { get; set; }

        /// <summary>
        /// All SqlColumns & respective data
        /// </summary>
        internal List<SqlColumn> Items { get; set; } = new List<SqlColumn>();
    }
}