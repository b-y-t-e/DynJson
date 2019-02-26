using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DynJson.Helpers.CoreHelpers;
using System.Web;
using System.Xml.Linq;

namespace DynJson.Helpers.WebHelpers
{
    public class QueryStringHelper
    {
        public String Method
        {
            get;
            set;
        }

        public String CallbackMethod
        {
            get;
            set;
        }

        public Dictionary<String, Object> Arguments
        {
            get;
            set;
        }

        public Dictionary<String, Object> ArgumentsLowercase
        {
            get;
            set;
        }


        public Dictionary<String, String> ArgumentsStrings
        {
            get;
            set;
        }

        public Boolean IsEmpty
        {
            get
            {
                return (Method ?? "").Trim() == "" && Arguments.Count == 0;
            }
        }

        //////////////////////////////////////////

        public String Text
        {
            get;
            private set;
        }

        //////////////////////////////////////////

        public QueryStringHelper()
        {
            Load("");
        }

        public QueryStringHelper(String Text)
            : this()
        {
            Load(Text);
        }

        //////////////////////////////////////////

        public void Load(String Text)
        {
            this.Text = (Text ?? "").Trim().TrimStart('/');
            string argumentsAsString = "";
            string callbackAsString = "";

            int index = this.Text.IndexOf("?");
            if (index >= 0)
            {
                this.Method = this.Text.Substring(0, index);
                argumentsAsString = this.Text.Substring(index + 1);
            }
            else
            {
                this.Method = this.Text;
                argumentsAsString = "";
            }

            int callbackIndex = this.Text.LastIndexOf("?");
            if (index != callbackIndex)
            {
                callbackIndex = argumentsAsString.LastIndexOf("?");
                callbackAsString = argumentsAsString.Substring(callbackIndex + 1);

                if (callbackAsString.StartsWith("callback="))
                {
                    this.CallbackMethod = callbackAsString.Substring("callback=".Length);
                    this.CallbackMethod = this.CallbackMethod.Split(new[] { '&' })[0];
                    argumentsAsString = argumentsAsString.Substring(0, callbackIndex);
                }
            }

            IList<string> parts = argumentsAsString.
                Split(new[] { '&' });

            this.ArgumentsStrings = new Dictionary<string, string>();

            foreach (string part in parts)
            {
                if (part.Trim() == "")
                    continue;

                index = part.IndexOf("=");

                string argName = "";
                string argValue = "";

                if (index >= 0)
                {
                    argName = part.Substring(0, index);

                    argValue = part.Substring(index + 1);
                    argValue = HttpUtility.UrlDecode(argValue);
                }
                else
                {
                    argName = part;
                    argValue = "";
                }

                Object objValue = argValue.DeserializeJson();
                this.ArgumentsStrings[argName] = argValue;
            }

            LoadArguments();
        }

        public void LoadArguments()
        {
            this.Arguments = new Dictionary<string, object>();
            this.ArgumentsLowercase = new Dictionary<string, object>();
            foreach (var argName in ArgumentsStrings.Keys)
            {
                var argValue = ArgumentsStrings[argName];

                Object objValue = argValue.DeserializeJson();
                this.Arguments[argName] = objValue;
                this.ArgumentsLowercase[argName.ToLower()] = objValue;
            }
        }

        public void CorrectJsonToXml()
        {
            foreach (String key in Arguments.Keys.ToArray())
            {
                Object objValue = Arguments[key];
                String argValue = ArgumentsStrings[key];

                if (objValue is JObject)
                {
                    XNode node = JsonConvert.DeserializeXNode(argValue, "Root");
                    objValue = node.ToString();
                    Arguments[key] = objValue;
                    ArgumentsLowercase[key.ToLower()] = objValue;
                }
                else if (objValue is JArray)
                {
                    XNode node = JsonConvert.DeserializeXNode("{Item:" + argValue + "}", "Root");
                    objValue = node.ToString();
                    Arguments[key] = objValue;
                    ArgumentsLowercase[key.ToLower()] = objValue;
                }
            }
        }
    }
}
