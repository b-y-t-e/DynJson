using System;
using System.Collections.Generic;
using System.Text;

namespace DynJson.Helpers
{
    public static class Logger
    {
        public static List<DynJsonLogEvent> LogActions;

        public static Boolean IsEnabled
        {
            get
            {
                return LogActions != null && LogActions.Count > 0;
            }
        }
        public static void LogInfo(String SubSystem, String Title, Object Value, String Message)
        {
            Log(EDynJsonLogType.INFO, SubSystem, Title, Value, Message);
        }
        public static void LogError(String SubSystem, String Title, Object Value, String Message)
        {
            Log(EDynJsonLogType.ERROR, SubSystem, Title, Value, Message);
        }
        public static void LogPerformance(String SubSystem, String Title, Object Value, String Message)
        {
            Log(EDynJsonLogType.PERFORMANCE, SubSystem, Title, Value, Message);
        }
        public static void LogWarning(String SubSystem, String Title, Object Value, String Message)
        {
            Log(EDynJsonLogType.WARNING, SubSystem, Title, Value, Message);
        }
        public static void LogDebug(String SubSystem, String Title, Object Value, String Message)
        {
            Log(EDynJsonLogType.DEBUG, SubSystem, Title, Value, Message);
        }
        private static void Log(EDynJsonLogType Type, String SubSystem, String Title, Object Value, String Message)
        {
            if (!IsEnabled)
                return;

            foreach (var act in LogActions)
                try { act(Type, SubSystem, Title, Value, Message); }
                catch { }
        }
    }

    public delegate void DynJsonLogEvent(EDynJsonLogType Type, String SubSystem, String Title, Object Value, String Message);

    public enum EDynJsonLogType
    {
        ERROR = 0,
        INFO = 1,
        PERFORMANCE = 2,

        WARNING = 3,
        DEBUG = 4
    }
}
