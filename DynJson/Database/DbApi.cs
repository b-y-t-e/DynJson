using DynJson.Helpers.CoreHelpers;
using DynJson.Helpers.DatabaseHelpers;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace DynJson.Database
{
    public class DbApi
    {
        private String idName = "ID";

        //////////////////////////////////////////////

        public String ConnectionString { get; set; }

        //////////////////////////////////////////////

        public DbApi(String ConnectionString)
        {
            this.ConnectionString = ConnectionString;
        }

        //////////////////////////////////////////////

        public Dictionary<string, object> select(MyQueryDyn sqlquery)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                Dictionary<string, object> item = con.SelectAsDict(sqlquery.ToString());
                return item;
            }
        }

        public List<Dictionary<string, object>> selectmany(MyQueryDyn sqlquery)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                List<Dictionary<string, object>> items = con.SelectManyAsDict(sqlquery.ToString());
                return items;
            }
        }

        public object selectscalar(MyQueryDyn sqlquery)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                object value = con.SelectScalar(sqlquery.ToString());
                return value;
            }
        }

        public IList<object> selectscalars(MyQueryDyn sqlquery)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                IList<object> values = con.SelectScalars<object>(sqlquery.ToString());
                return values;
            }
        }
        //////////////////////////////////////////////

        public Dictionary<string, object> select(string sqlquery)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                Dictionary<string, object> item = con.SelectAsDict(sqlquery);
                return item;
            }
        }

        public List<Dictionary<string, object>> selectmany(string sqlquery)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                List<Dictionary<string, object>> items = con.SelectManyAsDict(sqlquery);
                return items;
            }
        }

        public object selectscalar(string sqlquery)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                object value = con.SelectScalar(sqlquery);
                return value;
            }
        }

        public IList<object> selectscalars(string sqlquery)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                IList<object> values = con.SelectScalars<object>(sqlquery);
                return values;
            }
        }

        //////////////////////////////////////////////

        public Object save(String TableName, Object ItemOrItems)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                IList<Object> items = convertToList(ItemOrItems);
                saveItems(con, TableName, items);
            }
            return null;
        }

        public Object savechildren(String TableName, Object ItemOrItems, String ParentPropertyName)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                IList<Object> items = convertToList(ItemOrItems);
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
            return null;
        }

        public Object savechildren(String TableName, Object ItemOrItems, String ParentPropertyName, Object ParentPropertyValue)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                IList<Object> items = convertToList(ItemOrItems);
                IList<Object> ids = getIDs(items);

                foreach (Object item in items)
                    ReflectionHelper.SetItemValue(item, ParentPropertyName, ParentPropertyValue);

                deleteItemsByParentPropertyExceptIDs(con, TableName, ids, ParentPropertyName, ParentPropertyValue);
                saveItems(con, TableName, items);
            }
            return null;
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
            if (!itemExists(con, TableName, Item))
            {
                Object newID = con.Insert(TableName, Item, idName, "select SCOPE_IDENTITY()");
                if (newID != null)
                    ReflectionHelper.SetItemValue(Item, this.idName, newID);
            }
            else
            {
                con.Update(TableName, Item, idName, "");
            }
            return null;
        }

        bool itemExists(SqlConnection con, String TableName, Object Item)
        {
            Object idValue = ReflectionHelper.
                GetItemValue(Item, idName);

            MyQuery query = new MyQuery();
            query.Append("select 1 from " + TableName + " where " + idName + " = {0}", idValue);

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
