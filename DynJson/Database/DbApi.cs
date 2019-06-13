using DynJson.Helpers.CoreHelpers;
using DynJson.Helpers.DatabaseHelpers;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using DynJson.Classes;
using DynJson.Helpers;
using System.Diagnostics;

namespace DynJson.Database
{
    public class DbApi
    {
        private String idName = "ID";

        //////////////////////////////////////////////

        public string connectionString { get; set; }

        public string dbName { get; set; }

        //////////////////////////////////////////////

        public DbApi(string connectionString, string dbName)
        {
            this.connectionString = connectionString;
            this.dbName = dbName;
        }

        //////////////////////////////////////////////

        public void execute(object sqlquery)
        {
            Stopwatch st = Stopwatch.StartNew();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.ExecuteNonQuery(sqlquery.ToString());
                }
            }
            catch (Exception ex)
            {
                if (Logger.IsEnabled)
                    Logger.LogError("SQL-" + dbName, "execute", ex.Message, sqlquery.ToString());
                throw;
            }
            finally
            {
                if (Logger.IsEnabled)
                    Logger.LogPerformance("SQL-" + dbName, "execute", st.ElapsedMilliseconds, sqlquery.ToString());
            }
        }

        public object single(object sqlquery)
        {
            Stopwatch st = Stopwatch.StartNew();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    Dictionary<string, object> item = con.SelectAsDict(sqlquery.ToString());
                    return item;
                }
            }
            catch (Exception ex)
            {
                if (Logger.IsEnabled)
                    Logger.LogError("SQL-" + dbName, "single", ex.Message, sqlquery.ToString());
                throw;
            }
            finally
            {
                if (Logger.IsEnabled)
                    Logger.LogPerformance("SQL-" + dbName, "single", st.ElapsedMilliseconds, sqlquery.ToString());
            }
        }

        public object many(object sqlquery)
        {
            Stopwatch st = Stopwatch.StartNew();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    List<Dictionary<string, object>> items = con.SelectManyAsDict(sqlquery.ToString());
                    return items;
                }
            }
            catch (Exception ex)
            {
                if (Logger.IsEnabled)
                    Logger.LogError("SQL-" + dbName, "many", ex.Message, sqlquery.ToString());
                throw;
            }
            finally
            {
                if (Logger.IsEnabled)
                    Logger.LogPerformance("SQL-" + dbName, "many", st.ElapsedMilliseconds, sqlquery.ToString());
            }
        }

        public object value(object sqlquery)
        {
            Stopwatch st = Stopwatch.StartNew();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    object value = con.SelectScalar(sqlquery.ToString());
                    return value;
                }
            }
            catch (Exception ex)
            {
                if (Logger.IsEnabled)
                    Logger.LogError("SQL-" + dbName, "value", ex.Message, sqlquery.ToString());
                throw;
            }
            finally
            {
                if (Logger.IsEnabled)
                    Logger.LogPerformance("SQL-" + dbName, "value", st.ElapsedMilliseconds, sqlquery.ToString());
            }
        }

        public IList<object> values(object sqlquery)
        {
            Stopwatch st = Stopwatch.StartNew();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    IList<object> values = con.SelectScalars<object>(sqlquery.ToString());
                    return values;
                }
            }
            catch (Exception ex)
            {
                if (Logger.IsEnabled)
                    Logger.LogError("SQL-" + dbName, "values", ex.Message, sqlquery.ToString());
                throw;
            }
            finally
            {
                if (Logger.IsEnabled)
                    Logger.LogPerformance("SQL-" + dbName, "values", st.ElapsedMilliseconds, sqlquery.ToString());
            }
        }

        //////////////////////////////////////////////

        public Object save(String TableName, Object ItemOrItems)
        {
            Stopwatch st = Stopwatch.StartNew();
            IList<Object> items = null;
            try
            {
                items = convertToList(ItemOrItems);
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    saveItems(con, TableName, items);
                }
                return ItemOrItems;
            }
            catch (Exception ex)
            {
                if (Logger.IsEnabled)
                    Logger.LogError("SQL-" + dbName, "save", ex.Message, $"{TableName} / {ItemOrItems.SerializeJson() ?? ""}");
                throw;
            }
            finally
            {
                if (Logger.IsEnabled)
                    Logger.LogPerformance("SQL-" + dbName, "save", st.ElapsedMilliseconds, $"{TableName} / {items?.Count ?? 0}");
            }
        }

        public Object savechildren(String TableName, Object ItemOrItems, String ParentPropertyName)
        {
            Stopwatch st = Stopwatch.StartNew();
            IList<Object> items = null;
            try
            {
                items = convertToList(ItemOrItems);
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    IList<Object> ids = getIDs(items);

                    object parentPropertyValue = null;
                    foreach (Object item in items)
                    {
                        parentPropertyValue = ReflectionHelper.GetItemValue(item, ParentPropertyName);
                        if (parentPropertyValue != null) break;
                    }

                    deleteItemsByParentPropertyExceptIDs(con, TableName, ids, ParentPropertyName, parentPropertyValue);
                    saveItems(con, TableName, items);
                }
                return ItemOrItems;
            }
            catch (Exception ex)
            {
                if (Logger.IsEnabled)
                    Logger.LogError("SQL-" + dbName, "savechildren#1", ex.Message, $"{TableName} / {ItemOrItems.SerializeJson() ?? ""}");
                throw;
            }
            finally
            {
                if (Logger.IsEnabled)
                    Logger.LogPerformance("SQL-" + dbName, "savechildren#1", st.ElapsedMilliseconds, $"{TableName} / {items?.Count ?? 0}");
            }
        }

        public Object savechildren(String TableName, Object ItemOrItems, String ParentPropertyName, Object ParentPropertyValue)
        {
            Stopwatch st = Stopwatch.StartNew();
            IList<Object> items = null;
            try
            {
                items = convertToList(ItemOrItems);
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    IList<Object> ids = getIDs(items);

                    foreach (Object item in items)
                        ReflectionHelper.SetItemValue(item, ParentPropertyName, ParentPropertyValue);

                    deleteItemsByParentPropertyExceptIDs(con, TableName, ids, ParentPropertyName, ParentPropertyValue);
                    saveItems(con, TableName, items);
                }
                return ItemOrItems;
            }
            catch (Exception ex)
            {
                if (Logger.IsEnabled)
                    Logger.LogError("SQL-" + dbName, "savechildren#2", ex.Message, $"{TableName} / {ItemOrItems.SerializeJson() ?? ""}");
                throw;
            }
            finally
            {
                if (Logger.IsEnabled)
                    Logger.LogPerformance("SQL-" + dbName, "savechildren#2", st.ElapsedMilliseconds, $"{TableName} / {items?.Count ?? 0}");
            }
        }

        //////////////////////////////////////////////

        IList<Object> convertToList(Object ItemOrItems)
        {
            IList<Object> items = null;
            if (!MyTypeHelper.IsListOrArray(ItemOrItems))
            {
                items = new[] { ItemOrItems };
            }
            else
            {
                items = new List<Object>();
                foreach (var item in ItemOrItems as IList)
                    items.Add(item);
            }
            return items;
        }

        IList<Object> getIDs(IList<Object> Items)
        {
            Object[] ids = new object[Items.Count];
            for (var i = 0; i < Items.Count; i++)
                ids[i++] = ReflectionHelper.GetItemValue(Items[i], this.idName);
            return ids;
        }

        //////////////////////////////////////////////

        void saveItems(SqlConnection con, String TableName, IList<Object> Items)
        {
            foreach (Object obj in Items)
                saveItem(con, TableName, obj);
        }

        Object saveItem(SqlConnection con, String TableName, Object Item)
        {
            Object idValue = ReflectionHelper.
                GetItemValue(Item, idName);

            if (!itemExists(con, TableName, idValue))
            {
                Boolean overrideKey = !DynLan.Helpers.MyTypeHelper.IsNumeric(idValue);

                Object newID = con.Insert(TableName, Item, idName, "select SCOPE_IDENTITY()", overrideKey);
                if (newID != null)
                    ReflectionHelper.SetItemValue(Item, this.idName, newID);
            }
            else
            {
                con.Update(TableName, Item, idName, "");
            }
            return null;
        }

        bool itemExists(SqlConnection con, String TableName, Object ItemID)
        {
            MyQuery query = new MyQuery();
            query.Append("select 1 from " + TableName + " where " + idName + " = {0}", ItemID);

            return con.SelectScalar<object>(query.ToString()) != null;
        }

        void deleteItemsByParentPropertyExceptIDs(SqlConnection con, String TableName, IList<Object> IDs, String ParentPropertyName, Object ParentPropertyValue)
        {
            var notNullIDs = IDs.
                Where(i => i != null).
                ToArray();

            if (notNullIDs.Length == 0)
            {
                MyQuery query = new MyQuery();
                query.Append("delete from " + TableName + " where " + ParentPropertyName + " = {0} ", ParentPropertyValue);
                con.ExecuteNonQuery(query.ToString());
            }
            else
            {
                MyQuery query = new MyQuery();
                query.Append("delete from " + TableName + " where " + ParentPropertyName + " = {0} and id not in (" + notNullIDs.JoinSql() + ") ", ParentPropertyValue);
                con.ExecuteNonQuery(query.ToString());
            }
        }
    }
}
