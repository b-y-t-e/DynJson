using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynJson.Helpers.DatabaseHelpers
{
    public class MyQuery
    {
        private StringBuilder _txt;

        //////////////////////////////

        public MyQueryProvider QueryProvider { get; private set; }

        public Int32 Length
        {
            get
            {
                return _txt.Length;
            }
        }

        //////////////////////////////

        public MyQuery(MyQueryProvider QueryHelper = null)
        {
            if (QueryHelper == null) this.QueryProvider = CreateQueryProvider();
            else this.QueryProvider = QueryHelper;
            _txt = new StringBuilder();
        }

        //////////////////////////////

        public MyQuery Insert(Int32 Index, String Format, params Object[] Params)
        {
            if (Params == null || Params.Length == 0)
            {
                _txt.Insert(Index, Format);
                return this;
            }
            else
            {
                _txt.Insert(Index, QueryProvider.Format(Format, Params));
                return this;
            }
        }

        public MyQuery Append(String Format, params Object[] Params)
        {
            if (Params == null || Params.Length == 0)
            {
                _txt.Append(Format);
                return this;
            }
            else
            {
                _txt.Append(QueryProvider.Format(Format, Params));
                return this;
            }
        }

        public MyQuery AppendVal(Object Obj)
        {
            _txt.Append(QueryProvider.From_val(Obj, true));
            return this;
        }

        public void Clear()
        {
            _txt.Remove(0, _txt.Length);
        }

        public override string ToString()
        {
            return _txt.ToString();
        }

        public virtual string ToStringAndClear()
        {
            var str = _txt.ToString();
            this.Clear();
            return str;
        }

        protected virtual MyQueryProvider CreateQueryProvider()
        {
            return new MyQueryProvider();
        }
    }

    public class MyQueryProvider
    {
        protected virtual String From_DateTime(DateTime? Datetime, Boolean WithQuotes)
        {
            if (Datetime.HasValue)
            {
                From_DateTime(Datetime.Value, WithQuotes);
            }
            return null;
        }

        protected virtual String FormatDatePart(Int32 Value)
        {
            StringBuilder lStr = new StringBuilder();
            lStr.Append(Value);
            if (lStr.Length == 1) lStr.Insert(0, "0");
            return lStr.ToString();
        }

        protected virtual String FormatMilisecondDatePart(Int32 Value)
        {
            StringBuilder lStr = new StringBuilder();
            lStr.Append(Value);
            if (lStr.Length == 1) lStr.Insert(0, "00");
            else if (lStr.Length == 2) lStr.Insert(0, "0");
            return lStr.ToString();
        }

        protected virtual String From_DateTime(DateTime Datetime, Boolean WithQuotes)
        {
            if (WithQuotes)
            {
                return String.Format(
                    "'{0}-{1}-{2} {3}:{4}:{5}.{6}'",
                    Datetime.Year,
                    FormatDatePart(Datetime.Month),
                    FormatDatePart(Datetime.Day),
                    FormatDatePart(Datetime.Hour),
                    FormatDatePart(Datetime.Minute),
                    FormatDatePart(Datetime.Second),
                    FormatMilisecondDatePart(Datetime.Millisecond));
            }
            else
            {
                return String.Format(
                    "{0}-{1}-{2} {3}:{4}:{5}.{6}",
                    Datetime.Year,
                    FormatDatePart(Datetime.Month),
                    FormatDatePart(Datetime.Day),
                    FormatDatePart(Datetime.Hour),
                    FormatDatePart(Datetime.Minute),
                    FormatDatePart(Datetime.Second),
                    FormatMilisecondDatePart(Datetime.Millisecond));
            }
        }

        protected virtual String From_Enum(Object EnumValue)
        {
            return Convert.ToInt32(EnumValue).ToString();  // true.Equals(BooleanValue) ? "1" : "0";
        }

        protected virtual String From_Boolean(Object BooleanValue)
        {
            return true.Equals(BooleanValue) ? "1" : "0";
        }

        protected virtual String From_Numeric(Object NumericValue)
        {
            return NumericValue.ToString().Replace(",", ".");
        }

        protected virtual String From_Int(Object NumericValue)
        {
            return NumericValue.ToString();
        }

        protected virtual String From_String(String String, Boolean WithQuotes)
        {
            if (WithQuotes)
            {
                return String.Format(
                    "'{0}'",
                    String.Replace("'", "''"));
            }
            else
            {
                return String.Replace("'", "''");
            }
        }

        protected virtual String From_Simple(Object Object)
        {
            return Object.ToString();
        }

        protected virtual String From_ByteArray(Byte[] Array, Boolean WithQuotes)
        {
            if (WithQuotes)
            {
                return String.Format(
                    "'{0}'",
                    BitConverter.ToString(Array));
            }
            else
            {
                return BitConverter.ToString(Array);
            }
        }

        public virtual String From_val(Object Obj, Boolean WithQuotes)
        {
            if (Obj == null)
            {
                return From_Simple("NULL");
            }
            else
            {
                Type t = Obj.GetType();
                if (t.IsEnum)
                {
                    return From_Enum(Obj);
                }
                else if (t == typeof(TimeSpan) || t == typeof(TimeSpan?))
                {
                    TimeSpan time = (TimeSpan)Obj;
                    var str = String.Format(
                        "'{0}:{1}:{2}.{3}'",
                        time.Days,
                        FormatDatePart(time.Minutes),
                        FormatDatePart(time.Seconds),
                        FormatMilisecondDatePart(time.Milliseconds));
                    return str;
                }
                else if (t == typeof(DateTime))
                {
                    return From_DateTime((DateTime)Obj, WithQuotes);
                }
                else if (t == typeof(DateTime?))
                {
                    return From_DateTime((DateTime?)Obj, WithQuotes);
                }
                else if (t == typeof(Boolean?) ||
                    t == typeof(Boolean))
                {
                    return From_Boolean(Obj);
                }
                else if (t == typeof(Decimal) ||
                    t == typeof(Single) ||
                    t == typeof(Double) ||
                    t == typeof(Decimal?) ||
                    t == typeof(Single?) ||
                    t == typeof(Double?))
                {
                    return From_Numeric(Obj);
                }
                else if (t == typeof(Int32) ||
                    t == typeof(Int32?) ||
                    t == typeof(Int64) ||
                    t == typeof(Int64?) ||
                    t == typeof(Int16) ||
                    t == typeof(Int16?))
                {
                    return From_Int(Obj);
                }
               /* else if (t == typeof(System.Numerics.BigInteger) ||
                    t == typeof(System.Numerics.BigInteger?))
                {
                    Int64 v = (Int64)((System.Numerics.BigInteger)Obj);
                    return From_Int(Obj);
                }
                else if (t == typeof(Microsoft.Scripting.Math.BigInteger))
                {
                    Int64 v = ((Microsoft.Scripting.Math.BigInteger)Obj).ToInt64();
                    return From_Int(Obj);
                }*/
                else if (t == typeof(Byte[]))
                {
                    return From_ByteArray((Byte[])Obj, WithQuotes);
                }
                else
                {
                    return From_String(Obj.ToString(), WithQuotes);
                }
                throw new NotSupportedException();
            }
        }

        public virtual String Format(String Format, params Object[] Params)
        {
            if (Params != null && Params.Length > 0)
            {
                Object[] lNewParams = new Object[Params.Length];
                Int32 lIndex = -1;
                foreach (var lParam in Params)
                    lNewParams[++lIndex] = From_val(lParam, true);
                return String.Format(Format, lNewParams);
            }
            else
            {
                return Format;
            }
        }
    }

}
