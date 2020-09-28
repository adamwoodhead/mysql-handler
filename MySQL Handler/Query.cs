using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MySQL_Handler
{
    /// <summary>
    /// Available MySQL Queries
    /// </summary>
    internal static class Query
    {
        /// <summary>
        /// Select Query Builder
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectOptions"></param>
        /// <param name="convertTo"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static List<T> BuildSelect<T>(SelectOptions selectOptions, ConditionalOptions conditionalOptions, string[] columns, string[] fields, object[] values, Type convertTo = null, WhereOption whereOption = WhereOption.EQUAL, SelectLimiter limit = null)
        {
            string query;
            
            if (selectOptions == SelectOptions.FIELDS)
            {
                query = $"SELECT `{string.Join("`, `", columns)}` FROM ";
            }
            else
            {
                query = $"SELECT * FROM ";
            }

            Type type = convertTo ?? typeof(T);

            // Table Name

            if (!(type.GetCustomAttributes(typeof(SqlTableAttribute), true).FirstOrDefault() is SqlTableAttribute table))
            {
                Logger.Verbose($"SqlTableAttribute was not found on object type: {type}");
                return new List<T>();
            }

            SqlObject sqlObject = new SqlObject
            {
                Table = table.TableName
            };

            query += $"`{Configuration.MySqlDatabase}`.`{sqlObject.Table}`";

            switch (conditionalOptions)
            {
                case ConditionalOptions.NONE:
                    break;
                case ConditionalOptions.BY_ID:
                    query += $" WHERE {Converter.GetPrimaryPropertyColumnName(type)} = @param1";
                    break;
                case ConditionalOptions.WHERE:
                    query += $" WHERE ";

                    int iter = 1;
                    foreach (string fie in fields)
                    {
                        if (iter != 1)
                        {
                            query += " AND ";
                        }

                        switch (whereOption)
                        {
                            case WhereOption.EQUAL:
                                query += $"`{fie}` = @param{iter}";
                                break;
                            case WhereOption.NOTEQUAL:
                                query += $"`{fie}` != @param{iter}";
                                break;
                            case WhereOption.GREATERTHAN:
                                query += $"`{fie}` > @param{iter}";
                                break;
                            case WhereOption.LESSTHAN:
                                query += $"`{fie}` < @param{iter}";
                                break;
                            default:
                                throw new ArgumentNullException("Invalid or Missing WhereOption");
                        }

                        iter++;
                    }
                    break;
                case ConditionalOptions.WHERENULL:
                    query += $" WHERE `{fields[0]}` IS NULL";
                    break;
                default:
                    break;
            }

            if (limit != null)
            {
                query += limit.QueryData;
            }
            else
            {
                query += ";";
            }

            sqlObject.CommandText = query;

            List<T> objects;

            switch (selectOptions)
            {
                case SelectOptions.ALL:
                    switch (conditionalOptions)
                    {
                        case ConditionalOptions.NONE:
                            objects = SqlFunc.Select<T>(sqlObject, null);
                            break;
                        case ConditionalOptions.BY_ID:
                            objects = SqlFunc.Select<T>(sqlObject, null, values);
                            break;
                        case ConditionalOptions.WHERE:
                            objects = SqlFunc.Select<T>(sqlObject, null, values);
                            break;
                        case ConditionalOptions.WHERENULL:
                            objects = SqlFunc.Select<T>(sqlObject, null, values);
                            break;
                        default:
                            Logger.Verbose("Conditional Select Error");
                            objects = new List<T>();
                            break;
                    }
                    break;
                case SelectOptions.FIELDS:
                    switch (conditionalOptions)
                    {
                        case ConditionalOptions.NONE:
                            objects = SqlFunc.Select<T>(sqlObject, columns);
                            break;
                        case ConditionalOptions.BY_ID:
                            objects = SqlFunc.Select<T>(sqlObject, columns, values);
                            break;
                        case ConditionalOptions.WHERE:
                            objects = SqlFunc.Select<T>(sqlObject, columns, values);
                            break;
                        case ConditionalOptions.WHERENULL:
                            objects = SqlFunc.Select<T>(sqlObject, columns, values);
                            break;
                        default:
                            Logger.Verbose("Conditional Select Error");
                            objects = new List<T>();
                            break;
                    }
                    break;
                default:
                    Logger.Verbose("Select Type Error");
                    objects = new List<T>();
                    break;
            }

            foreach (T obj in objects)
            {
                foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    object[] attributes = property.GetCustomAttributes(false);

                    object column = attributes
                        .FirstOrDefault(x => x.GetType() == typeof(SqlColumnAttribute));

                    if (column != null)
                    {
                        SqlColumnAttribute col = column as SqlColumnAttribute;

                        // Should we be getting this derived property? If not, skip
                        if (selectOptions == SelectOptions.FIELDS && !columns.Contains(col.ColumnName))
                        {
                            continue;
                        }

                        if (col.IsTable)
                        {
                            List<object> data = BuildSelect<object>(SelectOptions.ALL, ConditionalOptions.WHERE, null, new string[] { col.AttachedTable }, new string[] { SqlFunc.ReflectPrimary(obj).ToString() }, property.PropertyType);
                            if (data.Count > 0)
                            {
                                property.SetValue(obj, data[0]);
                            }
                            else
                            {
                                Logger.Verbose($"Dataset for {property.PropertyType} had no value...");
                            }
                        }
                        else if (property.PropertyType != typeof(byte[]) && typeof(IList).IsAssignableFrom(property.PropertyType))
                        {
                            Type listOf = property.PropertyType.GetGenericArguments()[0];
                            property.SetValue(obj, Activator.CreateInstance(property.PropertyType));

                            foreach (object item in BuildSelect<object>(SelectOptions.ALL, ConditionalOptions.WHERE, null, new string[] { col.AttachedTable }, new string[] { SqlFunc.ReflectPrimary(obj).ToString() }, listOf))
                            {
                                (property.GetValue(obj) as IList).Add(item);
                            }
                        }
                    }
                }
            }

            return objects;
        }

        /// <summary>
        /// Count Query Builder
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conditionalOptions"></param>
        /// <param name="column"></param>
        /// <param name="fields"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        internal static int BuildCount<T>(ConditionalOptions conditionalOptions, string column, string[] fields, object[] values)
        {
            string query;

            if (column != null)
            {
                query = $"SELECT COUNT(`{column}`) AS `Count` FROM ";
            }
            else
            {
                query = $"SELECT COUNT(*) AS `Count` FROM ";
            }

            Type type = typeof(T);

            // Table Name

            if (!(type.GetCustomAttributes(typeof(SqlTableAttribute), true).FirstOrDefault() is SqlTableAttribute table))
            {
                Logger.Verbose($"SqlTableAttribute was not found on object type: {type}");
                return 0;
            }

            SqlObject sqlObject = new SqlObject
            {
                Table = table.TableName
            };

            query += $"`{Configuration.MySqlDatabase}`.`{sqlObject.Table}`";

            int iter = 1;
            switch (conditionalOptions)
            {
                case ConditionalOptions.NONE:
                    break;
                case ConditionalOptions.BY_ID:
                    query += $" WHERE {Converter.GetPrimaryPropertyColumnName(type)} = @param1;";
                    break;
                case ConditionalOptions.WHERE:
                    query += $" WHERE ";

                    iter = 1;
                    foreach (string fie in fields)
                    {
                        if (iter != 1)
                        {
                            query += " AND ";
                        }

                        query += $"`{fie}` = @param{iter}";
                        iter++;
                    }
                    break;
                case ConditionalOptions.WHERENULL:
                    query += $" WHERE ";

                    iter = 1;
                    foreach (string fie in fields)
                    {
                        if (iter != 1)
                        {
                            query += " AND ";
                        }

                        query += $"`{fie}` IS NULL";
                        iter++;
                    }
                    break;
                default:
                    break;
            }

            query += ";";

            sqlObject.CommandText = query;

            return SqlFunc.Count(sqlObject, values);
        }

        /// <summary>
        /// Sum Query Builder
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conditionalOptions"></param>
        /// <param name="column"></param>
        /// <param name="fields"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        internal static int BuildSum<T>(ConditionalOptions conditionalOptions, string column, string[] fields, object[] values)
        {
            string query;

            query = $"SELECT SUM(`{column}`) AS `Sum` FROM ";

            Type type = typeof(T);

            // Table Name

            if (!(type.GetCustomAttributes(typeof(SqlTableAttribute), true).FirstOrDefault() is SqlTableAttribute table))
            {
                Logger.Verbose($"SqlTableAttribute was not found on object type: {type}");
                return 0;
            }

            SqlObject sqlObject = new SqlObject
            {
                Table = table.TableName
            };

            query += $"`{Configuration.MySqlDatabase}`.`{sqlObject.Table}`";

            int iter = 1;
            switch (conditionalOptions)
            {
                case ConditionalOptions.NONE:
                    break;
                case ConditionalOptions.BY_ID:
                    query += $" WHERE {Converter.GetPrimaryPropertyColumnName(type)} = @param1;";
                    break;
                case ConditionalOptions.WHERE:
                    query += $" WHERE ";

                    iter = 1;
                    foreach (string fie in fields)
                    {
                        if (iter != 1)
                        {
                            query += " AND ";
                        }

                        query += $"`{fie}` = @param{iter}";
                        iter++;
                    }
                    break;
                case ConditionalOptions.WHERENULL:
                    query += $" WHERE ";

                    iter = 1;
                    foreach (string fie in fields)
                    {
                        if (iter != 1)
                        {
                            query += " AND ";
                        }

                        query += $"`{fie}` IS NULL";
                        iter++;
                    }
                    break;
                default:
                    break;
            }

            query += ";";

            sqlObject.CommandText = query;

            return SqlFunc.Sum(sqlObject, values);
        }

        /// <summary>
        /// Insert object into MySql Data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        internal static void BuildInsert<T>(T obj)
        {
            string query;
            List<SqlObject> sqlObjects = Converter.Convert(obj);

            foreach (SqlObject sqlObject in sqlObjects)
            {
                sqlObject.Items = sqlObject.Items.Where(x => x.Value != null).ToList();

                query = $"INSERT INTO ";

                query += $"`{Configuration.MySqlDatabase}`.`{sqlObject.Table}` (";

                int paramCount = 1;
                foreach (SqlColumn col in sqlObject.Items)
                {
                    query += $"`{col.ColumnName}`,";
                    paramCount++;
                }

                query = query.TrimEnd(',') + ") VALUES (";

                paramCount = 1;
                foreach (SqlColumn col in sqlObject.Items)
                {
                    query += $"@param{paramCount},";
                    paramCount++;
                }

                query = query.TrimEnd(',') + ") ";

                query += $"ON DUPLICATE KEY UPDATE ";

                paramCount = 1;
                foreach (SqlColumn col in sqlObject.Items)
                {
                    query += $"`{col.ColumnName}` = @param{paramCount},";
                    paramCount++;
                }

                query = query.TrimEnd(',');
                
                query += ";";

                sqlObject.CommandText = query;

                sqlObject.AttachedObject.GetType().GetProperty(Converter.GetPrimaryProperty(sqlObject.AttachedObject).Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(sqlObject.AttachedObject, SqlFunc.Row_Insert_ID(sqlObject), null);
            }
        }

        /// <summary>
        /// Update object in MySql Data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        internal static void BuildUpdate<T>(T obj, string propertyName = null)
        {
            string query;

            List<SqlObject> sqlObjects = Converter.Convert(obj);

            foreach (SqlObject sqlObject in sqlObjects)
            {
                if (!string.IsNullOrEmpty(propertyName) && !sqlObject.Items.Any(x => x.PropertyName.Contains(propertyName)))
                {
                    continue;
                }

                if (SqlFunc.ReflectPrimary(sqlObject) == null)
                {
                    Logger.Verbose("Primary is null, we need to insert, not update");
                    BuildInsert(Converter.ChangeType(sqlObject.AttachedObject, sqlObject.AttachedObject.GetType()));
                    continue;
                }

                query = $"UPDATE ";

                query += $"`{Configuration.MySqlDatabase}`.`{sqlObject.Table}` SET ";

                if (string.IsNullOrEmpty(propertyName))
                {
                    int paramCount = 1;
                    foreach (SqlColumn col in sqlObject.Items)
                    {
                        query += $"{col.ColumnName} = @param{paramCount},";
                        paramCount++;
                    }
                }
                else
                {
                    foreach (SqlColumn col in sqlObject.Items)
                    {
                        if (col.PropertyName.Contains(propertyName))
                        {
                            query += $"{col.ColumnName} = @param1";
                            break;
                        }
                    }
                }

                query = query.TrimEnd(',');

                query += $" WHERE {Converter.GetPrimaryProperty(obj).Name} = {SqlFunc.ReflectPrimary(sqlObject)};";

                sqlObject.CommandText = query;

                SqlFunc.Row_Update_ID(sqlObject, propertyName);

                if (!string.IsNullOrEmpty(propertyName))
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Drop object and all deriving items from MySql Data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        internal static void BuildDrop<T>(T obj)
        {
            string query;

            List<SqlObject> sqlObjects = Converter.Convert(obj);

            foreach (SqlObject sqlObject in sqlObjects)
            {
                if (SqlFunc.ReflectPrimary(sqlObject) == null)
                {
                    Logger.Verbose("ID is null, we can't delete this...");
                    continue;
                }

                query = $"DELETE FROM ";

                query += $"`{Configuration.MySqlDatabase}`.`{sqlObject.Table}` ";
                
                query += $"WHERE {Converter.GetPrimaryProperty(obj).Name} = {SqlFunc.ReflectPrimary(sqlObject)};";

                sqlObject.CommandText = query;

                SqlFunc.Row_Drop_ID(sqlObject);
            }
        }
    }
}