using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace MySQL_Handler
{
    /// <summary>
    /// Result of Property after converting an object into an SqlObject
    /// </summary>
    internal class SqlColumn
    {

        /// <summary>
        /// MySQL Column Name
        /// </summary>
        internal string ColumnName { get; set; }

        /// <summary>
        /// Column is Primary
        /// </summary>
        internal bool Primary { get; set; } = false;
        /// <summary>
        /// Record Value
        /// </summary>
        internal object Value { get; set; }

        internal string PropertyName { get; set; }

        internal MySqlDbType Type { get; set; }

        internal SqlColumn(string col, MySqlDbType type, object val, bool prim, string propName)
        {
            ColumnName = col;
            Type = type;
            Value = val;
            Primary = prim;
            PropertyName = propName;
        }
    }
}