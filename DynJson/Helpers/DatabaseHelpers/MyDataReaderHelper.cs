using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Data.Common;
using System.Threading;
using System.Data.SqlClient;
using DynJson.Helpers;
using System.Data.SqlClient;
using DynJson.Helpers.CoreHelpers;
using System.Collections;
using System.Dynamic;
//using ProZ.Classes.Classes;

namespace DynJson.Helpers.DatabaseHelpers
{
    public static class DbDataReaderHelper
    {
        public static void CloseIfOpen(
            this DbConnection Connection)
        {
            if (Connection != null)
            {
                if (Connection.State != ConnectionState.Closed)
                {
                    Connection.Close();
                }
            }
        }

        public static void OpenIfClosed(
            this DbConnection Connection,
            Int32 MaxTries = 1)
        {
            if (Connection != null)
            {
                if (Connection.State != ConnectionState.Open)
                {
                    Connection.Open();
                }
            }
        }

        /// <summary>
        /// pobiera wartość z datareader'a i konwertuje ją
        /// </summary>
        public static T Get<T>(
            this IDataReader Reader,
            Int32 Index)
        {
            if (Reader != null)
            {
                Object lValue = Reader.FieldCount > Index ? Reader.GetValue(Index) : null;
                if (lValue == null || lValue == DBNull.Value)
                {
                    return default(T);
                }
                else
                {
                    return MyTypeHelper.ConvertTo<T>(lValue);
                }
            }
            return default(T);
        }

        /// <summary>
        /// pobiera wartość z datareader'a i konwertuje ją
        /// </summary>
        public static T Get<T>(this IDataReader Reader, String FieldName, T MinValue)
            where T : IComparable
        {
            var lValue = Get<T>(Reader, FieldName);
            if (MinValue.CompareTo(lValue) > 0)
                return MinValue;
            return lValue;
        }

        /// <summary>
        /// pobiera wartość z datareader'a i konwertuje ją
        /// </summary>
        public static T Get<T>(this IDataReader Reader, String FieldName)
        {
            if (Reader != null)
            {
                //Object lValue = Reader[FieldName];
                var lPoz = Reader.GetOrdinal(FieldName);
                Object lValue = Reader.GetValue(lPoz);
                if (lValue == null || lValue == DBNull.Value)
                {
                    return default(T);
                }
                else
                {
                    return MyTypeHelper.ConvertTo<T>(lValue);
                }
            }
            return default(T);
        }

        public static T Select<T>(
            this DbConnection Connection,
            String Query)
            where T : class, new()
        {
            T lResult = null;
            if (Connection != null)
            {
                Connection.ExecuteReader<T>(Query, true, (r) =>
                {
                    lResult = r.MatchTo<T>();
                });
            }
            return lResult;
        }

        public static List<T> SelectMany<T>(
            this DbConnection Connection,
            String Query)
            where T : class, new()
        {
            List<T> lResult = new List<T>();
            if (Connection != null)
            {
                Connection.ExecuteReader<T>(Query, false, (r) =>
                {
                    var lItem = r.MatchTo<T>();
                    if (lItem != null)
                        lResult.Add(lItem);
                });
            }
            return lResult;
        }

        public static T SelectScalar<T>(
            this DbConnection Connection,
            String Query)
        {
            var lValue = SelectScalar(Connection, Query);
            if (lValue == null) return default(T);
            else return lValue.ConvertTo<T>();
        }

        public static Object SelectScalar(
            this DbConnection Connection,
            String Query)
        {
            Object lResult = null;
            if (Connection != null)
            {
                Connection.ExecuteReader(Query, true, (r) =>
                {
                    lResult = r.Get<Object>(0);
                    if (lResult != null && lResult.GetType() == typeof(DBNull))
                        lResult = null;
                });
            }
            return lResult;
        }

        /*
        public static void ExecuteNonQuery(
           this DbConnection Connection,
           IEnumerable<String> Queries)
        {
            if (Connection != null)
            {
                Connection.OpenIfClosed();
                foreach (var query in Queries)
                {
                    using (var lCommand = Connection.CreateCommand(query))
                    {
                        lCommand.ExecuteNonQuery();
                    }
                }
            }
        }
        */

        public static void ExecuteNonQuery(
           this DbConnection Connection,
           String Query/*,
           Int32? Timeout = null*/)
        {
            if (Connection != null)
            {
                Connection.OpenIfClosed();

                /*DateTime? start = null;
                if (InternalProfiler.IsEnabled)
                    start = DateTimeHelper.Now();*/

                using (var lCommand = Connection.CreateCommand(Query))
                {
                    //if (Timeout.HasValue)
                    //    lCommand.CommandTimeout =  Timeout.Value;
                    lCommand.ExecuteNonQuery();
                }

                /*if (InternalProfiler.IsEnabled)
                {
                    DateTime now = DateTimeHelper.Now();
                    ProfilerDataQueue.Add(
                        new ProfilerDataItem()
                        {
                            Duration = TimeSpan.FromTicks(now.Ticks - start.Value.Ticks),
                            Time = now,
                            Sql = Query
                        });
                }*/

            }
        }

        public static DataSet ExecuteToDataSet(
           this DbConnection Connection,
           String Query,
           Int32 MaxRows = 0)
        {
            DataSet dataSet = null;
            if (Connection != null)
            {
                if (String.IsNullOrEmpty(Query))
                {
                    return new DataSet();
                }
                else
                {
                    Connection.OpenIfClosed();

                    using (var lCommand = Connection.CreateCommand(Query))
                    {
                        using (var dataAdapter = CreateDataAdapter(Connection, lCommand))
                        {
                            dataSet = new DataSet();
                            if (MaxRows <= 0)
                                dataAdapter.Fill(dataSet);
                            else
                                dataAdapter.Fill(dataSet, 0, MaxRows, "TableName");
                        }
                    }
                }

                try
                {
                    List<String> names = new List<String>();
                    StringBuilder str = new StringBuilder();
                    Char prev = ' ';
                    Char prev_prev = ' ';
                    Boolean isInside = false;
                    foreach (Char curr in Query)
                    {
                        if (prev_prev == '/' && prev == '*' && curr == '*')
                        {
                            isInside = true;
                        }
                        else if (prev_prev == '*' && prev == '*' && curr == '/')
                        {
                            isInside = false;

                            String text = str.ToString().Trim();
                            names.Add(text.Substring(0, text.Length - 2));
                            str.Clear();
                        }
                        else if (isInside)
                        {
                            str.Append(curr);
                        }

                        prev_prev = prev;
                        prev = curr;
                    }

                    {
                        String text = str.ToString().Trim();
                        if (text.Length > 0)
                            names.Add(text);
                        str.Clear();
                    }

                    Int32 i = 0;
                    foreach (DataTable table in dataSet.Tables)
                    {
                        table.TableName = names[i++];
                    }
                }
                catch { }
            }
            return dataSet;
        }


        public static DataSet ExecuteToDataSet(
           this DbConnection Connection,
           params String[] Queries)
        {
            DataSet dataSet = null;
            if (Connection != null)
            {
                String Query = "";
                if (!(Queries == null || Queries.Length == 0))
                    Query = String.Join("; ", Queries);

                if (String.IsNullOrEmpty(Query))
                {
                    return new DataSet();
                }
                else
                {
                    Connection.OpenIfClosed();

                    using (var lCommand = Connection.CreateCommand(Query))
                    {
                        using (var dataAdapter = CreateDataAdapter(Connection, lCommand))
                        {
                            dataSet = new DataSet();
                            dataAdapter.Fill(dataSet);
                        }
                    }
                }
            }
            return dataSet;
        }

        private static DbDataAdapter CreateDataAdapter(DbConnection Connection, IDbCommand Command)
        {
            DbDataAdapter adapter;

            if (Connection is System.Data.SqlClient.SqlConnection)
                adapter = new System.Data.SqlClient.SqlDataAdapter(Command as System.Data.SqlClient.SqlCommand);
            //if (Connection is System.Data.Odbc.OdbcConnection)
            //    adapter = new System.Data.Odbc.OdbcDataAdapter(Command as System.Data.Odbc.OdbcCommand);
            else if (Connection is DbConnection)
                adapter = new SqlDataAdapter(Command as SqlCommand);
            else
                throw new Exception("[CreateDataAdapter] Unknown DbConnection type: " + Connection.GetType().FullName);

            return adapter;
        }

        /// <summary>
        /// wykonuje update
        /// </summary>
        public static void Update(
            this DbConnection Connection,
            String TableName,
            Object Item,
            String PrimaryKey,
            String PostfixSql)
        {
            Save(Connection, TableName, Item, PrimaryKey, false, PostfixSql, false);
        }

        /// <summary>
        /// wykonuje update
        /// </summary>
        public static Object Insert(
            this DbConnection Connection,
            String TableName,
            Object Item,
            String PrimaryKey,
            String PostfixSql,
            Boolean OverridePrimaryKey = false)
        {
            return Save(Connection, TableName, Item, PrimaryKey, true, PostfixSql, OverridePrimaryKey);
        }

        /// <summary>
        /// wykonuje update
        /// </summary>
        public static String SqlGenerateUpdate(
            this DbConnection Connection,
            String TableName,
            Object Item,
            String PrimaryKey,
            String PostfixSql)
        {
            return SqlGenerateSave(Connection, TableName, Item, PrimaryKey, false, PostfixSql, false);
        }

        /// <summary>
        /// wykonuje update
        /// </summary>
        public static String SqlGenerateInsert(
            this DbConnection Connection,
            String TableName,
            Object Item,
            String PrimaryKey,
            String PostfixSql,
            Boolean OverridePrimaryKey = false)
        {
            return SqlGenerateSave(Connection, TableName, Item, PrimaryKey, true, PostfixSql, OverridePrimaryKey);
        }

        /// <summary>
        /// wykonuje update
        /// </summary>
        public static Object Save(
            this DbConnection Connection,
            String TableName,
            Object Item,
            String PrimaryKey,
            Boolean IsInsert,
            String PostfixSql,
            Boolean OverridePrimaryKey = false)
        {
            String finalQuery = SqlGenerateSave(Connection, TableName, Item, PrimaryKey, IsInsert, PostfixSql, OverridePrimaryKey);

            if (finalQuery == null)
                return null;

            try
            {
                return Connection.SelectScalar<Object>(finalQuery);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private static IEnumerable<DbDataColumnValue> GetItemValuesForDbFields(Object Item, DbDataColumns DbFields)
        {
            if (Item is IDictionary<string, object> dictKeyValue)
            {
                foreach (var val in dictKeyValue)
                {
                    DbDataColumn dataColumn = null;
                    DbFields.TryGetValue(val.Key, out dataColumn);
                    if (dataColumn == null)
                        continue;

                    yield return new DbDataColumnValue()
                    {
                        Name = dataColumn.Name,
                        Value = val.Value
                    };
                }
            }
            else if (Item is IDictionary dict)
            {
                foreach (var key in dict.Keys)
                {
                    DbDataColumn dataColumn = null;
                    DbFields.TryGetValue(UniConvert.ToString(key), out dataColumn);
                    if (dataColumn == null)
                        continue;

                    yield return new DbDataColumnValue()
                    {
                        Name = dataColumn.Name,
                        Value = dict[key]
                    };
                }
            }
            else
            {
                foreach (DbDataColumn databaseColumn in DbFields.Values)
                {
                    if (null == DynLan.Helpers.RefUnsensitiveHelper.I.GetProperty(Item, databaseColumn.Name))
                        continue;

                    yield return new DbDataColumnValue()
                    {
                        Name = databaseColumn.Name,
                        Value = DynLan.Helpers.RefUnsensitiveHelper.I.GetValue(Item, databaseColumn.Name)
                    };
                }
            }
        }

        /// <summary>
        /// wykonuje update
        /// </summary>
        public static String SqlGenerateSave(
            this DbConnection Connection,
            String TableName,
            Object Item,
            String PrimaryKey,
            Boolean IsInsert,
            String PostfixSql,
            Boolean OverridePrimaryKey = false)
        {
            if (Connection != null)
            {
                if (String.IsNullOrEmpty(TableName))
                    throw new NotSupportedException("TableName sould not be empty!");
                //TableName = getTableName(Item.GetType());

                DbDataColumns databaseColumns = DatabaseCache.GetColumns(Connection, TableName);
                MyQuery saveQuery = new MyQuery();

                Object primaryKeyValue = ReflectionHelper. GetItemValue(Item, PrimaryKey);
                IList<DbDataColumnValue> valuesToSave = GetItemValuesForDbFields(Item, databaseColumns).ToArray();

                if (valuesToSave.Count > 0)
                {
                    if (IsInsert)
                    {
                        saveQuery.
                            Append(" insert into " + TableName + " ( ");

                        var lCount = 0;
                        foreach (DbDataColumnValue lValue in valuesToSave)
                        {
                            // dla sqlite
                            if (OverridePrimaryKey || !lValue.Name.EqualsNonsensitive(PrimaryKey))
                            {
                                if (lCount > 0) saveQuery.Append(", ");
                                saveQuery.Append(lValue.Name);
                                lCount++;
                            }
                        }

                        saveQuery.
                            Append(" ) values ( ");

                        lCount = 0;
                        foreach (DbDataColumnValue lValue in valuesToSave)
                        {
                            if (OverridePrimaryKey || !lValue.Name.EqualsNonsensitive(PrimaryKey))
                            {
                                if (lCount > 0) saveQuery.Append(", ");
                                saveQuery.AppendVal(lValue.Value);
                                lCount++;
                            }
                            // dla sqlite
                            /*else
                            {
                                if (lCount > 0) lQuery.Append(", ");
                                lQuery.AppendVal(null);
                                lCount++;
                            }*/
                        }

                        saveQuery.
                            Append(" ) ");
                    }
                    else
                    {
                        saveQuery.Append(" update " + TableName + " set ");

                        var lCount = 0;
                        foreach (DbDataColumnValue lValue in valuesToSave)
                        {
                            if (!lValue.Name.EqualsNonsensitive(PrimaryKey))
                            {
                                if (lCount > 0) saveQuery.Append(", ");
                                saveQuery.Append(lValue.Name).Append(" = ").AppendVal(lValue.Value);
                                lCount++;
                            }
                        }

                        saveQuery.Append(" where ").Append(PrimaryKey).Append(" = ").AppendVal(primaryKeyValue);
                    }

                    return saveQuery.ToString() + (PostfixSql ?? "");
                }
                else
                {
                    throw new Exception("Nie można wykonać update!");
                }
            }
            return null;
        }

        /// <summary>
        /// tworzy command'a
        /// </summary>
        public static IDbCommand CreateCommand(
            this DbConnection Connection,
            String SqlStatement)
        {
            Connection.OpenIfClosed();
            var lCommand = Connection.CreateCommand();
            lCommand.CommandText = SqlStatement;
            CorrectCommand(lCommand, Connection);
            // lCommand.Transaction = Transaction;
            return lCommand;
        }

        private static void CorrectCommand(IDbCommand Command, DbConnection Connection)
        {
            /*  var timeout = 60 * 5;
              if (Connection is DbConnection)
              {                
                  var c = Connection as DbConnection;
                  if (c.CommandTimeout > timeout || c.CommandTimeout == 0)
                  {
                      Command.CommandTimeout = c.CommandTimeout;
                  }
                  else
                  {
                      Command.CommandTimeout = timeout;
                  }
              }
              else
              {
                  Command.CommandTimeout = timeout;
              }*/
        }

        /// <summary>
        /// odpala datareader'a
        /// </summary>
        public static void Execute(
            this DbConnection Connection,
            String SqlStatement)
        {
            if (Connection != null && !String.IsNullOrEmpty(SqlStatement))
            {
                Connection.OpenIfClosed();

                /*DateTime? start = null;
                if (InternalProfiler.IsEnabled)
                    start = DateTimeHelper.Now();*/

                using (var lCommand = Connection.CreateCommand())
                {
                    lCommand.CommandText = SqlStatement;
                    CorrectCommand(lCommand, Connection);
                    lCommand.ExecuteNonQuery();
                }

                /*if (InternalProfiler.IsEnabled)
                {
                    DateTime now = DateTimeHelper.Now();
                    ProfilerDataQueue.Add(
                        new ProfilerDataItem()
                        {
                            Duration = TimeSpan.FromTicks(now.Ticks - start.Value.Ticks),
                            Time = now,
                            Sql = SqlStatement
                        });
                }*/

            }
        }

        /// <summary>
        /// Opala zapytanie
        /// </summary>
        /// <param name="SqlStatement">Zapytanie</param>
        /// <param name="timeout">Timeout dla zapytania w sekundach</param>
        /*public static void Execute(
            this DbConnection Connection,
            String SqlStatement)
        {
            if (Connection != null && !String.IsNullOrEmpty(SqlStatement))
            {
                Connection.OpenIfClosed();
                using (var lCommand = Connection.CreateCommand())
                {
                    lCommand.CommandText = SqlStatement;
                    //lCommand.CommandTimeout = timeout;
                    lCommand.ExecuteNonQuery();
                }
            }
        }*/

        public static dynamic Select(
            this DbConnection Connection,
            String Query)
        {
            dynamic lResult = null;
            if (Connection != null)
            {
                Connection.ExecuteReader(Query, true, (r) =>
                {
                    var item = new ExpandoObject() as IDictionary<string, object>;
                    for (var i = 0; i < r.FieldCount; i++)
                    {
                        Object val = r.GetValue(i);
                        String name = r.GetName(i);
                        if (val == null || val is DBNull)
                            val = null;
                        item[name.Replace(" ", "_")] = val;
                    }
                    lResult = item;
                });
            }
            return lResult;
        }

        public static List<dynamic> SelectMany(
            this DbConnection Connection,
            String Query)
        {
            List<dynamic> lResult = new List<dynamic>();
            if (Connection != null)
            {
                Connection.ExecuteReader(Query, false, (r) =>
                {
                    var item = new ExpandoObject() as IDictionary<string, object>;
                    for (var i = 0; i < r.FieldCount; i++)
                    {
                        Object val = r.GetValue(i);
                        String name = r.GetName(i);
                        if (val == null || val is DBNull)
                            val = null;
                        item[name.Replace(" ", "_")] = val;
                    }
                    lResult.Add(item);
                });
            }
            return lResult;
        }

        public static Dictionary<String, Object> SelectAsDict(
            this DbConnection Connection,
            String Query)
        {
            Dictionary<String, Object> lResult = null;
            if (Connection != null)
            {
                Connection.ExecuteReader(Query, true, (r) =>
                {
                    Dictionary<String, Object> item = new Dictionary<String, Object>();
                    for (var i = 0; i < r.FieldCount; i++)
                    {
                        Object val = r.GetValue(i);
                        String name = r.GetName(i);
                        if (val == null || val is DBNull)
                            val = null;
                        item[name.Replace(" ", "_")] = val;
                    }
                    lResult = item;
                });
            }
            return lResult;
        }

        public static List<Dictionary<String, Object>> SelectManyAsDict(
            this DbConnection Connection,
            String Query)
        {
            List<Dictionary<String, Object>> lResult = new List<Dictionary<String, Object>>();
            if (Connection != null)
            {
                Connection.ExecuteReader(Query, false, (r) =>
                {
                    Dictionary<String, Object> item = new Dictionary<String, Object>();
                    for (var i = 0; i < r.FieldCount; i++)
                    {
                        Object val = r.GetValue(i);
                        String name = r.GetName(i);
                        if (val == null || val is DBNull)
                            val = null;
                        item[name.Replace(" ", "_")] = val;
                    }
                    lResult.Add(item);
                });
            }
            return lResult;
        }

        /// <summary>
        /// odpala datareader'a
        /// </summary>
        public static void ExecuteReader(
            this DbConnection Connection,
            String SqlStatement,
            Action<IDataReader> DataReaderAction)
        {
            ExecuteReader(Connection, SqlStatement, false, DataReaderAction);
        }

        public static IList<T> SelectScalars<T>(
            this DbConnection Connection,
            String SqlStatement,
            Int32 ColumnIndex = 0)
        {
            List<T> outList = new List<T>();
            ExecuteReader(
                Connection,
                SqlStatement,
                false,
                true,
                (r) =>
                {
                    T value = (T)MyTypeHelper.ConvertTo(r.GetValue(ColumnIndex), typeof(T));
                    outList.Add(value);
                });
            return outList;
        }

        public static IList<Dictionary<String, Object>> SelectItems(
            this DbConnection Connection,
            String SqlStatement,
            Boolean LowerNames = false)
        {
            IList<Dictionary<String, Object>> outList = new List<Dictionary<String, Object>>();
            ExecuteReader(
                Connection,
                SqlStatement,
                false,
                true,
                (r) =>
                {
                    Dictionary<String, Object> item = new Dictionary<string, object>();
                    for (var i = 0; i < r.FieldCount; i++)
                    {
                        var val = r.GetValue(i);
                        if (val is DBNull) val = null;
                        var name = r.GetName(i);
                        if (string.IsNullOrEmpty(name))
                            name = "NonameColumn" + i;

                        if (LowerNames) name = name.ToLower();
                        item[name] = val;
                    }
                    outList.Add(item);
                });
            return outList;
        }

        public static IList<Dictionary<String, Object>> SelectItemsXml(
            this DbConnection Connection,
            String SqlStatement)
        {
            IList<Dictionary<String, Object>> outList = new List<Dictionary<String, Object>>();
            ExecuteXmlReader(
                Connection,
                SqlStatement,
                false,
                true,
                (r) =>
                {
                    Dictionary<String, Object> item = new Dictionary<string, object>();
                    item["xml"] = r;
                    outList.Add(item);
                });
            return outList;
        }

        /// <summary>
        /// odpala datareader'a
        /// </summary>
        public static void ExecuteReader(
            this DbConnection Connection,
            String SqlStatement,
            Boolean OneRow,
            Action<IDataReader> DataReaderAction)
        {
            ExecuteReader(Connection, SqlStatement, OneRow, false, DataReaderAction);
        }

        /// <summary>
        /// odpala datareader'a
        /// </summary>
        public static void ExecuteXmlReader(
            this DbConnection Connection,
            String SqlStatement,
            Boolean OneRow,
            Action<string> DataReaderAction)
        {
            ExecuteXmlReader(Connection, SqlStatement, OneRow, false, DataReaderAction);
        }

        /// <summary>
        /// odpala datareader'a
        /// </summary>
        public static void ExecuteXmlReader(
            this DbConnection Connection,
            String SqlStatement,
            Boolean OneRow,
            Boolean MultiResult,
            Action<string> DataReaderAction)
        {
            if (Connection != null && !String.IsNullOrEmpty(SqlStatement))
            {
                Connection.OpenIfClosed();

                using (var lCommand = Connection.CreateCommand())
                {
                    // lCommand.Transaction = Transaction;
                    lCommand.CommandText = SqlStatement;
                    CorrectCommand(lCommand, Connection);

                    if (lCommand is SqlCommand)
                    {
                        using (var lReader = (lCommand as SqlCommand).ExecuteXmlReader())
                        {
                            while (lReader.Read())
                            {
                                while (lReader.ReadState != System.Xml.ReadState.EndOfFile)
                                {
                                    if (DataReaderAction != null)
                                    {
                                        DataReaderAction(lReader.ReadOuterXml());
                                    }
                                    if (OneRow) break;
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }


            }
        }

        /// <summary>
        /// odpala datareader'a
        /// </summary>
        public static void ExecuteReader(
            this DbConnection Connection,
            String SqlStatement,
            Boolean OneRow,
            Boolean MultiResult,
            Action<IDataReader> DataReaderAction)
        {
            if (Connection != null && !String.IsNullOrEmpty(SqlStatement))
            {
                Connection.OpenIfClosed();

                /*DateTime? start = null;
                if (InternalProfiler.IsEnabled)
                    start = DateTimeHelper.Now();*/

                using (var lCommand = Connection.CreateCommand())
                {
                    // lCommand.Transaction = Transaction;
                    lCommand.CommandText = SqlStatement;
                    CorrectCommand(lCommand, Connection);
                    using (var lReader = lCommand.ExecuteReader())
                    {
                        while (true)
                        {
                            while (lReader.Read())
                            {
                                if (DataReaderAction != null)
                                {
                                    DataReaderAction(lReader);
                                }
                                if (OneRow) break;
                            }

                            if (!MultiResult || !lReader.NextResult())
                                break;
                        }
                    }
                }

                /*if (InternalProfiler.IsEnabled)
                {
                    DateTime now = DateTimeHelper.Now();
                    ProfilerDataQueue.Add(
                        new ProfilerDataItem()
                        {
                            Duration = TimeSpan.FromTicks(now.Ticks - start.Value.Ticks),
                            Time = now,
                            Sql = SqlStatement
                        });
                }*/

            }
        }

        /// <summary>
        /// odpala datareader'a
        /// </summary>
        /*public static Int32 ExecuteAndReturnAffectedRows(
            this DbConnection Connection,
            String SqlStatement)
        {
            if (Connection != null && !String.IsNullOrEmpty(SqlStatement))
            {
                Connection.OpenIfClosed();
                using (var lCommand = Connection.CreateCommand())
                {
                    // lCommand.Transaction = Transaction;
                    lCommand.CommandText = SqlStatement;
                    lCommand.CommandTimeout = 60 * 5;

                    using (var lReader = lCommand.ExecuteReader())
                    {
                        return lReader.RecordsAffected;
                    }
                }
            }
            return 0;
        }*/

        /// <summary>
        /// odpala datareader'a
        /// </summary>
        public static void ExecuteReader<T>(
            this DbConnection Connection,
            String SqlStatement,
            Boolean OneResult,
            Action<MyDbReader<T>> DataReaderAction)
        {
            if (Connection != null && !String.IsNullOrEmpty(SqlStatement))
            {
                Connection.OpenIfClosed();

                /*DateTime? start = null;
                if (InternalProfiler.IsEnabled)
                    start = DateTimeHelper.Now();*/

                using (var lCommand = Connection.CreateCommand())
                {
                    // lCommand.Transaction = Transaction;
                    lCommand.CommandText = SqlStatement;
                    CorrectCommand(lCommand, Connection);
                    var commandBehavior = OneResult ? CommandBehavior.SingleRow : CommandBehavior.Default;
                    using (var dbReader = lCommand.ExecuteReader(commandBehavior))
                    {
                        using (var reader = new MyDbReader<T>(dbReader))
                        {
                            if (DataReaderAction != null)
                            {
                                while (dbReader.Read())
                                {
                                    DataReaderAction(reader);
                                    if (OneResult) break;
                                }
                            }
                        }
                    }
                }

                /*if (InternalProfiler.IsEnabled)
                {
                    DateTime now = DateTimeHelper.Now();
                    ProfilerDataQueue.Add(
                        new ProfilerDataItem()
                        {
                            Duration = TimeSpan.FromTicks(now.Ticks - start.Value.Ticks),
                            Time = now,
                            Sql = SqlStatement
                        });
                }*/

            }
        }

        /// <summary>
        /// Kopiuje wartości z datareader'a do klasy
        /// </summary>
        public static void MatchTo<T>(
            this MyDbReader<T> DataReader,
            T Item)
            where T : new()
        {
            MatchTo<T>(DataReader, false, Item);
        }

        /// <summary>
        /// Kopiuje wartości z datareader'a do klasy
        /// </summary>
        public static T MatchTo<T>(
            this MyDbReader<T> DataReader)
            where T : new()
        {
            if (!DataReader.R.IsClosed)
            {
                T lItem = new T();
                MatchTo<T>(DataReader, false, lItem);
                return lItem;
            }
            return default(T);
        }

        /// <summary>
        /// Kopiuje wartości z datareader'a do klasy
        /// </summary>
        public static void MatchTo<T>(
            this MyDbReader<T> DataReader,
            Boolean CaseSensitive,
            T Item)
        {
            //var lProperties = typeof(T).GetProperties();
            //var lFields = typeof(T).GetFields();

            if (!DataReader.R.IsClosed)
            {
                for (int i = 0; i < DataReader.Count; i++)
                {
                    if (DataReader.DestTypes.ContainsKey(i))
                    {
                        var setter = DataReader.DestSetters[i];
                        if (setter != null)
                        {
                            setter.SetValue(
                                Item,
                                MyTypeHelper.ConvertTo(
                                    DataReader.R.Get<Object>(i),
                                    DataReader.DestTypes[i]),
                                null);
                        }
                    }
                }
            }
        }

        ////////////////////////////////////////////////////

        /*private static String getTableName(Type Type)
        {
            var lAttribute = Type.GetCustomAttributes(typeof(MyTableNameAttribute), true).FirstOrDefault() as MyTableNameAttribute;
            if (lAttribute == null)
            {
                return Type.Name;
            }
            else
            {
                return lAttribute.TableName;
            }
        }*/
    }

    /*public class MyTableNameAttribute : Attribute
    {
        public String TableName { get; set; }

        public MyTableNameAttribute(String TableName)
        {
            this.TableName = TableName;
        }
    }*/

    public class DbDataColumnValue
    {
        public String Name;

        public Boolean IsDateTime;

        public Object Value;
    }

    public class ObjectValue : IDisposable
    {
        // public Object Object;

        public Type Type;

        public String Name;

        public PropertyInfo Property;

        public FieldInfo Field;

        ////////////////

        public Object GetValue(Object Object)
        {
            if (Property != null) return Property.GetValue(Object, null);
            else if (Field != null) return Field.GetValue(Object);
            return null;
        }

        public void SetValue(Object Object, Object Value)
        {
            if (Property != null) Property.SetValue(Object, Value, null);
            else if (Field != null) Field.SetValue(Object, Value);
        }


        public void Dispose()
        {
            //Clear();
        }
    }
}
