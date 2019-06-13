using System;
using System.Collections.Generic;
using System.Text;

namespace DynJson.Helpers
{
    public static class Logger
    {
        public static List<DynJsonLogEventDelegate> LogActions;

        public static Boolean IsEnabled
        {
            get
            {
                return LogActions != null && LogActions.Count > 0;
            }
        }
        public static void LogInfo(String SubSystem, String Title, Object Value, String Message)
        {
            Log(new DynJsonLogEvent() { Type = EDynJsonLogType.INFO, SubSystem = SubSystem, Title = Title, Value = Value, Message = Message });
        }
        public static void LogError(String SubSystem, String Title, Object Value, String Message)
        {
            Log(new DynJsonLogEvent() { Type = EDynJsonLogType.ERROR, SubSystem = SubSystem, Title = Title, Value = Value, Message = Message });
        }
        public static void LogPerformance(String SubSystem, String Title, Object Value, String Message)
        {
            Log(new DynJsonLogEvent() { Type = EDynJsonLogType.PERFORMANCE, SubSystem = SubSystem, Title = Title, Value = Value, Message = Message });
        }
        public static void LogWarning(String SubSystem, String Title, Object Value, String Message)
        {
            Log(new DynJsonLogEvent() { Type = EDynJsonLogType.WARNING, SubSystem = SubSystem, Title = Title, Value = Value, Message = Message });
        }
        public static void LogDebug(String SubSystem, String Title, Object Value, String Message)
        {
            Log(new DynJsonLogEvent() { Type = EDynJsonLogType.DEBUG, SubSystem = SubSystem, Title = Title, Value = Value, Message = Message });
        }
        private static void Log(DynJsonLogEvent Event)
        {
            if (!IsEnabled)
                return;

            foreach (var act in LogActions)
                try { act(Event); }
                catch { }
        }
    }

    public delegate void DynJsonLogEventDelegate(DynJsonLogEvent Event);

    public enum EDynJsonLogType
    {
        ERROR = 0,
        INFO = 1,
        PERFORMANCE = 2,

        WARNING = 3,
        DEBUG = 4
    }

    public class DynJsonLogEvent
    {
        public EDynJsonLogType Type { get; set; }
        public String SubSystem { get; set; }
        public String Title { get; set; }
        public Object Value { get; set; }
        public String Message { get; set; }
    }
}
