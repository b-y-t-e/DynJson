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
    public static class MyDataSetHelper
    {
        public static IEnumerable<DataRow> Where(
            this DataSet DataSet,
            Func<DataRow, Boolean> Predicate)
        {
            foreach (DataTable table in DataSet.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    if (row.ItemArray.Length > 0 && Predicate(row))
                        yield return row;
                }
            }
        }
    }
}
