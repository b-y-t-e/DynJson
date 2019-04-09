using System;
using System.Collections.Generic;
using System.Text;

namespace DynJson.Exceptions
{

    [Serializable]
    public class MethodNotFoundException : Exception
    {
        public string MethodName { get; private set; }
        // public MethodNotFoundException() { }
        public MethodNotFoundException(string methodName, string message) : base(message) { this.MethodName = methodName; }
        public MethodNotFoundException(string methodName, string message, Exception inner) : base(message, inner) { this.MethodName = methodName; }
        protected MethodNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
