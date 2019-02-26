using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using DynJson.Helpers;
using System.Data.SqlClient;
using System.Data;
using System.Threading;

namespace DynJson.Helpers.DatabaseHelpers
{
    public static class MyDataRowHelper
    {
        public static Object GetValue(
            this DataRow Row,
            Int32 Index)
        {
            Object value = null;
            
            if (Index < Row.ItemArray.Length && Index >= 0) 
                value = Row.ItemArray[Index];

            if (value == null || value == DBNull.Value)
                value = null;

            return value;
        }

        public static Object GetValue(
            this DataRow Row,
            String Name)
        {
            Object value = null;

            if (Row.Table.Columns.Contains(Name))
                value = Row[Name];

            if (value == null || value == DBNull.Value)
                value = null;

            return value;
        }
    }
}
