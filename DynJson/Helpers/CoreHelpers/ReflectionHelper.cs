using System.Linq;
using System;
using System.Reflection;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using DynJson.Helpers;

namespace DynJson.Helpers.CoreHelpers
{
    public class ValueHelper
    {
        public Object Value;

        public Type Type;

        public String PropertyName;

        public PropertyInfo Property;
    }

    public static class ReflectionHelper
    {
        public static Dictionary<string, object> ToDictionary(Object Value)
        {
            if (Value == null || MyTypeHelper.IsPrimitive(Value.GetType()))
                return null;

            Dictionary<string, object> dict = new Dictionary<string, object>();

            foreach (var field in GetFields(Value))
                dict[field.Name] = field.GetValue(Value);

            foreach (var property in GetProperties(Value))
                dict[property.Name] = property.GetValue(Value);

            return dict;
        }

        public static void CopyTo(Object Source, Object Dest)
        {
            if (Source == null || Dest == null)
                return;

            foreach (PropertyInfo property in GetProperties(Source))
                SetValue(
                    Dest,
                    property.Name,
                    GetValue(Source, property.Name));
        }

        public static PropertyInfo[] GetProperties(Object Item)
        {
            if (Item != null)
            {
                return Item.GetType().GetProperties().ToArray();
            }
            return new PropertyInfo[0];
        }

        public static FieldInfo[] GetFields(Object Item)
        {
            if (Item != null)
            {
                return Item.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance).ToArray();
            }
            return new FieldInfo[0];
        }

        public static MethodInfo GetMethod(Type Type, String MethodName)
        {
            if (Type != null)
            {
                return Type.GetMethod(MethodName);
            }
            return null;
        }

        public static PropertyInfo GetProperty(Type Type, String PropertyName)
        {
            if (Type != null)
            {
                return Type.GetProperty(PropertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            }
            return null;
        }

        public static PropertyInfo GetProperty(Object Item, String PropertyName)
        {
            if (Item != null)
            {
                return GetProperty((Type)Item.GetType(), PropertyName);
            }
            return null;
        }

        public static MethodInfo GetMethod(Object Item, String MethodName)
        {
            if (Item != null)
            {
                return GetMethod((Type)Item.GetType(), MethodName);
            }
            return null;
        }

        public static Boolean ContainsProperty(this Object Item, String PropertyName)
        {
            if (Item != null && !String.IsNullOrEmpty(PropertyName))
            {
                var lProperty = GetProperty(Item, PropertyName);
                return lProperty != null;
            }
            return false;
        }

        public static DataType GetValue<DataType>(this Object Item, String PropertyName)
        {
            if (Item != null && !String.IsNullOrEmpty(PropertyName))
            {
                var lProperty = GetProperty(Item, PropertyName);
                if (lProperty != null)
                {
                    return (DataType)lProperty.GetValue(Item, null);
                }
            }
            return default(DataType);
        }

        public static bool SetValue<DataType>(this Object Item, String PropertyName, DataType Value)
        {
            var lProperty = GetProperty(Item, PropertyName);
            if (lProperty != null)
            {
                var lType1 = lProperty.PropertyType;
                var lType2 = typeof(DataType);
                if (lType1.Equals(lType2) || MyTypeHelper.Is(lType2, lType1))
                {
                    lProperty.SetValue(Item, Value, null);
                }
                else
                {
                    var lNewValue = MyTypeHelper.ConvertTo(Value, lType1);
                    lProperty.SetValue(Item, lNewValue, null);
                }
                return true;
            }
            return false;
        }

        public static Object GetValue(this Object Item, String PropertyName)
        {
            return GetValue<Object>(Item, PropertyName);
        }

        public static bool SetValue(this Object Item, String PropertyName, Object Value)
        {
            return SetValue<Object>(Item, PropertyName, Value);
        }

        public static Object Invoke(this Object Item, String MethodName, params Object[] Params)
        {
            if (Item != null)
            {
                Params = Params ?? new Object[0];

                MethodInfo method = FindMethodToInvoke(
                    Item,
                    true,
                    MethodName,
                    Params.Length);

                if (method == null)
                {
                    method = FindMethodToInvoke(
                        Item,
                        false,
                        MethodName,
                        Params.Length);
                }

                if (method != null)
                {
                    return method.Invoke(Item, Params);
                }
                /*else
                {
                    throw new Exception("Not found " + MethodName + " with " + Params.Length + " count");
                }*/
            }
            return null;
        }

        public static MethodInfo FindMethodToInvoke(Object Item, Boolean ExactName, String MethodName, Int32 ParamCount)
        {
            Type itemType = Item.GetType();

            MethodInfo[] publicMethods = itemType.
                GetMethods(BindingFlags.Public | BindingFlags.Instance);

            MethodInfo method = publicMethods.
                Where(m =>
                    ((ExactName && m.Name == MethodName) || (!ExactName && GetNotExactMethodName(m.Name) == MethodName)) &&
                    (m.GetParameters() == null || m.GetParameters()./*Where(p => !p.IsRetval).*/Count() == ParamCount)).
                FirstOrDefault();

            if (method == null)
            {
                MethodInfo[] privateMethods = itemType.
                    GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);

                method = privateMethods.
                    Where(m =>
                        ((ExactName && m.Name == MethodName) || (!ExactName && GetNotExactMethodName(m.Name) == MethodName)) &&
                        (m.GetParameters() == null || m.GetParameters()./*Where(p => !p.IsRetval).*/Count() == ParamCount)).
                    FirstOrDefault();
            }

            return method;
        }

        private static String GetNotExactMethodName(String Name)
        {
            var parts = Name.Split(new[] { '.' });
            return parts[parts.Length - 1];
        }

        public static object GetValueFromPath(Object Item, String Path)
        {
            if (Item != null)
            {
                Path = Path ?? "";
                if (String.IsNullOrEmpty(Path))
                {
                    return Item;
                }
                else
                {
                    String[] pathItems = Path.Split(new char[] { '.' }, StringSplitOptions.None);
                    if (pathItems != null && pathItems.Length > 0)
                    {
                        Object currentValue = Item;

                        Int32 pathIndex = -1;
                        foreach (String pathItem in pathItems)
                        {
                            pathIndex++;
                            String name = GetNameForItem(pathItem);
                            Object index = GetIndexForItem(pathItem);

                            String stringValue = GetStringValue(name);
                            Decimal? decimalValue = GetDecimalValue(name);

                            if (stringValue != null)
                            {
                                currentValue = stringValue;
                            }
                            else if (decimalValue != null)
                            {
                                currentValue = decimalValue.Value;
                            }
                            else if (String.IsNullOrEmpty(name))
                            {
                                if (index == null)
                                {
                                    currentValue = null;
                                    break;
                                }
                            }
                            else
                            {
                                currentValue = GetValue(currentValue, name);
                                if (currentValue == null)
                                {
                                    currentValue = null;
                                    break;
                                }
                            }

                            if (index != null)
                            {
                                try
                                {
                                    IDictionary dict = currentValue as IDictionary;
                                    IList list = currentValue as IList;
                                    String text = currentValue as String;

                                    if (index is string)
                                    {
                                        currentValue = dict[(String)index];
                                    }
                                    else
                                    {
                                        if (dict != null)
                                            currentValue = dict[(Int32)index];
                                        else if (list != null)
                                            currentValue = list[(Int32)index];
                                        else
                                            currentValue = text[(Int32)index];
                                    }
                                }
                                catch
                                {
                                    currentValue = null;
                                    break;
                                }
                            }
                        }

                        return currentValue;
                    }
                }
            }
            return null;
        }

        private static Object GetIndexForItem(String PathItem)
        {
            Int32 startIndex = PathItem.IndexOf('[');
            Int32 endIndex = PathItem.IndexOf(']');
            if (startIndex >= 0 && endIndex > startIndex)
            {
                String content = PathItem.Substring(startIndex + 1, endIndex - startIndex - 1);
                String stringVal = GetStringValue(content.Trim());
                if (stringVal != null)
                {
                    return stringVal;
                }

                Int32 v = 0;
                if (Int32.TryParse(content, out v))
                    return Convert.ToInt32(content);

                return content;
            }
            return null;
        }

        private static String GetNameForItem(String PathItem)
        {
            Int32 startIndex = PathItem.IndexOf('[');
            Int32 endIndex = PathItem.IndexOf(']');
            if (startIndex >= 0 && endIndex > startIndex)
            {
                return PathItem.Substring(0, startIndex);
            }
            return PathItem;
        }

        private static String GetStringValue(String trimmedContent)
        {
            trimmedContent = trimmedContent.Trim();
            if ((trimmedContent.StartsWith("|") || trimmedContent.StartsWith("`") || trimmedContent.StartsWith("'") || trimmedContent.StartsWith("\"")) &&
                trimmedContent.Length > 1 &&
                (trimmedContent.EndsWith("|") || trimmedContent.EndsWith("`") || trimmedContent.EndsWith("'") || trimmedContent.EndsWith("\"")))
            {
                return trimmedContent.Substring(1, trimmedContent.Length - 2);
            }
            return null;
        }

        public static Decimal? GetDecimalValue(String trimmedContent)
        {
            trimmedContent = trimmedContent.Trim();
            try { return UniConvert.ParseUniDecimal(trimmedContent); }
            catch { }
            return null;
        }

    }
}
