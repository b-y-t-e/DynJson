using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace DynJson.Helpers.WebHelpers
{
    public static class JsonSrvHelper
    {
        public static JsonEmptyResult Execute(Func<JsonEmptyResult> Func)
        {
            JsonEmptyResult lResult = new JsonEmptyResult();
            try
            {
                if (Func != null)
                {
                    var lTmpResult = Func();
                    lResult.CopyFrom(lTmpResult);
                }
            }
            catch (Exception ex)
            {
                lResult.Error = ex.Message; // ErrorHelper.ToDisplayString(ex);
                                            //lResult.IsLoginError = ex is SrvLoginException;
            }
            return lResult;
        }

        public static JsonResult<T> Execute<T>(Func<JsonResult<T>> Func)
        {
            return Execute<T>("string", Func);
        }

        public static JsonResult<T> Execute<T>(String ResultType, Func<JsonResult<T>> Func)
        {
            JsonResult<T> lResult = new JsonResult<T>()
            {
                //ResultType = ResultType,
                Result = DefaultValueHelper.GetDefault<T>()
            };
            try
            {
                if (Func != null)
                {
                    var lTmpResult = Func();
                    lResult.CopyFrom(lTmpResult);
                }
            }
            catch (Exception ex)
            {
                lResult.Error = ex.Message;
                //lResult.IsLoginError = ex is SrvLoginException;
            }
            return lResult;
        }

        public static JsonPartResult<T> Execute<T>(Func<JsonPartResult<T>> Func)
        {
            return Execute<T>("string", Func);
        }

        public static JsonPartResult<T> Execute<T>(String ResultType, Func<JsonPartResult<T>> Func)
        {
            JsonPartResult<T> lResult = new JsonPartResult<T>()
            {
                //ResultType = ResultType,
                Result = DefaultValueHelper.GetDefault<T>()
            };
            try
            {
                if (Func != null)
                {
                    var lTmpResult = Func();
                    lResult.CopyFrom(lTmpResult);
                }
            }
            catch (Exception ex)
            {
                lResult.Error = ex.Message;
                //lResult.IsLoginError = ex is SrvLoginException;
            }
            return lResult;
        }

        public static JsonResult<T> Execute<T>(Func<T> Func)
        {
            return Execute<T>("string", Func);
        }

        public static JsonResult<T> Execute<T>(String ResultType, Func<T> Func)
        {
            JsonResult<T> lResult = new JsonResult<T>()
            {
                //ResultType = ResultType,
                Result = DefaultValueHelper.GetDefault<T>()
            };
            try
            {
                if (Func != null)
                {
                    lResult.Result = Func();
                }
            }
            catch (Exception ex)
            {
                lResult.Error = ex.Message;
                //lResult.IsLoginError = ex is SrvLoginException;
            }
            return lResult;
        }

        public static JsonEmptyResult Execute(Action Func)
        {
            JsonEmptyResult lResult = new JsonEmptyResult();
            try
            {
                if (Func != null)
                {
                    Func();
                }
            }
            catch (Exception ex)
            {
                lResult.Error = ex.Message; // ErrorHelper.ToDisplayString(ex);
                                            //lResult.IsLoginError = ex is SrvLoginException;
            }
            return lResult;
        }
    }

    [DataContract]
    public class JsonEmptyResult
    {
        public JsonEmptyResult()
        {
            Error = "";
            //IsLoginError = false;
            //ResultType = "void";
        }

        [DataMember]
        public String Error { get; set; }

        //[DataMember]
        //public Boolean IsLoginError { get; set; }

        //[DataMember]
        //public String ResultType { get; set; }

        public virtual void CopyFrom(JsonEmptyResult Other)
        {
            this.Error = Other.Error;
            //this.IsLoginError = Other.IsLoginError;
            //this.ResultType = Other.ResultType;
        }
    }

    [DataContract]
    public class JsonResult<T> : JsonEmptyResult
    {
        public JsonResult()
            : base()
        {
            //ResultType = "";
            Result = DefaultValueHelper.GetDefault<T>();
            Error = "";
        }

        public JsonResult(Object OtherJsonResult, T NewResult)
            : base()
        {
            //ResultType = Convert.ToString(OtherJsonResult.GetType().GetProperty("ResultType").GetValue(OtherJsonResult, null));
            Error = Convert.ToString(OtherJsonResult.GetType().GetProperty("Error").GetValue(OtherJsonResult, null));
            //IsLoginError = Convert.ToBoolean(OtherJsonResult.GetType().GetProperty("IsLoginError").GetValue(OtherJsonResult, null));
            Result = NewResult;
        }

        [DataMember]
        public T Result { get; set; }

        public override void CopyFrom(JsonEmptyResult Other)
        {
            base.CopyFrom(Other);
            if (Other is JsonResult<T>)
            {
                this.Result = (Other as JsonResult<T>).Result;
            }
        }
    }

    [DataContract]
    public class JsonPartResult<T> : JsonResult<T>
    {
        public JsonPartResult()
            : base()
        {
        }

        public JsonPartResult(Object OtherJsonResult, T NewResult)
            : base(OtherJsonResult, NewResult)
        {
            var lOtherType = OtherJsonResult.GetType();
            var lPartIndexProp = lOtherType.GetProperty("PartIndex");
            var lPartCountProp = lOtherType.GetProperty("PartCount");
            if (lPartIndexProp != null)
            {
                this.PartIndex = Convert.ToInt32(lPartIndexProp.GetValue(OtherJsonResult, null));
            }
            if (lPartCountProp != null)
            {
                this.PartCount = Convert.ToInt32(lPartCountProp.GetValue(OtherJsonResult, null));
            }
        }

        [DataMember]
        public Int32 PartIndex { get; set; }

        [DataMember]
        public Int32 PartCount { get; set; }

        public override void CopyFrom(JsonEmptyResult Other)
        {
            base.CopyFrom(Other);
            var lOtherType = Other.GetType();
            var lPartIndexProp = lOtherType.GetProperty("PartIndex");
            var lPartCountProp = lOtherType.GetProperty("PartCount");
            if (lPartIndexProp != null)
            {
                this.PartIndex = Convert.ToInt32(lPartIndexProp.GetValue(Other, null));
            }
            if (lPartCountProp != null)
            {
                this.PartCount = Convert.ToInt32(lPartCountProp.GetValue(Other, null));
            }
            /*if (Other is JsonPartResult<T>)
            {
                this.PartIndex = (Other as JsonPartResult<T>).PartIndex;
                this.PartCount = (Other as JsonPartResult<T>).PartCount;
            }*/
        }
    }

    public static class DefaultValueHelper
    {
        public static T GetDefault<T>()
        {
            if (typeof(T) == typeof(String))
                return (T)(Object)null;
            else
                return default(T);
        }
    }

    [Serializable]
    public class SrvLoginException : Exception
    {
        public SrvLoginException() { }
        public SrvLoginException(string message) : base(message) { }
        public SrvLoginException(string message, Exception inner) : base(message, inner) { }
        protected SrvLoginException(
          SerializationInfo info,
          StreamingContext context) : base(info, context)
        { }
    }
}