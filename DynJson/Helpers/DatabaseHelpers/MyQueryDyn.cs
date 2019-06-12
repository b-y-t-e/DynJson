using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynJson.Helpers.DatabaseHelpers
{
    public class SqlBuilder
    {
        private StringBuilder _txt;

        //////////////////////////////

        public MyQueryProvider QueryProvider { get; private set; }

        public Int32 length
        {
            get
            {
                return _txt.Length;
            }
        }

        //////////////////////////////

        public SqlBuilder(MyQueryProvider QueryHelper = null)
        {
            if (QueryHelper == null) this.QueryProvider = CreateQueryProvider();
            else this.QueryProvider = QueryHelper;
            _txt = new StringBuilder();
        }

        //////////////////////////////

        public SqlBuilder append(String Format)
        {
            _txt.Append(Format);
            return this;
        }

        public SqlBuilder append(String Format, Object Param1)
        {
            _txt.Append(QueryProvider.Format(Format, new[] { Param1 }));
            return this;
        }

        public SqlBuilder append(String Format, Object Param1, Object Param2)
        {
            _txt.Append(QueryProvider.Format(Format, new[] { Param1, Param2 }));
            return this;
        }

        public SqlBuilder append(String Format, Object Param1, Object Param2, Object Param3)
        {
            _txt.Append(QueryProvider.Format(Format, new[] { Param1, Param2, Param2, Param3 }));
            return this;
        }

        public SqlBuilder append(String Format, Object Param1, Object Param2, Object Param3, Object Param4)
        {
            _txt.Append(QueryProvider.Format(Format, new[] { Param1, Param2, Param2, Param3, Param4 }));
            return this;
        }

        public SqlBuilder append(String Format, Object Param1, Object Param2, Object Param3, Object Param4, Object Param5)
        {
            _txt.Append(QueryProvider.Format(Format, new[] { Param1, Param2, Param2, Param3, Param4, Param5 }));
            return this;
        }

        public SqlBuilder append(String Format, Object Param1, Object Param2, Object Param3, Object Param4, Object Param5, Object Param6)
        {
            _txt.Append(QueryProvider.Format(Format, new[] { Param1, Param2, Param2, Param3, Param4, Param5, Param6 }));
            return this;
        }

        public SqlBuilder append(String Format, Object Param1, Object Param2, Object Param3, Object Param4, Object Param5, Object Param6, Object Param7)
        {
            _txt.Append(QueryProvider.Format(Format, new[] { Param1, Param2, Param2, Param3, Param4, Param5, Param6, Param7 }));
            return this;
        }

        public SqlBuilder append(String Format, Object Param1, Object Param2, Object Param3, Object Param4, Object Param5, Object Param6, Object Param7, Object Param8)
        {
            _txt.Append(QueryProvider.Format(Format, new[] { Param1, Param2, Param2, Param3, Param4, Param5, Param6, Param7, Param8 }));
            return this;
        }
        public SqlBuilder append(String Format, Object Param1, Object Param2, Object Param3, Object Param4, Object Param5, Object Param6, Object Param7, Object Param8, Object Param9)
        {
            _txt.Append(QueryProvider.Format(Format, new[] { Param1, Param2, Param2, Param3, Param4, Param5, Param6, Param7, Param8, Param9 }));
            return this;
        }
        public SqlBuilder append(String Format, Object Param1, Object Param2, Object Param3, Object Param4, Object Param5, Object Param6, Object Param7, Object Param8, Object Param9, Object Param10)
        {
            _txt.Append(QueryProvider.Format(Format, new[] { Param1, Param2, Param2, Param3, Param4, Param5, Param6, Param7, Param8, Param9, Param10 }));
            return this;
        }

        public SqlBuilder appendval(Object Obj)
        {
            _txt.Append(QueryProvider.From_val(Obj, true));
            return this;
        }

        public void clear()
        {
            _txt.Remove(0, _txt.Length);
        }

        public string tostring()
        {
            return _txt.ToString();
        }

        public override string ToString()
        {
            return _txt.ToString();
        }
        
        protected virtual MyQueryProvider CreateQueryProvider()
        {
            return new MyQueryProvider();
        }
    }

}
