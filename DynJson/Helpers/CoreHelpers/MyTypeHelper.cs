using System;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace DynJson.Helpers.CoreHelpers
{
    public static class MyTypeHelper
    {
        public static Boolean IsPrimitive(this Object Object)
        {
            if (Object == null)
                return true;

            return IsPrimitive(Object.GetType());
        }

        public static Boolean IsPrimitive(this Type Type)
        {
            return Type.IsPrimitive ||
                Type.IsEnum ||
                Type.IsValueType ||
                Type == typeof(String);
        }

        public static bool IsString(this Type Type)
        {
            return (Type == typeof(String));
        }

        public static bool IsClass(this Type Type)
        {
            return (Type != typeof(String)) && Type.IsClass;
        }

        public static bool IsDateTime(this Type Type)
        {
            return (Type == typeof(DateTime) ||
                Type == typeof(DateTime?));
        }

        public static bool IsTimeSpan(this Type Type)
        {
            return (Type == typeof(TimeSpan) ||
                Type == typeof(TimeSpan?));
        }

        public static bool IsNumeric(this Type Type)
        {
            if (Type == typeof(Decimal) ||
                Type == typeof(Decimal?) ||
                Type == typeof(Int32) ||
                Type == typeof(Int32?) ||
                Type == typeof(Int16) ||
                Type == typeof(Int16?) ||
                Type == typeof(Int64) ||
                Type == typeof(Int64?) ||
                Type == typeof(Single) ||
                Type == typeof(Single?) ||
                Type == typeof(Double) ||
                Type == typeof(Double?) ||
                Type == typeof(Byte) ||
                Type == typeof(Byte?) ||
                Type == typeof(SByte?) ||
                Type == typeof(SByte) ||
                Type == typeof(SByte?) ||
                Type == typeof(Boolean) ||
                Type == typeof(Boolean?))
                return true;
            return false;
        }

        public static bool IsDecimal(this Type Type)
        {
            if (Type == typeof(Decimal) ||
                Type == typeof(Decimal?))
                return true;
            return false;
        }

        public static bool IsBoolean(this Type Type)
        {
            if (Type == typeof(Boolean) ||
                Type == typeof(Boolean?))
                return true;
            return false;
        }

        public static bool IsInteger(this Type Type)
        {
            if (Type == typeof(Int32) ||
                Type == typeof(Int32?) ||
                Type == typeof(Int16) ||
                Type == typeof(Int16?) ||
                Type == typeof(Int64) ||
                Type == typeof(Int64?) ||
                Type == typeof(Byte) ||
                Type == typeof(Byte?) ||
                Type == typeof(SByte?) ||
                Type == typeof(SByte) ||
                Type == typeof(SByte?) ||
                Type == typeof(Boolean) ||
                Type == typeof(Boolean?))
                return true;
            return false;
        }

        ////////////////////

        public static Boolean IsEqualWithNumericConvert(this Object o1, Object o2)
        {
            if (o1 == o2) return true;
            else if (o1 != null && o2 == null) return false;
            else if (o1 == null && o2 != null) return false;
            else if (o1.Equals(o2)) return true;
            else
            {
                var t1 = o1.GetType();
                var t2 = o2.GetType();
                if (IsNumeric(t1) && IsNumeric(t2))
                {
                    var v1 = Convert.ToDecimal(o1);
                    var v2 = Convert.ToDecimal(o2);
                    return v1.Equals(v2);
                }
                else
                {
                    return false;
                }
            }
        }

        public static Boolean IsEqual(this Object Obj1, Object Obj2)
        {
            if (Obj1 == Obj2) return true;
            else if (Obj1 != null && Obj2 == null) return false;
            else if (Obj1 == null && Obj2 != null) return false;
            else return Obj1.Equals(Obj2);
        }

        public static Boolean EqualIn(this Object Object, params Object[] Values)
        {
            if (Values != null)
            {
                foreach (var lValue in Values)
                    if (Object.IsEqual(lValue))
                        return true;
            }
            return false;
        }

        ////////////////////

        public static Boolean Is(this Type Type, Type Subclass)
        {
            if (Subclass.IsInterface)
            {
                return Type.GetInterfaces().Contains(Subclass);
            }
            else
            {
                return PrivIs(Type, Subclass);
            }
        }

        private static Boolean PrivIs(Type Type, Type Subclass)
        {
            if (Type == Subclass)
                return true;
            else
            {
                var lBase = Type.BaseType;
                if (lBase != null)
                    return PrivIs(lBase, Subclass);
                return false;
            }
        }

        ////////////////////

        public static Boolean IsListOrArray(this Object Obj)
        {
            return IsListOrArray(Obj?.GetType());
        }

        public static Boolean IsListOrArray(this Type Type)
        {
            if (Type == null)
                return false;

            else if (Type == typeof(string))
                return false;

            else if (Type.IsArray)
                return true;

            else if (Type.Is(typeof(IList)))
                return true;

            else
                return false;
        }

        public static Boolean IsEnumerable(this Object Obj)
        {
            return IsEnumerable(Obj.GetType());
        }

        public static Boolean IsEnumerable(this Type Type)
        {
            return Type.Is(typeof(IEnumerable));
        }

        public static Type GetCollectionElementType(this Type seqType)
        {
            if (seqType == null || seqType == typeof(string))
            {
                return null;
            }
            else if (seqType.IsArray)
            {
                return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
            }
            else if (seqType.IsGenericType &&
                        seqType.GetGenericTypeDefinition() == typeof(IList<>))
            {
                return seqType.GetGenericArguments()[0]; // use this...
            }
            else
            {
                foreach (Type interfaceType in seqType.GetInterfaces())
                {
                    if (interfaceType.IsGenericType &&
                        interfaceType.GetGenericTypeDefinition() == typeof(IList<>))
                    {
                        var args = seqType.GetGenericArguments();
                        Type itemType = args.Length > 0 ? args[0] : typeof(Object);
                        return itemType;
                    }
                    else if (interfaceType.IsGenericType &&
                        interfaceType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                    {
                        Type itemType = seqType.GetGenericArguments()[0];
                        return itemType;
                    }
                }
            }
            return null;
        }

        ////////////////////

        public static bool IsNullableType(this Type type)
        {
            return type != null && ((type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) || type.IsClass);
        }

        public static bool IsNullAssignable(Type type)
        {
            return !type.IsValueType || IsNullableType(type);
        }

        ////////////////////

        public static Type GetNonNullableType(Type type)
        {
            /*if (IsNullableType(type) && type != typeof(String))
            {
                return type.GetGenericArguments()[0];
            }
            return type;*/
            if (IsNullableType(type) && type != typeof(String))
            {
                var genericArguments = type.GetGenericArguments();
                if (genericArguments.Length > 0)
                    return genericArguments[0];
            }
            return type;
        }

        ////////////////////

        public static Type GetMemberType(MemberInfo mi)
        {
            FieldInfo fi = mi as FieldInfo;
            if (fi != null) return fi.FieldType;
            PropertyInfo pi = mi as PropertyInfo;
            if (pi != null) return pi.PropertyType;
            EventInfo ei = mi as EventInfo;
            if (ei != null) return ei.EventHandlerType;
            return null;
        }

        ////////////////////

        public static Int32 ToInt(this Int64 Val) { return Convert.ToInt32(Val, CultureInfo.InvariantCulture); }

        public static Int32 ToInt(this Double Val) { return Convert.ToInt32(Val, CultureInfo.InvariantCulture); }

        public static Int32 ToInt(this Decimal Val) { return Convert.ToInt32(Val, CultureInfo.InvariantCulture); }

        public static Int32 ToInt(this Single Val) { return Convert.ToInt32(Val, CultureInfo.InvariantCulture); }

        ////////////////////

        public static Double ToDouble(this Int32 Val) { return Convert.ToDouble(Val, CultureInfo.InvariantCulture); }

        public static Double ToDouble(this Int64 Val) { return Convert.ToDouble(Val, CultureInfo.InvariantCulture); }

        public static Double ToDouble(this Decimal Val) { return Convert.ToDouble(Val, CultureInfo.InvariantCulture); }

        public static Double ToDouble(this Single Val) { return Convert.ToDouble(Val, CultureInfo.InvariantCulture); }

        ////////////////////

        public static T ConvertTo<T>(this Object Object)
        {
            var lValue = ConvertTo(Object, typeof(T));
            if (lValue == null)
            {
                if (default(T) == null)
                {
                    return (T)(object)null;
                }
                else
                {
                    return default(T);
                }
            }
            else
            {
                return (T)lValue;
            }
        }

        // static object _lck = new object();
        //static Dictionary<Type, Func<Object, Object>> _convertCashe = null;
        public static Object ConvertTo(this Object Object, Type DestinationType)
        {
            if (Object == null || Object == DBNull.Value)
            {
                return null;
            }
            else
            {
                var lType = Object.GetType();
                if (lType == DestinationType || DestinationType == typeof(Object))
                {
                    return Object;
                }
                else
                {
                    var a = Object;

                    if (DestinationType == typeof(UInt16))
                        return CorrectNumericValue(Convert.ToUInt16(a, CultureInfo.InvariantCulture));
                    else if (DestinationType == typeof(UInt16?))
                        return CorrectNumericValue(Convert.ToUInt16(a, CultureInfo.InvariantCulture));

                    else if (DestinationType == typeof(Int16))
                        return CorrectNumericValue(Convert.ToInt16(a, CultureInfo.InvariantCulture));
                    else if (DestinationType == typeof(Int16?))
                        return CorrectNumericValue(Convert.ToInt16(a, CultureInfo.InvariantCulture));

                    else if (DestinationType == typeof(Int32))
                        return CorrectNumericValue(Convert.ToInt32(a, CultureInfo.InvariantCulture));
                    else if (DestinationType == typeof(Int32?))
                        return CorrectNumericValue(Convert.ToInt32(a, CultureInfo.InvariantCulture));

                    else if (DestinationType == typeof(UInt32))
                        return CorrectNumericValue(Convert.ToUInt32(a, CultureInfo.InvariantCulture));
                    else if (DestinationType == typeof(UInt32?))
                        return CorrectNumericValue(Convert.ToUInt32(a, CultureInfo.InvariantCulture));

                    else if (DestinationType == typeof(Int64))
                        return CorrectNumericValue(Convert.ToInt64(a, CultureInfo.InvariantCulture));
                    else if (DestinationType == typeof(Int64?))
                        return CorrectNumericValue(Convert.ToInt64(a, CultureInfo.InvariantCulture));

                    else if (DestinationType == typeof(UInt64))
                        return CorrectNumericValue(Convert.ToUInt64(a, CultureInfo.InvariantCulture));
                    else if (DestinationType == typeof(UInt64?))
                        return CorrectNumericValue(Convert.ToUInt64(a, CultureInfo.InvariantCulture));

                    else if (DestinationType == typeof(Single))
                        return CorrectNumericValue(Convert.ToSingle(a, CultureInfo.InvariantCulture));
                    else if (DestinationType == typeof(Single?))
                        return CorrectNumericValue(Convert.ToSingle(a, CultureInfo.InvariantCulture));

                    else if (DestinationType == typeof(Double))
                        return CorrectNumericValue(Convert.ToDouble(a, CultureInfo.InvariantCulture));
                    else if (DestinationType == typeof(Double?))
                        return CorrectNumericValue(Convert.ToDouble(a, CultureInfo.InvariantCulture));

                    else if (DestinationType == typeof(Decimal))
                        return CorrectNumericValue(Convert.ToDecimal(a, CultureInfo.InvariantCulture));
                    else if (DestinationType == typeof(Decimal?))
                        return CorrectNumericValue(Convert.ToDecimal(a, CultureInfo.InvariantCulture));

                    else if (DestinationType == typeof(DateTime))
                        return CorrectNumericValue(Convert.ToDateTime(a, CultureInfo.InvariantCulture));
                    else if (DestinationType == typeof(DateTime?))
                        return CorrectNumericValue(Convert.ToDateTime(a, CultureInfo.InvariantCulture));

                    else if (DestinationType == typeof(TimeSpan) && (lType == typeof(DateTime) || lType == typeof(DateTime?)))
                        return ((DateTime)a).TimeOfDay;
                    else if (DestinationType == typeof(TimeSpan?) && (lType == typeof(DateTime) || lType == typeof(DateTime?)))
                        return ((DateTime)a).TimeOfDay;

                    else if (DestinationType == typeof(Byte))
                        return CorrectNumericValue(Convert.ToByte(a, CultureInfo.InvariantCulture));
                    else if (DestinationType == typeof(Byte?))
                        return CorrectNumericValue(Convert.ToByte(a, CultureInfo.InvariantCulture));

                    else if (DestinationType == typeof(Boolean))
                        return CorrectNumericValue(Convert.ToBoolean(a, CultureInfo.InvariantCulture));
                    else if (DestinationType == typeof(Boolean?))
                        return CorrectNumericValue(Convert.ToBoolean(a, CultureInfo.InvariantCulture));

                    else if (DestinationType == typeof(Guid))
                        return (new Guid(Convert.ToString(a, CultureInfo.InvariantCulture)));
                    else if (DestinationType == typeof(Guid?))
                        return (new Guid(Convert.ToString(a, CultureInfo.InvariantCulture)));

                    else if (DestinationType == typeof(String))
                        return CorrectNumericValue(Convert.ToString(a, CultureInfo.InvariantCulture));

                    else if (DestinationType.IsEnum)
                        return Enum.ToObject(DestinationType, a);

                    else
                        return Convert.ChangeType(Object, DestinationType, CultureInfo.InvariantCulture);
                }
            }
        }

        private static Object CorrectNumericValue(Object Value)
        {
            if (Value == null || "".Equals(Value))
            {
                return 0M;
            }
            else
            {
                return Value;
            }
        }
    }
}
