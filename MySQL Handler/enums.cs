using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySQL_Handler
{
    /// <summary>
    /// SqlHandler Select Options
    /// </summary>
    public enum SelectOptions { ALL, FIELDS }

    /// <summary>
    /// SqlHandler Select Options
    /// </summary>
    public enum ConditionalOptions { NONE, BY_ID, WHERE, WHERENULL }

    /// <summary>
    /// SqlHander Where Options
    /// </summary>
    public enum WhereOption { NONE, EQUAL, NOTEQUAL, GREATERTHAN, LESSTHAN }

    /// <summary>
    /// MySQL Column/ C# Object Attribute Types
    /// </summary>
    public enum FieldOptions { NONE, PRIMARY, TABLE, LIST }

    /// <summary>
    /// MySQL Column Types
    /// </summary>
    public enum SqlVarType { INT12, BOOL, ENUM, VARCHAR60, VARCHAR120, TEXT, BLOB, MEDIUMBLOB, LONGBLOB, VARBINARY, DOUBLE }

    /// <summary>
    /// Available Log Types
    /// </summary>
    internal enum LogType { INFO, WARNING, ERROR, EXCEPTION, VERBOSE }

    /// <summary>
    /// Limiter Order Types
    /// </summary>
    public enum OrderBy { None, Ascending, Descending }
}
