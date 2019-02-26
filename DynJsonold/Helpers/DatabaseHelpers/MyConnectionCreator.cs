using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using DynJson;
using DynJson.Helpers;

namespace DynJson.Helpers.DatabaseHelpers
{
    public class MyConnectionCreator
    {
        public static MyConnection Create()
        {
            //throw new NotImplementedException();
            return new MyConnection(SettingsHelper.Get("sqlConnectionString")); // Globals.CONNECTION_STRING, Globals.ODBC);
        }
    }
}
