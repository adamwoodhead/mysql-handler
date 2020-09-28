using System;
using System.Collections.Generic;
using System.Linq;

namespace MySQL_Handler
{
    /// <summary>
    /// Contains the necessary properties & methods for SqlHandler to work, derive (T)objects from this class.
    /// </summary>
    public abstract class SqlClass<T>
    {
        #region events

        public event EventHandler Syncing;

        public event EventHandler Synced;

        public event EventHandler Inserting;

        public event EventHandler Inserted;

        public static event EventHandler Selecting;

        public static event EventHandler Selected;

        public event EventHandler Updating;

        public event EventHandler Updated;

        public event EventHandler Dropping;

        public event EventHandler Dropped;

        internal virtual void OnSyncing() => Syncing?.Invoke(this, EventArgs.Empty);

        internal virtual void OnSynced() => Synced?.Invoke(this, EventArgs.Empty);

        internal virtual void OnInserting() => Inserting?.Invoke(this, EventArgs.Empty);

        internal virtual void OnInserted() => Inserted?.Invoke(this, EventArgs.Empty);

        internal static void OnSelecting() => Selecting?.Invoke(null, EventArgs.Empty);

        internal static void OnSelected() => Selected?.Invoke(null, EventArgs.Empty);

        internal virtual void OnUpdating() => Updating?.Invoke(this, EventArgs.Empty);

        internal virtual void OnUpdated() => Updated?.Invoke(this, EventArgs.Empty);

        internal virtual void OnDropping() => Dropping?.Invoke(this, EventArgs.Empty);

        internal virtual void OnDropped() => Dropped?.Invoke(this, EventArgs.Empty);

        #endregion

        public abstract void Delete();

        public virtual (bool, string) Validate()
        {
            return (true, null);
        }

        public void UpdateField(string property)
        {
            OnUpdating();

            if (this.GetType().GetProperties().Select(x => x.Name).Any(x => x.Contains(property)) == false)
            {
                throw new ArgumentException($"Property <{property}> could not be found on Type <{this.GetType().Name}>.");
            }

            Query.BuildUpdate(this, this.GetType().GetProperties().Select(x => x.Name).FirstOrDefault(x => x.Contains(property)));

            OnUpdated();
        }

        /// <summary>
        /// Insert or Update the object in SqlHandler, depending on where it's primary exists in the related table.
        /// </summary>
        public void Sync()
        {
            OnSyncing();

            if (SqlFunc.ReflectPrimary(this) == null)
            {
                Insert();
            }
            else
            {
                Update();
            }

            OnSynced();
        }

        /// <summary>
        /// Insert the object in SqlHandler.
        /// </summary>
        internal void Insert()
        {
            OnInserting();

            Query.BuildInsert(this);

            OnInserted();
        }

        /// <summary>
        /// Update the object in SqlHandler.
        /// </summary>
        internal void Update()
        {
            OnUpdating();

            Query.BuildUpdate(this);

            OnUpdated();
        }

        /// <summary>
        /// Drop the object in SqlHandler.
        /// </summary>
        public void Drop()
        {
            OnDropping();

            Query.BuildDrop(this);

            OnDropped();
        }

        #region Selects

        /// <summary>
        /// Select *
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectOptions"></param>
        /// <param name="conditionalOptions"></param>
        /// <param name="convertTo"></param>
        /// <returns></returns>
        public static List<T> SelectAll(SelectLimiter limit = null)
        {
            return Query.BuildSelect<T>(SelectOptions.ALL, ConditionalOptions.NONE, null, null, null, null, WhereOption.NONE, limit);
        }

        /// <summary>
        /// Select *Columns*
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectOptions"></param>
        /// <param name="conditionalOptions"></param>
        /// <param name="columns"></param>
        /// <param name="convertTo"></param>
        /// <returns></returns>
        public static List<T> SelectFields(string[] columns, SelectLimiter limit = null)
        {
            return Query.BuildSelect<T>(SelectOptions.FIELDS, ConditionalOptions.NONE, columns, null, null, null, WhereOption.NONE, limit);
        }
        
        /// <summary>
        /// Select *Columns*
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectOptions"></param>
        /// <param name="conditionalOptions"></param>
        /// <param name="columns"></param>
        /// <param name="convertTo"></param>
        /// <returns></returns>
        public static List<T> SelectFields(string columns, SelectLimiter limit = null)
        {
            return SelectFields(new string[] { columns }, limit);
        }

        /// <summary>
        /// Select * Where primary Equals *IDValue*
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="idvalue"></param>
        /// <param name="convertTo"></param>
        /// <returns></returns>
        public static T SelectSingleByPrimary(object idvalue)
        {
            return Query.BuildSelect<T>(SelectOptions.ALL, ConditionalOptions.BY_ID, null, null, new object[] { idvalue }, null, WhereOption.NONE, new SelectLimiter(1)).FirstOrDefault();
        }

        /// <summary>
        /// First of - Select *Columns* Where primary Equals *IDValue*
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columns"></param>
        /// <param name="idvalue"></param>
        /// <param name="convertTo"></param>
        /// <returns></returns>
        public static T SelectFieldsSingleByPrimary(string[] columns, object idvalue)
        {
            return Query.BuildSelect<T>(SelectOptions.FIELDS, ConditionalOptions.BY_ID, columns, null, new object[] { idvalue }, null, WhereOption.NONE, new SelectLimiter(1)).FirstOrDefault();
        }

        /// <summary>
        /// First of - Select *Columns* Where primary Equals *IDValue*
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columns"></param>
        /// <param name="idvalue"></param>
        /// <param name="convertTo"></param>
        /// <returns></returns>
        public static T SelectFieldSingleByPrimary(string column, object idvalue)
        {
            return SelectFieldsSingleByPrimary(new string[] { column }, idvalue);
        }

        /// <summary>
        /// Select *Columns* Where *Fields* Equal *Values*
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectOptions"></param>
        /// <param name="conditionalOptions"></param>
        /// <param name="columns"></param>
        /// <param name="fields"></param>
        /// <param name="values"></param>
        /// <param name="convertTo"></param>
        /// <returns></returns>
        public static List<T> SelectFieldsWhere(string[] columns, string[] fields, object[] values, WhereOption whereOption = WhereOption.EQUAL, SelectLimiter limit = null)
        {
            return Query.BuildSelect<T>(SelectOptions.FIELDS, ConditionalOptions.WHERE, columns, fields, values, null, whereOption, limit);
        }

        /// <summary>
        /// Select *Column* Where *Fields* Equal *Values*
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectOptions"></param>
        /// <param name="conditionalOptions"></param>
        /// <param name="columns"></param>
        /// <param name="fields"></param>
        /// <param name="values"></param>
        /// <param name="convertTo"></param>
        /// <returns></returns>
        public static List<T> SelectFieldWhere(string column, string[] fields, object[] values, WhereOption whereOption = WhereOption.EQUAL, SelectLimiter limit = null)
        {
            return SelectFieldsWhere(new string[] { column }, fields, values, whereOption, limit);
        }

        /// <summary>
        /// Select *Column* Where *Fields* Equal *Values*
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectOptions"></param>
        /// <param name="conditionalOptions"></param>
        /// <param name="columns"></param>
        /// <param name="fields"></param>
        /// <param name="values"></param>
        /// <param name="convertTo"></param>
        /// <returns></returns>
        public static List<T> SelectFieldsWhere(string[] columns, string field, object value, WhereOption whereOption = WhereOption.EQUAL, SelectLimiter limit = null)
        {
            return SelectFieldsWhere(columns, new string[] { field }, new object[] { value }, whereOption, limit);
        }

        /// <summary>
        /// Select *Column* Where *Fields* Equal *Values*
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectOptions"></param>
        /// <param name="conditionalOptions"></param>
        /// <param name="columns"></param>
        /// <param name="fields"></param>
        /// <param name="values"></param>
        /// <param name="convertTo"></param>
        /// <returns></returns>
        public static List<T> SelectFieldWhere(string column, string field, object value, WhereOption whereOption = WhereOption.EQUAL, SelectLimiter limit = null)
        {
            return SelectFieldsWhere(new string[] { column }, new string[] { field }, new object[] { value }, whereOption, limit);
        }

        /// <summary>
        /// Select * Where *Fields* Equal *Values*
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectOptions"></param>
        /// <param name="conditionalOptions"></param>
        /// <param name="fields"></param>
        /// <param name="values"></param>
        /// <param name="convertTo"></param>
        /// <returns></returns>
        public static List<T> SelectAllWhere(string[] fields, object[] values, WhereOption whereOption = WhereOption.EQUAL, SelectLimiter limit = null)
        {
            return Query.BuildSelect<T>(SelectOptions.ALL, ConditionalOptions.WHERE, null, fields, values, null, whereOption, limit);
        }

        /// <summary>
        /// Select * Where *Field* Equals *Value*
        /// </summary>
        /// <param name="field"></param>
        /// <param name="values"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static List<T> SelectAllWhere(string field, object values, WhereOption whereOption = WhereOption.EQUAL, SelectLimiter limit = null)
        {
            return Query.BuildSelect<T>(SelectOptions.ALL, ConditionalOptions.WHERE, null, new string[] { field }, new object[] { values }, null, whereOption, limit);
        }

        /// <summary>
        /// Select * Where *Fields* Equal Null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectOptions"></param>
        /// <param name="conditionalOptions"></param>
        /// <param name="columns"></param>
        /// <param name="convertTo"></param>
        /// <returns></returns>
        public static List<T> SelectAllWhereFieldNull(string[] fields, SelectLimiter limit = null)
        {
            return Query.BuildSelect<T>(SelectOptions.ALL, ConditionalOptions.WHERENULL, null, fields, null, null, WhereOption.NONE, limit);
        }

        /// <summary>
        /// Select * Where *Fields* Equal Null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectOptions"></param>
        /// <param name="conditionalOptions"></param>
        /// <param name="columns"></param>
        /// <param name="convertTo"></param>
        /// <returns></returns>
        public static List<T> SelectAllWhereFieldNull(string field, SelectLimiter limit = null)
        {
            return SelectAllWhereFieldNull(new string[] { field }, limit);
        }

        /// <summary>
        /// Select *Columns* Where *Fields* Equal Null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectOptions"></param>
        /// <param name="conditionalOptions"></param>
        /// <param name="columns"></param>
        /// <param name="fields"></param>
        /// <param name="convertTo"></param>
        /// <returns></returns>
        public static List<T> SelectFieldsWhereFieldNull(string[] columns, string[] fields, SelectLimiter limit = null)
        {
            return Query.BuildSelect<T>(SelectOptions.FIELDS, ConditionalOptions.WHERENULL, columns, fields, null, null, WhereOption.NONE, limit);
        }

        /// <summary>
        /// Select *Columns* Where *Fields* Equal Null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectOptions"></param>
        /// <param name="conditionalOptions"></param>
        /// <param name="column"></param>
        /// <param name="fields"></param>
        /// <param name="convertTo"></param>
        /// <returns></returns>
        public static List<T> SelectFieldWhereFieldNull(string column, string field, SelectLimiter limit = null)
        {
            return SelectFieldsWhereFieldNull(new string[] { column }, new string[] { field }, limit);
        }

        #endregion

        #region Count

        public static int CountAll()
        {
            return Query.BuildCount<T>(ConditionalOptions.NONE, null, null, null);
        }

        public static int CountField(string column)
        {
            return Query.BuildCount<T>(ConditionalOptions.NONE, column, null, null);
        }

        public static int CountWhere(string[] fields, object[] values)
        {
            return Query.BuildCount<T>(ConditionalOptions.WHERE, null, fields, values);
        }

        public static int CountWhere(string field, object value)
        {
            return CountWhere(new string[] { field }, new object[] { value });
        }

        public static int CountFieldWhere(string column, string[] fields, object[] values)
        {
            return Query.BuildCount<T>(ConditionalOptions.WHERE, column, fields, values);
        }

        public static int CountFieldWhere(string column, string field, object value)
        {
            return CountFieldWhere(column, new string[] { field }, new object[] { value });
        }
        
        public static int CountFieldWhereFieldNull(string column, string[] fields)
        {
            return Query.BuildCount<T>(ConditionalOptions.WHERENULL, column, fields, null);
        }

        public static int CountFieldWhereFieldNull(string column, string field)
        {
            return CountFieldWhereFieldNull(column, new string[] { field });
        }

        #endregion

        #region Sum

        public static int SumField(string column)
        {
            return Query.BuildSum<T>(ConditionalOptions.NONE, column, null, null);
        }

        public static int SumFieldWhere(string column, string[] fields, object[] values)
        {
            return Query.BuildSum<T>(ConditionalOptions.WHERE, column, fields, values);
        }

        public static int SumFieldWhere(string column, string field, object value)
        {
            return CountFieldWhere(column, new string[] { field }, new object[] { value });
        }

        public static int SumFieldWhereFieldNull(string column, string[] fields)
        {
            return Query.BuildSum<T>(ConditionalOptions.WHERENULL, column, fields, null);
        }

        public static int SumFieldWhereFieldNull(string column, string field)
        {
            return SumFieldWhereFieldNull(column, new string[] { field });
        }

        #endregion
    }
}