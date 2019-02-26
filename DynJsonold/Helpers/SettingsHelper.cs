using System;
using System.Collections.Generic;
using System.Text;

namespace DynJson.Helpers
{
    public static class SettingsHelper
    {
        public static string Get(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process).Replace("'", "");
        }
    }
}
