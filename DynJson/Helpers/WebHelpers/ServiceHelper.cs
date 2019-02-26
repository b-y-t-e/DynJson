using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
//using System.ServiceModel.Web;
using System.Text;

namespace DynJson.Helpers.WebHelpers
{

    /*public enum StreamContentType
    {
        APPLICATION_XML,
        TEXT_HTML,
        TEXT_PLAIN,
        SILVERLIGHT_APP
    }

    public class ServiceHelper
    {
        public static void SetStreamContentType(StreamContentType StreamContentType)
        {
            if (StreamContentType == StreamContentType.APPLICATION_XML)
            {
                WebOperationContext.Current.OutgoingResponse.ContentType = "application/xml";
            }
            else if (StreamContentType == StreamContentType.SILVERLIGHT_APP)
            {
                WebOperationContext.Current.OutgoingResponse.ContentType = "application/x-silverlight-app";
            }
            else if (StreamContentType == StreamContentType.TEXT_HTML)
            {
                WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            }
            else
            {
                WebOperationContext.Current.OutgoingResponse.ContentType = "text/plain";
            }
        }

        public static Stream StringToStream(string result, StreamContentType StreamContentType)
        {
            return StringToStream(Encoding.UTF8.GetBytes(result), StreamContentType);
        }

        public static Stream StringToStream(Byte[] Bytes, StreamContentType StreamContentType)
        {
            SetStreamContentType(StreamContentType);
            return new MemoryStream(Bytes);
        }

        public static String GetLocalPath(String FilePath)
        {
            var lTmp = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            var lResult = Path.Combine(
                Path.GetDirectoryName(lTmp),
                FilePath);
            return lResult;
        }

        public static String GetWebPath(String FilePath)
        {
            var lTmp = System.Web.HttpContext.Current.Server.MapPath("~/");
            var lResult = Path.Combine(
                lTmp,
                FilePath);
            return lResult;
        }

        public static String GetPath(String FilePath)
        {
            try { return GetWebPath(FilePath); }
            catch { return GetLocalPath(FilePath); }
        }

        public static Stream FileToStream(String FilePath, StreamContentType StreamContentType)
        {
            var lPath = GetPath(FilePath);
            if (File.Exists(lPath))
            {
                SetStreamContentType(StreamContentType);
                return new FileStream(lPath, FileMode.Open);
            }
            return StringToStream("", StreamContentType.TEXT_PLAIN);
        }
        
    }
*/
}
