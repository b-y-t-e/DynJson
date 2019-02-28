using DynJson.Helpers;
using DynJson.Executor;
using DynJson.Parser;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DynJson.Tokens;
using DynLan;
using DynLan.Classes;
using DynJson.Helpers.CoreHelpers;
using DynJson.Database;

namespace DynJson.Functions
{
    public class VarFunction : S4JStateFunction
    {
        public VarFunction() :
            this("@")
        {
            ReturnExactValue = true;
        }

        public VarFunction(params string [] aliases) :
            base(aliases)
        {
            Priority = 0;
            BracketsDefinition = new DynLanBrackets();
            CommentDefinition = new DynLanComment();
            QuotationDefinition = new DynLanQuotation();
            Evaluator = new DynLanEvaluator();
        }
    }

}
