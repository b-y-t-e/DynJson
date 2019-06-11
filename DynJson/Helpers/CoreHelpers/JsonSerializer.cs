using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Converters;
using System.IO;

namespace DynJson.Helpers.CoreHelpers
{
    public static class JsonSerializer
    {
        private static JsonSerializerSettings getSettings()
        {
            return new JsonSerializerSettings()
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateParseHandling = DateParseHandling.DateTime
            };
        }

        //////////////////////////////////

        public static String SerializeJsonNoBrackets(this IDictionary<String, Object> Dictionary)
        {
            if (Dictionary == null)
            {
                return "null";
            }
            else
            {
                String result = JsonConvert.SerializeObject(Dictionary , getSettings());
                return result.Substring(1, result.Length - 2);
            }
        }

        public static String SerializeJsonNoBrackets(this IList<Object> List)
        {
            if (List == null)
            {
                return "null";
            }
            else
            {
                String result = JsonConvert.SerializeObject(List , getSettings());
                return result.Substring(1, result.Length - 2);
            }
        }

        public static String SerializeJsonNoBrackets(this Object Value)
        {
            if (Value == null)
            {
                return "null";
            }
            else
            {
                String result = JsonConvert.SerializeObject(Value , getSettings());
                if (result.StartsWith("[") || result.StartsWith("{"))
                    return result.Substring(1, result.Length - 2);
                return result;
            }
        }

        public static String SerializeJson(this Object Item)
        {
            if (Item == null)
            {
                return "null";
            }
            else
            {
                return JsonConvert.SerializeObject(Item, getSettings());
            }
        }

        public static T DeserializeJson<T>(this String String)
        {
            if (String.IsNullOrEmpty(String))
            {
                return default(T);
            }
            else
            {
                return (T)JsonConvert.DeserializeObject<T>(String, getSettings()); 
            }
        }

        public static Object DeserializeJson(this String String)
        {
            if (String.IsNullOrEmpty(String))
            {
                return null;
            }
            else
            {
                return JsonConvert.DeserializeObject(String, getSettings()); 
            }
        }

        //////////////////////////////////

        public static String SerializeJsonToFile(String FilePath, Object Item)
        {
            var json = SerializeJson(Item);
            if (File.Exists(FilePath)) File.Delete(FilePath);
            File.WriteAllText(FilePath, json ?? "", Encoding.UTF8);
            return json;
        }

        public static T DeserializeJsonFromFile<T>(String FilePath)
        {
            if (File.Exists(FilePath))
            {
                return DeserializeJson<T>(
                    File.ReadAllText(FilePath, Encoding.UTF8));
            }
            else
            {
                return default(T);
            }
        }
    }
}