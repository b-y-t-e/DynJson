using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace DynJson.Helpers.CoreHelpers
{
    public class MySettings
    {
        private readonly char[] _valueSeparators = new[] { ';' };

        private readonly char[] _innerSeparators = new[] { ':' };

        private Dictionary<String, String> _values;

        private List<KeyValuePair<String, String>> _list;

        //////////////////////////////////////////

        public KeyValuePair<String, String> this[Int32 Index]
        {
            get { return _list[Index]; }
            // set { _values[StyleName] = value; }
        }

        public String this[String Key]
        {
            get { return (_values.ContainsKey(Key) ? _values[Key].Trim() : null); }
            set { _values[Key] = value; }
        }

        public Int32 Count
        {
            get { return _values.Count; }
        }

        //////////////////////////////////////////

        public MySettings(String Text)
        {
            _list = new List<KeyValuePair<string, string>>();
            _values = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(Text))
            {
                foreach (var pair in Text.SplitQ(_valueSeparators).Select(v => v.SplitQ(_innerSeparators)).Where(v => v.Count <= 2))
                {
                    var key = (pair[0] ?? "").Trim();
                    var val = (pair.Count > 1 ? pair[1] : "").Trim();
                    _values[key] = val;
                    _list.Add(new KeyValuePair<string, string>(key, val));
                }
            }
        }

        //////////////////////////////////////////

        public Dictionary<String, String> ToDictionary()
        {
            return new Dictionary<string, string>(
                _values);
        }

        public override string ToString()
        {
            return
                String.Join(
                    ";",
                    _values.Select(i => String.Format("{0}:{1}", i.Key, i.Value)).ToArray());
        }
    }
}
