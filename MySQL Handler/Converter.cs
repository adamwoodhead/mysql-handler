using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Collections;

namespace MySQL_Handler
{
    /// <summary>
    /// Used for converting C# Data into MySQL Data, and the reverse.
    /// </summary>
    internal static class Converter
    {
        /// <summary>
        /// Convert C# object to readable data for MySqlHandler to manipulate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal static List<SqlObject> Convert<T>(T obj)
        {
            SqlObject sqlObject = new SqlObject(obj);

            List<SqlObject> SqlObjects = new List<SqlObject>() { sqlObject };

            Type type = obj.GetType();

            // Table Name
            if (!(type.GetCustomAttributes(typeof(SqlTableAttribute), true).FirstOrDefault() is SqlTableAttribute table))
            {
                Exception exception = new Exception($"Table is null on Type {type}");
                throw exception;
            }
            sqlObject.Table = table.TableName;

            // Column Names & Values
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                object[] attributes = property.GetCustomAttributes(false);
                
                object column = attributes
                    .FirstOrDefault(x => x.GetType() == typeof(SqlColumnAttribute));

                if (column != null /*&& property.GetValue(obj) != null*/)
                {
                    SqlColumnAttribute col = column as SqlColumnAttribute;
                    if (col.IsTable)
                    {
                        SqlObjects.AddRange(Convert(property.GetValue(obj)));
                    }
                    else if(!col.Primary || (col.Primary && property.GetValue(obj) != null))
                    {
                        if (property.PropertyType != typeof(byte[]) && property.GetValue(obj) is IList enumerable)
                        {
                            foreach (object listObj in (property.GetValue(obj) as IList))
                            {
                                SqlObjects.AddRange(Convert(listObj));
                            }
                        }
                        else
                        {
                            Type valType = property.PropertyType;
                            object val = null;

                            if (valType.IsEnum)
                            {
                                val = System.Convert.ToInt32(property.GetValue(obj));
                            }
                            else if (valType == typeof(bool))
                            {
                                val = System.Convert.ToInt32(property.GetValue(obj));
                            }
                            else
                            {
                                val = property.GetValue(obj);
                            }

                            SqlColumn sqlItem = new SqlColumn(col.ColumnName, RelevantType(col.SqlVarType), val, col.Primary, property.Name);
                            sqlObject.Items.Add(sqlItem);
                        }                        
                    }
                }
            }

            return SqlObjects;
        }

        internal static MySqlDbType RelevantType(SqlVarType sqlVarType)
        {
            switch (sqlVarType)
            {
                case SqlVarType.INT12:
                    return MySqlDbType.Int16;
                case SqlVarType.BOOL:
                    return MySqlDbType.Int16;
                case SqlVarType.ENUM:
                    return MySqlDbType.Int16;
                case SqlVarType.VARCHAR60:
                    return MySqlDbType.VarChar;
                case SqlVarType.VARCHAR120:
                    return MySqlDbType.VarChar;
                case SqlVarType.TEXT:
                    return MySqlDbType.Text;
                case SqlVarType.BLOB:
                    return MySqlDbType.Blob;
                case SqlVarType.MEDIUMBLOB:
                    return MySqlDbType.MediumBlob;
                case SqlVarType.LONGBLOB:
                    return MySqlDbType.LongBlob;
                case SqlVarType.VARBINARY:
                    return MySqlDbType.VarBinary;
                case SqlVarType.DOUBLE:
                    return MySqlDbType.Double;
                default:
                    throw new Exception("Invalid SqlVarType Enum");
            }
        }

        /// <summary>
        /// Generate Sql syntax type string for related type
        /// </summary>
        /// <param name="sqltype"></param>
        /// <returns></returns>
        internal static string ConvertSqlType(SqlVarType sqltype)
        {
            switch (sqltype)
            {
                case SqlVarType.INT12:
                    return "INT(12)";
                case SqlVarType.BOOL:
                    return "TINYINT(4)";
                case SqlVarType.ENUM:
                    return "TINYINT(4)";
                case SqlVarType.VARCHAR60:
                    return "VARCHAR(60)";
                case SqlVarType.VARCHAR120:
                    return "VARCHAR(120)";
                case SqlVarType.TEXT:
                    return "TEXT";
                case SqlVarType.BLOB:
                    return "BLOB";
                case SqlVarType.MEDIUMBLOB:
                    return "MEDIUMBLOB";
                case SqlVarType.LONGBLOB:
                    return "LONGBLOB";
                case SqlVarType.VARBINARY:
                    return "VARBINARY(32768)";
                case SqlVarType.DOUBLE:
                    return "DOUBLE";
                default:
                    throw new Exception("Invalid SqlVarType Enum");
            }
        }

        /// <summary>
        /// Convert C# Object into readable data for MySqlHandler to manipulate, specifically for creating a table
        /// </summary>
        /// <param name="sqlObject"></param>
        /// <returns></returns>
        internal static string Convert(SqlObject sqlObject)
        {
            string query = $"CREATE TABLE `{sqlObject.Table}` (";

            Type type = Converter.GetTypesWithSqlTableAttributeByName(sqlObject.Table);

            if (type == null)
            {
                throw new Exception("Type is null");
            }

            SqlColumnAttribute sqlPrimaryColumn = null;
            foreach (PropertyInfo property in Converter.GetPropertiesWithSqlColumnAttribute(type))
            {
                if (property != null)
                {
                    SqlColumnAttribute sqlColumn = (property.GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(SqlColumnAttribute)) as SqlColumnAttribute);
                    if (sqlColumn.Primary)
                    {
                        sqlPrimaryColumn = sqlColumn;
                    }

                    if (!sqlColumn.IsTable && sqlColumn.ColumnName != null)
                    {
                        query += $"`{sqlColumn.ColumnName}` {sqlColumn.PropertyType} {((sqlColumn.AllowNull) ? "" : "NOT NULL")}{((sqlColumn.Primary ? " AUTO_INCREMENT," : ","))}";
                    }
                }
            }

            if (sqlPrimaryColumn != null)
            {
                query += $"PRIMARY KEY (`{sqlPrimaryColumn.ColumnName}`) );";
            }

            return query;
        }

        /// <summary>
        /// Convert MySql Data into readable C# Object
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal static object Convert(string tableName, string[] filter, MySqlDataReader obj)
        {
            Type type = GetTypesWithSqlTableAttributeByName(tableName);

            //Type newType = type.MakeGenericType();
            object newObject = Activator.CreateInstance(type, true);

            foreach (PropertyInfo property in GetPropertiesWithSqlColumnAttribute(newObject.GetType()))
            {
                if (property != null)
                {
                    string fieldName = (property.GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(SqlColumnAttribute)) as SqlColumnAttribute).ColumnName;
                    if (filter != null)
                    {
                        if (filter.Contains(fieldName))
                        {
                            property.SetValue(newObject, ChangeType(SqlFunc.Read<object>(obj, fieldName), property.PropertyType), null);
                        }
                    }
                    else
                    {
                        property.SetValue(newObject, ChangeType(SqlFunc.Read<object>(obj, fieldName), property.PropertyType), null);
                    }
                }
            }

            return newObject;
        }

        /// <summary>
        /// Change type of object
        /// </summary>
        /// <param name="value">Target Object</param>
        /// <param name="conversion">Target Type</param>
        /// <returns></returns>
        internal static object ChangeType(object value, Type conversion)
        {
            Type t = conversion;

            if (value == null)
            {
                return null;
            }

            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {

                t = Nullable.GetUnderlyingType(t);
            }

            if (t.IsEnum)
            {
                return System.Convert.ToInt32(value);
            }

            if (t == typeof(TimeSpan))
            {
                if (TimeSpan.TryParse(value.ToString(), out TimeSpan timeSpan))
                {
                    return timeSpan;
                }
                else
                {
                    return null;
                }
            }

            return System.Convert.ChangeType(value, t);
        }

        /// <summary>
        /// Search for object type by Table Name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static Type GetTypesWithSqlTableAttributeByName(string name)
        {
            foreach (Type type in MySQLHandler.Types ?? Assembly.GetEntryAssembly().GetTypes())
            {
                SqlTableAttribute table = type.GetCustomAttributes(typeof(SqlTableAttribute), true).FirstOrDefault() as SqlTableAttribute;
                
                if (table?.TableName == name)
                {
                    return type;  
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieve all properties from class that behold the SqlColumnAttribute attribute
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static IEnumerable<PropertyInfo> GetPropertiesWithSqlColumnAttribute(Type type)
        {
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                object[] attributes = property.GetCustomAttributes(false);

                object column = attributes
                    .FirstOrDefault(x => x.GetType() == typeof(SqlColumnAttribute));

                if (column != null)
                {
                    yield return property;
                }
            }

            yield return null;
        }

        internal static string GetPrimaryPropertyColumnName(object obj)
        {
            return GetPrimaryPropertyColumnName(obj.GetType());
        }

        internal static string GetPrimaryPropertyColumnName(Type type)
        {
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                object[] attributes = property.GetCustomAttributes(false);

                object column = attributes.FirstOrDefault(x => x.GetType() == typeof(SqlColumnAttribute) && (x as SqlColumnAttribute).Primary);

                if (column != null)
                {
                    return (column as SqlColumnAttribute).ColumnName;
                }
            }

            throw new Exception($"Primary Property Column Not Found on object type: {type.FullName}!");
        }

        internal static PropertyInfo GetPrimaryProperty(Type type)
        {
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                object[] attributes = property.GetCustomAttributes(false);

                object column = attributes.FirstOrDefault(x => x.GetType() == typeof(SqlColumnAttribute) && (x as SqlColumnAttribute).Primary);

                if (column != null)
                {
                    return property;
                }
            }

            return null;
        }
        
        internal static PropertyInfo GetPrimaryProperty(object obj)
        {
            return GetPrimaryProperty(obj.GetType());
        }
    }
}

