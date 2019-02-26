using System;
using DynJson.Helpers.CoreHelpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynJson.Helpers.DatabaseHelpers
{
    public class DatabaseQueryHelper
    {
        public HashSet<String> Arguments
        {
            get;
            private set;
        }

        //////////////////////////////////////////

        public String Text
        {
            get;
            private set;
        }

        //////////////////////////////////////////

        public DatabaseQueryHelper()
        {
            Load("");
        }

        public DatabaseQueryHelper(String Text)
            : this()
        {
            Load(Text);
        }

        //////////////////////////////////////////

        public void Load(String Text)
        {
            this.Text = (Text ?? "");

            this.Arguments = new HashSet<string>();

            int isInsideName = 0;
            string name = "";

            for (var i = 0; i < this.Text.Length; i++)
            {
                char ch = this.Text[i];

                if (ch == '{')
                {
                    if (isInsideName > 0)
                        name += ch;
                    isInsideName++;
                }

                else if (ch == '}')
                {
                    isInsideName--;
                    if (isInsideName > 0)
                        name += ch;
                }

                else if (isInsideName > 0)
                    name += ch;

                if (isInsideName == 0 && name != "")
                {
                    this.Arguments.Add(name);
                    name = "";
                }
            }
            
            if (name != "")
            {
                this.Arguments.Add(name);
                name = "";
            }
        }

        public List<String> GetMissingParameters(IEnumerable<String> Parameters )
        {
            List<String> result = new List<String>();
            foreach (String argument in this.Arguments)
            {
                if (!Parameters.Any(p => p.EqualsNonsensitive(argument)))
                    result.Add(argument);
            }
            return result;
        }
    }
}
