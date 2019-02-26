using System;
using System.Collections.Generic;
using System.Text;

namespace DynJson.Classes
{
    public class Tag
    {
        public String Name { get; set; }

        public Object Value { get; set; }
    }

    public class Tags : List<Tag>
    {

    }
}
