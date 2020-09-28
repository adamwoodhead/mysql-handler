using System;
using System.Collections.Generic;
using System.Text;

namespace MySQL_Handler
{
    /// <summary>
    /// For attributing object properties relevant to a MySQL column.
    /// </summary>
    public class SqlColumnAttribute : Attribute
    {
        /// <summary>
        /// MySQL Column Name
        /// </summary>
        internal string ColumnName { get; set; }

        /// <summary>
        /// MySQL Column Type
        /// </summary>
        internal string PropertyType { get; set; }

        internal SqlVarType SqlVarType { get; set; }

        /// <summary>
        /// MySQL Column Allows Null Entry
        /// </summary>
        internal bool AllowNull { get; set; }

        /// <summary>
        /// Derived From
        /// </summary>
        internal string AttachedTable { get; set; }

        /// <summary>
        /// Column is Primary in table
        /// </summary>
        internal bool Primary { get; set; }
        
        /// <summary>
        /// C# Property should be read as new Table, deriving objects
        /// </summary>
        internal bool IsTable { get; set; }

        /// <summary>
        /// Delagate Property to be a MySQL Record
        /// </summary>
        /// <param name="columnName">Column</param>
        /// <param name="sqltype">Column Data Type</param>
        /// <param name="allownull">Column Allows Null</param>
        /// <param name="options">Primary or None</param>
        public SqlColumnAttribute(string columnName, SqlVarType sqltype, bool allownull = true, FieldOptions options = FieldOptions.NONE)
        {
            ColumnName = columnName;
            SqlVarType = sqltype;
            Primary = (options == FieldOptions.PRIMARY);
            IsTable = (options == FieldOptions.TABLE);
            PropertyType = Converter.ConvertSqlType(sqltype);
            AllowNull = allownull;
        }

        /// <summary>
        /// Delegate Property to be a deriving table, or list
        /// </summary>
        /// <param name="attachedTable">Property for reallocation upon retrieval</param>
        /// <param name="options">Table or List</param>
        public SqlColumnAttribute(string attachedTable, FieldOptions options)
        {
            IsTable = (options == FieldOptions.TABLE);
            AttachedTable = attachedTable;
        }
    }

    /// <summary>
    /// For attributing objects relevant to a MySQL Table.
    /// </summary>
    public class SqlTableAttribute : Attribute
    {
        /// <summary>
        /// MySQL Table Name
        /// </summary>
        internal string TableName { get; set; }

        public SqlTableAttribute(string tableName)
        {
            TableName = tableName;
        }
    }
}