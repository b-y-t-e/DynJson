using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DynJson.Helpers.CoreHelpers
{
    public static class MyReflectionHelper
    {
        /*public static Object GetValueFromPath(Object Object, String Paths)
        {
            return GetValueFromPath(
                Object,
                ParsePath(Paths));
        }

        public static Object GetValueFromPath(Object Object, VarPaths Paths)
        {
            
        }

        public static VarPaths ParsePath(String Path)
        {
            VarPaths paths = new VarPaths();

            string[] items = (Path ?? "").Split('.');
            for (var i = 0; i < items.Length; i++)
            {
                VarPath path = new VarPath();
                path.Name = items[i];

                paths.Add(path);
            }

            return paths;
        }*/


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
                                if (currentValue is IDictionary dict)
                                {
                                    var newValue = dict[name];
                                    currentValue = newValue;
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

        public static DataType GetValue<DataType>(this Object Item, String PropertyName)
        {
            if (Item != null && !String.IsNullOrEmpty(PropertyName))
            {
                var lProperty = GetProperty(Item, PropertyName);
                if (lProperty != null)
                {
                    return (DataType)lProperty.GetValue(Item, null);
                }
                var lField = GetField(Item, PropertyName);
                if (lField != null)
                {
                    return (DataType)lField.GetValue(Item);
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

        public static FieldInfo GetField(Type Type, String PropertyName)
        {
            if (Type != null)
            {
                return Type.GetField(PropertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            }
            return null;
        }

        public static FieldInfo GetField(Object Item, String PropertyName)
        {
            if (Item != null)
            {
                return GetField((Type)Item.GetType(), PropertyName);
            }
            return null;
        }

        public static Object GetValue(this Object Item, String PropertyName)
        {
            return GetValue<Object>(Item, PropertyName);
        }

        public static bool SetValue(this Object Item, String PropertyName, Object Value)
        {
            return SetValue<Object>(Item, PropertyName, Value);
        }

        public static MethodInfo GetMethod(this Object Item, String MethodName, Int32 ParameterCount = -1)
        {
            if (Item != null)
            {
                return GetMethod((Type)Item.GetType(), MethodName, ParameterCount);
            }
            return null;
        }

        public static MethodInfo GetMethod(this Type Type, String MethodName, Int32 ParameterCount = -1)
        {
            if (Type != null)
            {
                return Type.
                    GetMethods().
                    FirstOrDefault(m =>
                        m.Name == MethodName &&
                        (
                            ParameterCount < 0 ||
                            m.GetParameters().Count() == ParameterCount));
            }
            return null;
        }
    }

    public class VarPath
    {
        public String Name { get; set; }
    }

    public class VarPaths : List<VarPath>
    {

    }
}
