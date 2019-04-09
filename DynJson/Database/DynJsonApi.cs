using DynJson.Helpers.CoreHelpers;
using DynJson.Helpers.DatabaseHelpers;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using DynJson.Executor;
using DynJson.Tokens;
using System.Threading.Tasks;

namespace DynJson.Database
{
    public class DynJsonApi
    {
        public S4JExecutor Executor { get; set; }

        //////////////////////////////////////////////

        public DynJsonApi(S4JExecutor Executor)
        {
            this.Executor = Executor;
        }

        //////////////////////////////////////////////

        public object exec(string name)
        {
            S4JToken result = null;
            S4JToken method = Executor.Methods.Find(name).GetAwaiter().GetResult();
            if (method != null)
                result = Executor.ExecuteWithParameters(method, new object[] { }).GetAwaiter().GetResult();
            return JsonToDynamicDeserializer.Deserialize(result.ToJson());
        }


        public object exec(string name, object p1)
        {
            S4JToken result = null;
            S4JToken method = Executor.Methods.Find(name).GetAwaiter().GetResult(); 
            if (method != null)
                result = Executor.ExecuteWithParameters(method,
                    new[] { p1 }).GetAwaiter().GetResult(); 
            return JsonToDynamicDeserializer.Deserialize(result.ToJson());
        }

        public object exec(string name, object p1, object p2)
        {
            S4JToken result = null;
            S4JToken method = Executor.Methods.Find(name).GetAwaiter().GetResult(); 
            if (method != null)
                result = Executor.ExecuteWithParameters(method,
                    new[] { p1, p2 }).GetAwaiter().GetResult(); 
            return JsonToDynamicDeserializer.Deserialize(result.ToJson());
        }

        public object exec(string name, object p1, object p2, object p3)
        {
            S4JToken result = null;
            S4JToken method = Executor.Methods.Find(name).GetAwaiter().GetResult();
            if (method != null)
                result = Executor.ExecuteWithParameters(method,
                    new[] { p1, p2, p3 }).GetAwaiter().GetResult();
            return JsonToDynamicDeserializer.Deserialize(result.ToJson());
        }

        public object exec(string name, object p1, object p2, object p3, object p4)
        {
            S4JToken result = null;
            S4JToken method = Executor.Methods.Find(name).GetAwaiter().GetResult();
            if (method != null)
                result = Executor.ExecuteWithParameters(method,
                    new[] { p1, p2, p3, p4 }).GetAwaiter().GetResult();
            return JsonToDynamicDeserializer.Deserialize(result.ToJson());
        }

        public object exec(string name, object p1, object p2, object p3, object p4, object p5)
        {
            S4JToken result = null;
            S4JToken method = Executor.Methods.Find(name).GetAwaiter().GetResult();
            if (method != null)
                result = Executor.ExecuteWithParameters(method,
                    new[] { p1, p2, p3, p4, p5 }).GetAwaiter().GetResult();
            return JsonToDynamicDeserializer.Deserialize(result.ToJson());
        }

        public object exec(string name, object p1, object p2, object p3, object p4, object p5, object p6)
        {
            S4JToken result = null;
            S4JToken method = Executor.Methods.Find(name).GetAwaiter().GetResult();
            if (method != null)
                result = Executor.ExecuteWithParameters(method,
                    new[] { p1, p2, p3, p4, p5, p6 }).GetAwaiter().GetResult();
            return JsonToDynamicDeserializer.Deserialize(result.ToJson());
        }

        public object exec(string name, object p1, object p2, object p3, object p4, object p5, object p6, object p7)
        {
            S4JToken result = null;
            S4JToken method = Executor.Methods.Find(name).GetAwaiter().GetResult();
            if (method != null)
                result = Executor.ExecuteWithParameters(method,
                    new[] { p1, p2, p3, p4, p5, p6, p7 }).GetAwaiter().GetResult();
            return JsonToDynamicDeserializer.Deserialize(result.ToJson());
        }

        public object exec(string name, object p1, object p2, object p3, object p4, object p5, object p6, object p7, object p8)
        {
            S4JToken result = null;
            S4JToken method = Executor.Methods.Find(name).GetAwaiter().GetResult();
            if (method != null)
                result = Executor.ExecuteWithParameters(method,
                    new[] { p1, p2, p3, p4, p5, p6, p7, p8 }).GetAwaiter().GetResult();
            return JsonToDynamicDeserializer.Deserialize(result.ToJson());
        }

        public object exec(string name, object p1, object p2, object p3, object p4, object p5, object p6, object p7, object p8, object p9)
        {
            S4JToken result = null;
            S4JToken method = Executor.Methods.Find(name).GetAwaiter().GetResult();
            if (method != null)
                result = Executor.ExecuteWithParameters(method,
                    new[] { p1, p2, p3, p4, p5, p6, p7, p8, p9 }).GetAwaiter().GetResult();
            return JsonToDynamicDeserializer.Deserialize(result.ToJson());
        }

        public object exec(string name, object p1, object p2, object p3, object p4, object p5, object p6, object p7, object p8, object p9, object p10)
        {
            S4JToken result = null;
            S4JToken method = Executor.Methods.Find(name).GetAwaiter().GetResult();
            if (method != null)
                result = Executor.ExecuteWithParameters(method,
                    new[] { p1, p2, p3, p4, p5, p6, p7, p8, p9, p10 }).GetAwaiter().GetResult();
            return JsonToDynamicDeserializer.Deserialize(result.ToJson());
        }
    }
}
