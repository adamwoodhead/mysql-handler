using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MySQL_Handler
{
    internal static class SqlFunc
    {
        private static List<string> ExistingTables { get; set; } = new List<string>();

        /// <summary>
        /// Builds & returns Connection String for MySQL Connection upon GET
        /// </summary>
        private static string ConnectionString
        {
            get
            {
                MySqlConnectionStringBuilder conn_string = new MySqlConnectionStringBuilder
                {
                    Server = Configuration.MySqlServer,
                    Port = Configuration.MySqlPort,
                    Database = Configuration.MySqlDatabase,
                    UserID = Configuration.MySqlUsername,
                    Password = Configuration.MySqlPassword,

                    UseCompression = true,
                    SslMode = MySqlSslMode.None,
                    Keepalive = 3600
                };

                return conn_string.ToString();
            }
        }

        internal static bool TestConnection(string server, uint port, string name, string user, string pass)
        {
            try
            {
                MySqlConnectionStringBuilder conn_string = new MySqlConnectionStringBuilder
                {
                    Server = server,
                    Port = port,
                    Database = name,
                    UserID = user,
                    Password = pass,

                    UseCompression = true,
                    SslMode = MySqlSslMode.None,
                    Keepalive = 60
                };

                using (MySqlConnection sqlConnection = new MySqlConnection(conn_string.ToString()))
                {
                    sqlConnection.Open();

                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Select Table, or Record
        /// </summary>
        /// <param name="sqlObject"></param>
        /// <returns></returns>
        internal static List<T> Select<T>(SqlObject sqlObject, string[] columnFilterList, object[] args = null)
        {
            using (MySqlConnection sqlConnection = new MySqlConnection(ConnectionString))
            {
                sqlConnection.Open();

                using (MySqlCommand Command = sqlConnection.CreateCommand())
                {
                    Command.CommandText = sqlObject.CommandText;

                    Logger.Verbose(sqlObject.CommandText);

                    if (args != null)
                    {
                        int paramCount = 1;
                        foreach (object arg in args)
                        {
                            Command.Parameters.AddWithValue($"@param{paramCount}", arg);
                            paramCount++;
                        }
                    }

                    if (!Table_Exists(sqlObject.Table))
                    {
                        Table_Create(sqlObject.Table, Converter.Convert(sqlObject));
                        return new List<T>();
                    }

                    List<T> objects = new List<T>();

                    try
                    {
                        using (MySqlDataReader reader = Command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    objects.Add((T)Converter.Convert(sqlObject.Table, columnFilterList, reader));
                                }
                            }
                        }
                    }
                    catch (NullReferenceException nrex)
                    {
                        MethodBase site = nrex.TargetSite;
                        string methodName = site?.Name;
                        if (methodName != null && methodName != "ExecutePacket")
                        {
                            throw nrex;
                        }
                    }

                    return objects;
                }
            }
            
        }

        /// <summary>
        /// Select Count of table, with conditions
        /// </summary>
        /// <param name="sqlObject"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static int Count(SqlObject sqlObject, object[] args = null)
        {
            using (MySqlConnection sqlConnection = new MySqlConnection(ConnectionString))
            {
                sqlConnection.Open();

                using (MySqlCommand Command = sqlConnection.CreateCommand())
                {
                    Command.CommandText = sqlObject.CommandText;

                    Logger.Verbose(sqlObject.CommandText);

                    if (args != null)
                    {
                        int paramCount = 1;
                        foreach (object arg in args)
                        {
                            Command.Parameters.AddWithValue($"@param{paramCount}", arg);
                            paramCount++;
                        }
                    }

                    if (!Table_Exists(sqlObject.Table))
                    {
                        Table_Create(sqlObject.Table, Converter.Convert(sqlObject));
                        return 0;
                    }

                    int count = 0;

                    try
                    {
                        using (MySqlDataReader reader = Command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    count = Read<int>(reader, "Count");
                                }
                            }
                        }
                    }
                    catch (NullReferenceException nrex)
                    {
                        MethodBase site = nrex.TargetSite;
                        string methodName = site?.Name;
                        if (methodName != null && methodName != "ExecutePacket")
                        {
                            throw nrex;
                        }
                    }

                    return count;
                }
            }
        }

        /// <summary>
        /// Select Sum of table, with conditions
        /// </summary>
        /// <param name="sqlObject"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static int Sum(SqlObject sqlObject, object[] args = null)
        {
            using (MySqlConnection sqlConnection = new MySqlConnection(ConnectionString))
            {
                sqlConnection.Open();

                using (MySqlCommand Command = sqlConnection.CreateCommand())
                {
                    Command.CommandText = sqlObject.CommandText;

                    Logger.Verbose(sqlObject.CommandText);

                    if (args != null)
                    {
                        int paramCount = 1;
                        foreach (object arg in args)
                        {
                            Command.Parameters.AddWithValue($"@param{paramCount}", arg);
                            paramCount++;
                        }
                    }

                    if (!Table_Exists(sqlObject.Table))
                    {
                        Table_Create(sqlObject.Table, Converter.Convert(sqlObject));
                        return 0;
                    }

                    int count = 0;

                    try
                    {
                        using (MySqlDataReader reader = Command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    count = Read<int>(reader, "Sum");
                                }
                            }
                        }
                    }
                    catch (NullReferenceException nrex)
                    {
                        MethodBase site = nrex.TargetSite;
                        string methodName = site?.Name;
                        if (methodName != null && methodName != "ExecutePacket")
                        {
                            throw nrex;
                        }
                    }

                    return count;
                }
            }
        }

        /// <summary>
        /// Insert Record and return ID
        /// </summary>
        /// <param name="sqlObject"></param>
        /// <returns></returns>
        internal static int Row_Insert_ID(SqlObject sqlObject)
        {
            using (MySqlConnection sqlConnection = new MySqlConnection(ConnectionString))
            {
                sqlConnection.Open();

                using (MySqlCommand Command = sqlConnection.CreateCommand())
                {
                    Command.CommandText = sqlObject.CommandText;

                    int paramCount = 1;
                    foreach (SqlColumn col in sqlObject.Items)
                    {
                        Command.Parameters.AddWithValue($"@param{paramCount}", col.Value ?? "");
                        paramCount++;
                    }

                    Logger.Verbose(Command.CommandText);

                    try
                    {
                        if (Command.ExecuteNonQuery() > 0)
                        {
                            return (int)Command.LastInsertedId;
                        }
                        
                        return -1;
                    }
                    catch (MySqlException ex)
                    {
                        Logger.Verbose($"MySQL Error Code: {ex.Number}");
                        Logger.Error(ex);

                        if (ex.Number == 1146)
                        {
                            Logger.Verbose($"Looks like we don't have a table for `{sqlObject.Table}`, let's create that.");
                            bool tableCreated = Table_Create(sqlObject.Table, Converter.Convert(sqlObject));
                            Logger.Verbose($"MySql Table `{Configuration.MySqlDatabase}`.`{sqlObject.Table}` Created: {tableCreated}");
                            // Now that we've created the table, lets recursively insert that data that we already have...
                            Logger.Verbose($"Insert: {sqlObject.AttachedObject.GetType().Name}");
                            return Row_Insert_ID(sqlObject);
                        }
                        else if (ex.Number == 1364)
                        {
                            Logger.Verbose("A field is missing a default value...");
                            throw ex;
                        }

                        return -1;
                    }
                }
            }
        }

        /// <summary>
        /// Update Record by ID
        /// </summary>
        /// <param name="sqlObject"></param>
        internal static void Row_Update_ID(SqlObject sqlObject, string singleField)
        {
            using (MySqlConnection sqlConnection = new MySqlConnection(ConnectionString))
            {
                sqlConnection.Open();

                using (MySqlCommand Command = sqlConnection.CreateCommand())
                {
                    Command.CommandText = sqlObject.CommandText;

                    if (string.IsNullOrEmpty(singleField))
                    {
                        int paramCount = 1;
                        foreach (SqlColumn col in sqlObject.Items)
                        {
                            MySqlParameter param = new MySqlParameter
                            {
                                ParameterName = $"@param{paramCount}",
                                MySqlDbType = col.Type,
                                Value = col.Value
                            };
                            Command.Parameters.Add(param);
                            paramCount++;
                        }
                    }
                    else
                    {
                        foreach (SqlColumn col in sqlObject.Items)
                        {
                            if (col.PropertyName.Contains(singleField))
                            {
                                MySqlParameter param = new MySqlParameter
                                {
                                    ParameterName = $"@param1",
                                    MySqlDbType = col.Type,
                                    Value = col.Value
                                };
                                Command.Parameters.Add(param);
                                break;
                            }
                        }
                    }
                    
                    Logger.Verbose(Command.CommandText);

                    try
                    {
                        if (Command.ExecuteNonQuery() > 0)
                        {
                            return;
                        }
                    }
                    catch (MySqlException ex)
                    {
                        Logger.Verbose($"MySQL Error Code: {ex.Number}");
                        Logger.Error(ex);

                        if (ex.Number == 1054)
                        {
                            // Unknown column 'field' in 'field list'
                            string columnName = "";
                            foreach (SqlColumn col in sqlObject.Items)
                            {
                                if (ex.Message.Contains($"'{col.ColumnName}'")) { columnName = col.ColumnName; break; }
                            }
                            Logger.Verbose($"Looks like we don't have a column for `{sqlObject.Table}.{columnName}`, let's create that.");
                            bool tableCreated = Table_Alter_Add_Column(sqlObject.Table, columnName, sqlObject);
                            Logger.Verbose($"MySql Table `{Configuration.MySqlDatabase}`.`{sqlObject.Table}` Altered: Column {columnName} Added");
                            Row_Update_ID(sqlObject, singleField);
                        }
                        
                        return;
                    }
                    
                    return;
                }
            }


            
        }

        /// <summary>
        /// Drop Record by ID
        /// </summary>
        /// <param name="sqlObject"></param>
        internal static void Row_Drop_ID(SqlObject sqlObject)
        {
            using (MySqlConnection sqlConnection = new MySqlConnection(ConnectionString))
            {
                sqlConnection.Open();

                using (MySqlCommand Command = sqlConnection.CreateCommand())
                {
                    Command.CommandText = sqlObject.CommandText;

                    Logger.Verbose(Command.CommandText);

                    if (Command.ExecuteNonQuery() > 0)
                    {
                        return;
                    }
                    
                    return;
                }
            }
        }

        /// <summary>
        /// Alter Table with new Column
        /// </summary>
        /// <param name="table"></param>
        /// <param name="col"></param>
        /// <param name="sqlObject"></param>
        /// <returns></returns>
        internal static bool Table_Alter_Add_Column(string table, string col, SqlObject sqlObject)
        {
            SqlColumnAttribute sqlColumnAttrib = null;

            foreach (PropertyInfo property in Converter.GetPropertiesWithSqlColumnAttribute(sqlObject.AttachedObject.GetType()))
            {
                if (property != null)
                {
                    sqlColumnAttrib = (property.GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(SqlColumnAttribute) && (x as SqlColumnAttribute).ColumnName == col) as SqlColumnAttribute);

                    if (sqlColumnAttrib != null)
                    {
                        break;
                    }
                }
            }

            Logger.Verbose($"Alter Table: {table}, Add Column {col}");

            using (MySqlConnection sqlConnection = new MySqlConnection(ConnectionString))
            {
                sqlConnection.Open();
                
                using (MySqlCommand Command = sqlConnection.CreateCommand())
                {

                    Command.CommandText = $"ALTER TABLE `{table}` ADD COLUMN `{col}` {sqlColumnAttrib.PropertyType}{((sqlColumnAttrib.AllowNull) ? "" : " NOT NULL")};";

                    Command.ExecuteNonQuery();

                    if (Column_Exists(table, col))
                    {
                        return true;
                    }
                    
                    return false;
                }
            }
        }

        /// <summary>
        /// Create Table if it does not already exist
        /// </summary>
        /// <param name="table">Table Name</param>
        /// <param name="query"></param>
        /// <returns></returns>
        internal static bool Table_Create(string table, string query)
        {
            Logger.Verbose($"Create Table: {table}");

            using (MySqlConnection sqlConnection = new MySqlConnection(ConnectionString))
            {
                sqlConnection.Open();
                
                using (MySqlCommand Command = sqlConnection.CreateCommand())
                {

                    Command.CommandText = query;

                    Command.ExecuteNonQuery();

                    if (Table_Exists(table))
                    {
                        return true;
                    }
                    
                    return false;
                }
            }
        }

        /// <summary>
        /// Check if Table Exists
        /// </summary>
        /// <param name="table">Table Name</param>
        /// <returns></returns>
        internal static bool Table_Exists(string table)
        {
            if (ExistingTables.Exists(x => x == table))
            {
                return true;
            }

            Logger.Verbose($"Table Exist Check: {table}");

            using (MySqlConnection sqlConnection = new MySqlConnection(ConnectionString))
            {
                sqlConnection.Open();

                using (MySqlCommand Command = sqlConnection.CreateCommand())
                {
                    Command.CommandText = $"SELECT 1 FROM `{table}` LIMIT 1;";

                    try
                    {
                        Command.ExecuteReader();
                        
                        // We would throw an exception if table does not exist.
                        ExistingTables.Add(table);
                        return true;
                    }
                    catch (MySqlException)
                    {
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Check if column exists in table
        /// </summary>
        /// <param name="table">Table Name</param>
        /// <param name="column">Column Name</param>
        /// <returns></returns>
        internal static bool Column_Exists(string table, string column)
        {
            Logger.Verbose($"Column Exist Check: {table}");

            using (MySqlConnection sqlConnection = new MySqlConnection(ConnectionString))
            {
                sqlConnection.Open();
                
                using (MySqlCommand Command = sqlConnection.CreateCommand())
                {

                    Command.CommandText = $"SHOW COLUMNS FROM `{table}` LIKE '{column}';";

                    try
                    {
                        Command.ExecuteReader();
                        // We would throw an exception if column does not exist.
                        return true;
                    }
                    catch (MySqlException)
                    {
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// SqlObject's Attached Object, or (T)Object must have (int?) property "ID", of which is returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns>ID of object, or SqlObject.AttachedObject, null if unset or non-existant</returns>
        internal static object ReflectPrimary<T>(T obj)
        {
            if (typeof(T) == typeof(SqlObject))
            {
                return (obj as SqlObject)?.AttachedObject?.GetType()?.GetProperty(Converter.GetPrimaryProperty((obj as SqlObject)?.AttachedObject).Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue((obj as SqlObject)?.AttachedObject) ?? null;
            }
            else
            {
                return obj?.GetType()?.GetProperty(Converter.GetPrimaryProperty(obj).Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(obj) ?? null;
            }
        }

        /// <summary>
        /// Convert DataReader Column data to true data in correct variable type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="DataReader"></param>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        internal static T Read<T>(DbDataReader DataReader, string FieldName)
        {
            if (FieldName == null)
            {
                return default;
            }

            int FieldIndex;
            try
            {
                FieldIndex = DataReader.GetOrdinal(FieldName);
            }
            catch (IndexOutOfRangeException)
            {
                return default;
            }

            if (DataReader.IsDBNull(FieldIndex))
            {
                return default;
            }
            else
            {
                object readData = DataReader.GetValue(FieldIndex);
                if (readData is T)
                {
                    return (T)readData;
                }
                else
                {
                    try
                    {
                        return (T)Convert.ChangeType(readData, typeof(T));
                    }
                    catch (InvalidCastException)
                    {
                        return default;
                    }
                }
            }
        }
    }
}