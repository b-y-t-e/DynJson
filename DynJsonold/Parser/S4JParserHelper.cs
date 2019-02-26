using System;
using System.Collections.Generic;
using System.Text;

namespace DynJson.Parser
{
    public static class S4JParserHelper
    {
        public static Int32? SkipWhiteSpaces(IList<char> chars, int index)
        {
            Int32? newIndex = chars.Count;
            for (var i = index; i < chars.Count; i++)
            {
                char ch = chars[i];
                if (!Char.IsWhiteSpace(ch))
                {
                    newIndex = i;
                    break;
                }
            }
            return newIndex;
        }

        public static bool Is(char[] chars, int index, char[] toFindChars)
        {
            if (toFindChars == null)
                return false;

            bool result = false;
            var j = toFindChars.Length - 1;
            var i = index + j;
            if (chars.Length > 0)
            {
                if (i >= chars.Length)
                {
                    result = false;
                    return result;
                }

                for (; ; i--, j--)
                {
                    if (j < 0)
                    {
                        result = true;
                        break;
                    }

                    if (i < 0)
                    {
                        break;
                    }

                    char ch = chars[i];
                    char toFindCh = toFindChars[j];
                    if (ch != toFindCh)
                    {
                        break;
                    }
                }
            }
            return result;
        }
    }
}
