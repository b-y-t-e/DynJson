using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using DynJson.Helpers;
using System.Reflection;
using DynJson.Helpers.CoreHelpers;

namespace DynJson.Helpers.DatabaseHelpers
{
    public class MyDbReader<T> : IDisposable
    {
        public Int32 Count;

        /*public Dictionary<String, Int32> I;

        public Dictionary<String, Type> T;*/

        //public Dictionary<Int32, Boolean> Valid;

        public Dictionary<Int32, Type> DestTypes;

        public Dictionary<Int32, PropertyInfo> DestSetters;

        public Dictionary<Int32, String> Names;

        public IDataReader R;

        public MyDbReader(IDataReader Reader)
        {
            var destType = typeof(T);
            this.Count = Reader.FieldCount;
            this.DestTypes = new Dictionary<Int32, Type>();
            this.DestSetters = new Dictionary<Int32, PropertyInfo>();
           // this.Names = new Dictionary<int, string>();
            //this.Valid = new Dictionary<int, bool>();

            this.R = Reader;
            for (var i = 0; i < Reader.FieldCount; i++)
            {
                var name = Reader.GetName(i);
                var setter = RefUnsensitiveHelper.I.GetProperty(destType, name);
                if (setter != null)
                {
                    var propertyType = setter.PropertyType;
                    if (propertyType != null)
                    {
                        this.DestTypes[i] = propertyType;
                        this.DestSetters[i] = setter;
                      //  this.Names[i] = name;
                        //this.Valid[i] = propertyType != null && setter != null;
                    }
                }
            }
        }

        public void Dispose()
        {
            this.R = null;
            this.DestTypes.Clear();
            this.DestTypes = null;
            this.DestSetters.Clear();
            this.DestSetters = null;
            //this.Names = null;
            //this.Valid = null;
        }
    }
}
