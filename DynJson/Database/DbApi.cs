using DynJson.Helpers.DatabaseHelpers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace DynJson.Database
{
    public class DbApi
    {
        public String ConnectionString { get; set; }

        public DbApi(String ConnectionString)
        {
            this.ConnectionString = ConnectionString;
        }

        public Object save(String TableName, Object Object)
        {
            MyQuery query = new MyQuery();
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Save(TableName, Object, "ID", true, "");
                //var result = con.SelectItems(query.ToString());
                //return result;
                return null;
            }
        }
    }
}
