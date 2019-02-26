using DynJson.Helpers.CoreHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace DynJson.Helpers.WebHelpers
{
    public static class JsFilesHelper
    {
        private static Object lck = new Object();

        private static TimeSpan maxDiff = TimeSpan.FromSeconds(300 * 4);

        ////////////////////////////////////////////////////

        public static JsFile Save(
            String FileName,
            Byte[] Content)
        {
            lock (lck)
            {
                Initialize();
                RemoveOldFiles(maxDiff);

                JsFile jsFile = null;
                if (!String.IsNullOrEmpty(FileName) && Content != null && Content.Length > 0)
                {
                    String directory = JsUstawienia.JS_FILES_DIRECTORY;

                    jsFile = new JsFile();
                    jsFile.Created = DateTime.Now;
                    jsFile.FileName = FileName;
                    jsFile.ID = Guid.NewGuid().ToString();
                    jsFile.Length = Content.Length;

                    // generate names
                    String definitionPath = GetDefinitionPath(jsFile.ID);
                    String contentPath = GetContentPath(jsFile.ID);

                    // save
                    JsonSerializer.SerializeJsonToFile(
                        definitionPath,
                        jsFile);

                    if (File.Exists(contentPath)) File.Delete(contentPath);
                    File.WriteAllBytes(contentPath, Content);
                }
                return jsFile;
            }
        }

        /*public static JsFile Load(String ID)
        {
            lock (lck)
            {
                Initialize();
                RemoveOldFiles(maxDiff);

                String definitionPath = GetDefinitionPath(ID);
                return JsonSerializer.DeserializeJsonFromFile<JsFile>(definitionPath);
            }
        }*/

        public static JsFileWithContent LoadWithContent(String ID)
        {
            lock (lck)
            {
                Initialize();
                RemoveOldFiles(maxDiff);

                String definitionPath = GetDefinitionPath(ID);
                String contentPath = GetContentPath(ID);

                return new JsFileWithContent()
                {
                    File = JsonSerializer.DeserializeJsonFromFile<JsFile>(definitionPath),
                    Content = File.ReadAllBytes(contentPath)
                };
            }
        }

        ////////////////////////////////////////////////////

        private static void Initialize()
        {
            lock (lck)
            {
                String directory = JsUstawienia.JS_FILES_DIRECTORY;
                if (!string.IsNullOrEmpty(directory))
                {
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);
                }
                else
                {
                    throw new Exception("Nieprawidłowa wartość ustawienia JS_FILES_DIRECTORY!");
                }
            }
        }

        private static void RemoveOldFiles(TimeSpan MaxDiff)
        {
            lock (lck)
            {
                String directory = JsUstawienia.JS_FILES_DIRECTORY;
                foreach (var file in Directory.GetFiles(directory, "*.def"))
                {
                    var jsFile = JsonSerializer.DeserializeJsonFromFile<JsFile>(file);
                    var diff = new TimeSpan(DateTime.Now.Ticks - jsFile.Created.Ticks);
                    if (diff > MaxDiff)
                    {
                        String definitionPath = GetDefinitionPath(jsFile.ID);
                        String contentPath = GetContentPath(jsFile.ID);

                        if (File.Exists(definitionPath))
                            File.Delete(definitionPath);

                        if (File.Exists(contentPath))
                            File.Delete(contentPath);
                    }
                }
            }
        }

        ////////////////////////////////////////////////////

        private static String GetDefinitionPath(String ID)
        {
            return Path.Combine(
                JsUstawienia.JS_FILES_DIRECTORY,
                ID + ".def");
        }

        private static String GetContentPath(String ID)
        {
            return Path.Combine(
                JsUstawienia.JS_FILES_DIRECTORY,
                ID + ".bin");
        }
    }

    [DataContract]
    public class JsFile
    {
        [DataMember]
        public String ID { get; set; }

        [DataMember]
        public String FileName { get; set; }

        [DataMember]
        public Int32 Length { get; set; }

        [DataMember]
        public DateTime Created { get; set; }
    }

    public class JsFileWithContent
    {
        public JsFile File;
        public Byte[] Content;
    }

    public static class JsUstawienia
    {
        public static String APP_DIR
        {
            get
            {
                var lUri = new Uri(Assembly.GetExecutingAssembly().CodeBase);
                return Path.GetDirectoryName(lUri.LocalPath);
            }
        }

        public static String JS_FILES_DIRECTORY
        {
            get
            {
                throw new NotImplementedException();
                /*String path = System.Configuration.ConfigurationManager.AppSettings["JS_FILES_DIRECTORY"];
                if (String.IsNullOrEmpty(path))
                    path = Path.Combine(APP_DIR, "JS_FILES_DIRECTORY");
                return path;*/
            }
        }
    }
}
