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
        /*private static JsonSerializerSettings getSettings()
        {
            JsonSerializerSettings customJsonSettings = new JsonSerializerSettings()
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Local
            };
            return customJsonSettings;
        }*/

        //////////////////////////////////

        public static String SerializeJsonNoBrackets(this IDictionary<String, Object> Dictionary)
        {
            if (Dictionary == null)
            {
                return "null";
            }
            else
            {
                String result = JsonConvert.SerializeObject(Dictionary); // , getSettings());
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
                String result = JsonConvert.SerializeObject(List); // , getSettings());
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
                String result = JsonConvert.SerializeObject(Value); // , getSettings());
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
                return JsonConvert.SerializeObject(Item); // , getSettings());
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
                return (T)JsonConvert.DeserializeObject<T>(String, new JsonSerializerSettings() { DateFormatHandling = DateFormatHandling.IsoDateFormat, DateParseHandling = DateParseHandling.DateTime }); //, getSettings()); 
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
                return JsonConvert.DeserializeObject(String, new JsonSerializerSettings() { DateFormatHandling = DateFormatHandling.IsoDateFormat, DateParseHandling = DateParseHandling.DateTime }); //, getSettings()); 
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






   /* public static class CompressionHelper
    {
        public static Byte[] Compress(this byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return new Byte[0];

            using (System.IO.MemoryStream wynik = new System.IO.MemoryStream())
            {
                using (ICSharpCode.SharpZipLib.GZip.GZipOutputStream zip = new ICSharpCode.SharpZipLib.GZip.GZipOutputStream(wynik))
                {
                    zip.Write(bytes, 0, bytes.Length);
                    zip.Finish();
                    return wynik.ToArray();
                }
            }
        }

        public static byte[] Dekompresja(this byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0)
                return null;

            if (buffer != null)
            {
                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    return Dekompresja(memoryStream);
                }
            }
            else
            {
                return new byte[0];
            }
        }

        public static byte[] Dekompresja(this System.IO.MemoryStream input)
        {
            if (input == null || input.Length == 0)
                return null;

            var lBytes = new List<byte>();
            using (var zip = new ICSharpCode.SharpZipLib.GZip.GZipInputStream(input))
            {
                byte[] buffer = new byte[4096];
                while (true)
                {
                    int read = zip.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < read; i++)
                        lBytes.Add(buffer[i]);
                    if (read <= 0) break;
                }
            }

            return lBytes.ToArray();
        }

    }*/
}