using System;
using System.Collections.Generic;
using System.Text;

namespace DynJson.Exceptions
{

    [Serializable]
    public class MethodNotFoundException : Exception
    {
        public MethodNotFoundException() { }
        public MethodNotFoundException(string message) : base(message) { }
        public MethodNotFoundException(string message, Exception inner) : base(message, inner) { }
        protected MethodNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
