using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DynJson.Helpers.CoreHelpers
{
    public static class MyReflectionHelper
    {
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
}
