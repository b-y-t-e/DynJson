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
        public static Object GetItemValue(Object Item, String PropertyName)
        {
            Object propertyValue = null;
            if (Item is IDictionary<string, object> dictKeyValue)
            {
                if (dictKeyValue.ContainsKey(PropertyName))
                    propertyValue = dictKeyValue[PropertyName];
            }
            else if (Item is IDictionary dict)
            {
                if (dict.Contains(PropertyName))
                    propertyValue = dict[PropertyName];
            }
            else
            {
                propertyValue = RefUnsensitiveHelper.I.
                    GetValue(Item, PropertyName);
            }
            return propertyValue;
        }

        public static bool SetItemValue(Object Item, String PropertyName, Object Value)
        {
            if (Item is IDictionary<string, object> dictKeyValue)
            {
                dictKeyValue[PropertyName] = Value;
            }
            else if (Item is IDictionary dict)
            {
                dict[PropertyName] = Value;
            }
            else
            {
                return RefUnsensitiveHelper.I.
                    SetValue(Item, PropertyName, Value);
            }
            return false;
        }
        
        public static Dictionary<string, object> ToDictionary(Object Value)
        {
            if (Value == null || MyTypeHelper.IsPrimitive(Value.GetType()))
                return null;

            Dictionary<string, object> resultDict = new Dictionary<string, object>();

            if (Value is IDictionary<string, object> dictKeyValue)
            {
                foreach (var item in dictKeyValue)
                    resultDict[item.Key] = item.Value;
            }
            else if (Value is IDictionary dict)
            {
                foreach (var key in dict.Keys)
                    resultDict[UniConvert.ToString(key)] = dict[key];
            }
            else
            {
                foreach (var field in GetFields(Value))
                    resultDict[field.Name] = field.GetValue(Value);

                foreach (var property in RefSensitiveHelper.I.GetPropertyinfos(Value))
                    resultDict[property.Name] = property.GetValue(Value);
            }

            return resultDict;
        }
        
        public static FieldInfo[] GetFields(Object Item)
        {
            if (Item != null)
            {
                return Item.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance).ToArray();
            }
            return new FieldInfo[0];
        }
        
    }
}
