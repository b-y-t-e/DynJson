using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace DynJson.Helpers.DatabaseHelpers
{
    public class MyConnection : IDisposable
    {
        public Boolean ToDispose { get; private set; }

        public DbConnection I { get; private set; }

        /*public MyConnection()
            : this(MyConnectionCreator.Create("dellsrv", "jotl_ot_H10"))
        {

        }*/

        /*public MyConnection(String Server, String Database)
            : this(MyConnectionCreator.Create(Server, Database))
        {

        }*/
        
        public MyConnection(String ConnectionString)
            : this ((DbConnection)new SqlConnection(ConnectionString))
        {

        }

        public MyConnection(DbConnection Connection)
        {
            I = Connection;
            //this.SetProperIsolationLevel();
            ToDispose = true;
        }

        public MyConnection(MyConnection Other)
        {
            if (Other != null && Other.I != null)
            {
                this.I = Other.I;
                ToDispose = false;
            }
            else
            {
                throw new Exception("Połączenie z bazą danych nie zostało zainicjalizowane!");
                // this.SetProperIsolationLevel();
                //ToDispose = true;
            }
        }

        public void Dispose()
        {
            if (ToDispose)
            {
                try { I.Close(); }
                catch { }

                try { I.Dispose(); }
                catch { }

                I = null;
            }
        }
    }
}
