using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.IO;

namespace DynJson.Helpers.CoreHelpers
{
    public static class MyStringHelper
    {
        private static readonly Char[] quotes = new[] { '\'', '"' };

        /*public static String MaxLength(this String Text, Int32 MaxLength)
        {
            Text = Text ?? "";
            return Text.Length > MaxLength ?
                Text.Substring(0, MaxLength) :
                Text;
        }*/

        public static String TrimStart(this String Text, String PrefixToRemove)
        {
            while (Text.StartsWith(PrefixToRemove))
                Text = Text.Substring(PrefixToRemove.Length);
            return Text;
        }

        public static Stream ToStream(this String Text, Encoding Enc = null)
        {
            if (Text == null)
                return null;

            if (Enc == null)
                Enc = Encoding.UTF8;

            return new MemoryStream(Enc.GetBytes(Text));
        }

        public static string ReplaceInsensitive(this string str, string oldValue, string newValue, StringComparison comparison)
        {
            StringBuilder sb = new StringBuilder();

            int previousIndex = 0;
            int index = str.IndexOf(oldValue, comparison);
            while (index != -1)
            {
                sb.Append(str.Substring(previousIndex, index - previousIndex));
                sb.Append(newValue);
                index += oldValue.Length;

                previousIndex = index;
                index = str.IndexOf(oldValue, index, comparison);
            }
            sb.Append(str.Substring(previousIndex));

            return sb.ToString();
        }

        public static IList<ReplaceInfo> GetReplaceInfos(this string str, string oldValue, StringComparison comparison)
        {
            List<ReplaceInfo> result = new List<ReplaceInfo>();
            //StringBuilder sb = new StringBuilder();

            //int previousIndex = 0;
            int index = str.IndexOf(oldValue, comparison);
            while (index != -1)
            {
                result.Add(new ReplaceInfo()
                {
                    Index = index,
                    Len = oldValue.Length
                });
                //sb.Append(str.Substring(previousIndex, index - previousIndex));
                //sb.Append(newValue);
                index += oldValue.Length;

                //previousIndex = index;
                index = str.IndexOf(oldValue, index, comparison);
            }
            //sb.Append(str.Substring(previousIndex));

            return result.OrderByDescending(i => i.Index).ToArray(); // sb.ToString();
        }

        public static String MaxLength(this String Text, Int32 MaxLength)
        {
            Text = Text ?? "";
            return Text.Length > MaxLength ?
                Text.Substring(0, MaxLength) :
                Text;
        }

#if SILVERLIGHT
#else
        public static StringBuilder AppendWithNewLine(
            this StringBuilder scriptCode,
            String Script)
        {
            if (scriptCode.Length > 0 && !scriptCode.EndsWith(Environment.NewLine))
                scriptCode.Append(Environment.NewLine);
            scriptCode.Append(Script);
            return scriptCode;
        }
#endif

        public static bool StartsWithAny(this String txt, String[] Items)
        {
            if (Items == null || Items.Length == 0)
                return true;

            foreach (var item in Items)
                if (txt.StartsWith(item))
                    return true;

            return false;
        }

        public static bool EndsWith(this StringBuilder sb, string test)
        {
            return EndsWith(sb, test, StringComparison.CurrentCulture);
        }

        public static bool EndsWith(this StringBuilder sb, string test,
            StringComparison comparison)
        {
            if (sb.Length < test.Length)
                return false;

            string end = sb.ToString(sb.Length - test.Length, test.Length);
            return end.Equals(test, comparison);
        }

        public static String GetPrefix(this String Line)
        {
            StringBuilder prefix = new StringBuilder();
            foreach (Char ch in Line)
            {
                if (Char.IsWhiteSpace(ch))
                    prefix.Append(ch);
                else
                    break;
            }
            return prefix.ToString();
        }

        public static String GetFirstWord(this String Line)
        {
            StringBuilder prefix = new StringBuilder();
            foreach (Char ch in Line.TrimStart())
            {
                if (!Char.IsWhiteSpace(ch) && ch != '(' && ch != ')' && ch != ':' && ch != '[' && ch != ']' && ch != '{' && ch != '}')
                    prefix.Append(ch);
                else
                    break;
            }
            return prefix.ToString();
        }

        public static String PrefixToEveryLine(this String Text, String InsertPrefix, String InsertPostfix = "")
        {
            StringBuilder str = new StringBuilder();
            foreach (String line in GetLines(Text))
            {
                str.AppendFormat("{0}{1}{2}{3}", InsertPrefix, line, InsertPostfix, Environment.NewLine);
            }
            return str.ToString();
        }

        /*public static String AddReturnStatement(this String Text)
        {
            if (!Text.Contains("return"))
            {
                StringBuilder str = new StringBuilder();
                IList<String> lines = GetLines(Text.TrimEnd());
                Int32 index = -1;

                foreach (String line in lines)
                {
                    index++;
                    if (index == lines.Count - 1)
                    {
                        var lineWithoutStart = line.TrimStart();
                        var prefix = line.Substring(0, line.Length - lineWithoutStart.Length);
                        str.AppendFormat("{0}return {1}{2}", prefix, lineWithoutStart, Environment.NewLine);
                    }
                    else
                    {
                        str.AppendFormat("{0}{1}", line, Environment.NewLine);
                    }
                }
                return str.ToString();
            }
            else
            {
                return Text;
            }
        }*/

        public static String JoinString<T>(this IEnumerable<T> Elements, String Separator = ",")
        {
            StringBuilder lTxt = new StringBuilder();
            if (Elements != null)
                foreach (var lItem in Elements)
                {
                    if (lItem != null)
                    {
                        if (lTxt.Length > 0) lTxt.Append(Separator);
                        lTxt.Append(lItem);
                    }
                }
            return lTxt.ToString();
        }

        public static String JoinSql<T>(this IEnumerable<T> Elements, Boolean WrapWithComas, Boolean ToLower = false, String Separator = ",")
        {
            StringBuilder lTxt = new StringBuilder();
            if (Elements != null)
                foreach (var lItem in Elements)
                {
                    if (lTxt.Length > 0) lTxt.Append(Separator);

                    if (lItem != null)
                    {
                        if (WrapWithComas)
                        {
                            lTxt.
                                Append("'").
                                Append(
                                    ToLower ?
                                    Convert.ToString(lItem, CultureInfo.InvariantCulture).Replace("'", "''").ToLower() :
                                    Convert.ToString(lItem, CultureInfo.InvariantCulture).Replace("'", "''")).
                                Append("'");
                        }
                        else
                        {
                            lTxt.Append(lItem);
                        }
                    }
                    else
                    {
                        lTxt.Append("NULL");
                    }
                }
            return lTxt.ToString();
        }

        public static String Join<T>(this IEnumerable<T> Elements, String Separator)
        {
            StringBuilder lTxt = new StringBuilder();
            if (Elements != null)
                foreach (var lItem in Elements)
                {
                    if (lTxt.Length > 0) lTxt.Append(Separator);
                    lTxt.Append(lItem != null ? Convert.ToString(lItem, CultureInfo.InvariantCulture) : "");
                }
            return lTxt.ToString();
        }

        public static String Join(this IEnumerable<String> Texts, String Separator)
        {
            StringBuilder lTxt = new StringBuilder();
            if (Texts != null)
                foreach (var lItem in Texts)
                {
                    if (lTxt.Length > 0) lTxt.Append(Separator);
                    lTxt.Append(lItem ?? "");
                }
            return lTxt.ToString();
        }

        public static IEnumerable<String> OnlyNumbers(this IEnumerable<String> Texts)
        {
            Decimal lV = 0;
            if (Texts != null)
                foreach (var lText in Texts)
                {
                    if (!string.IsNullOrEmpty(lText))
                        if (Decimal.TryParse(lText.Trim(), out lV))
                            yield return lText;
                }
        }

        public static Boolean IsNumber(this String Text)
        {
            Text = (Text ?? "").Trim();
            if (Text.Length == 0)
                return false;
            foreach (Char ch in Text)
                if (!Char.IsNumber(ch))
                    return false;
            return true;
        }

        public static Boolean IsQuotedText(this String Text)
        {
            Text = (Text ?? "").Trim();
            if (Text.StartsWith("'") && Text.EndsWith("'"))
                return true;
            if (Text.StartsWith("\"") && Text.EndsWith("\""))
                return true;
            return false;
        }

        public static Object ParseJsonOrText(this String Text)
        {
            try
            {
                if (Text == null)
                    return null;

                if (Text == "null")
                    return null;

                if (MyStringHelper.IsNumber(Text.Trim()) ||
                    MyStringHelper.IsQuotedText(Text.Trim()))
                {
                    return Text.DeserializeJson();
                }
                else
                {
                    return Text.Trim();
                }
            }
            catch
            {
                throw;
            }
        }

        public static Boolean StartsWithNumber(this String Text)
        {
            Text = (Text ?? "").Trim();
            foreach (Char ch in Text)
                if (Char.IsNumber(ch))
                    return true;
                else
                    return false;
            return false;
        }

        public static Boolean IsWhiteString(this String Text)
        {
            return String.IsNullOrEmpty(Text) || Text.Trim().Length == 0;
        }

        public static String[] Split(this String Text, params String[] Separators)
        {
            return SplitWithDefault(Text, null, Separators);
        }

        public static String[] SplitWithDefault(this String Text, String ValueForEmpty, params String[] Separators)
        {
            var lR = (Text ?? "").Split(Separators, StringSplitOptions.RemoveEmptyEntries);
            lR = lR ?? new String[0];
            if (lR.Length == 0 && ValueForEmpty != null) return new String[] { ValueForEmpty };
            return lR;
        }

        public static String[] GetLines(this String Text)
        {
            return Text.Split(
                new[] { Environment.NewLine, "\n", "\r" },
                StringSplitOptions.None);
        }

        public static Boolean EqualsNonsensitive(this String Str1, String Str2)
        {
            if (Str1 != null && Str2 == null) return false;
            else if (Str1 == null && Str2 != null) return false;
            else if (Str1 == null && Str2 == null) return true;
            else
            {
                return Str2.ToLower().Equals(Str1.ToLower());
            }
        }

        public static Char Get(this String Txt, Int32 Index)
        {
            if (Txt == null || Index < 0 || Index >= Txt.Length)
                return ' ';
            else
                return Txt[Index];
        }

        public static Char GetFromTail(this String Txt, Int32 Index)
        {
            var lInd = Txt.Length - 1 - Index;
            if (Txt == null || Index < 0 || Index >= Txt.Length)
                return ' ';
            else
                return Txt[lInd];
        }

        public static String Set(this String Txt, Int32 Index, Char Value)
        {
            if (Txt == null || Index < 0 || Index >= Txt.Length)
                return Txt ?? "";
            else
                return Txt.Remove(Index, 1).Insert(Index, Value.ToString());
        }

        public static String SetToTail(this String Txt, Int32 Index, Char Value)
        {
            var lInd = Txt.Length - 1 - Index;
            if (Txt == null || Index < 0 || Index >= Txt.Length)
                return Txt ?? "";
            else
                return Txt.Remove(lInd, 1).Insert(lInd, Value.ToString());
        }

        /// <summary>
        /// Split with Quotes
        /// </summary>
        public static List<String> SplitQ(this String Text, Char[] Separators, Boolean AddEmpty = true, Char[] Quotes = null)
        {
            if (Quotes == null)
                Quotes = quotes;

            List<String> items = new List<String>();
            StringBuilder currentItem = new StringBuilder();
            Boolean insideQuote = false;

            if (Text == null)
                return items;

            Char[] text = Text.ToCharArray();
            for (var i = 0; i < text.Length; i++)
            {
                Char ch = text[i];

                if (Quotes.Contains(ch))
                {
                    if (!insideQuote)
                    {
                        insideQuote = true;
                    }
                    else
                    {
                        insideQuote = false;
                    }
                    currentItem.Append(ch);
                }
                else
                {
                    if (insideQuote)
                    {
                        currentItem.Append(ch);
                    }
                    else
                    {
                        if (Separators.Contains(ch))
                        {
                            if (currentItem.Length > 0 || AddEmpty)
                            {
                                items.Add(currentItem.ToString());
                                currentItem.Clear();
                            }
                        }
                        else
                        {
                            currentItem.Append(ch);
                        }
                    }
                }
            }

            if (currentItem.Length > 0)
            {
                items.Add(currentItem.ToString());
                currentItem.Clear();
            }

            return items;
        }

        /// <summary>
        /// Get text without quotes
        /// </summary>
        public static String GetTextWithoutQuotes(this String Text, Char[] Quotes = null)
        {
            if (Quotes == null)
                Quotes = quotes;

            StringBuilder outText = new StringBuilder();
            Boolean insideQuote = false;

            if (Text == null)
                return null;

            Char[] text = Text.ToCharArray();
            for (var i = 0; i < text.Length; i++)
            {
                Char ch = text[i];

                if (Quotes.Contains(ch))
                {
                    if (!insideQuote)
                    {
                        insideQuote = true;
                    }
                    else
                    {
                        insideQuote = false;
                    }
                }
                else
                {
                    if (!insideQuote)
                    {
                        outText.Append(ch);
                    }
                }
            }

            return outText.ToString();
        }
        public static Boolean SequenceEqualInsensitive(
    IList<Char> Items1,
    IList<Char[]> ListOfItems2)
        {
            foreach (Char[] items2 in ListOfItems2)
                if (SequenceEqualInsensitive(Items1, items2))
                    return true;
            return false;
        }

        public static Boolean SequenceEqualInsensitive(
            this IList<Char> Items1,
            IList<Char> Items2)
        {
            if (Items1 != null && Items2 != null && Items1.Count == Items2.Count)
            {
                Int32 c = Items1.Count;
                for (Int32 i = 0; i < c; i++)
                    if (Char.ToLowerInvariant(Items1[i]) != Char.ToLowerInvariant(Items2[i]))
                        return false;
                return true;
            }
            else if (Items1 == null && Items2 == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static String FormatDate(DateTime DateTime, String Format)
        {
            String r = Format ?? "";
            r = r.Replace("mm", FormatDatePart(DateTime.Month));
            r = r.Replace("yyyy", DateTime.Year.ToString());
            r = r.Replace("dd", FormatDatePart(DateTime.Day));

            r = r.Replace("hh", FormatDatePart(DateTime.Hour));
            r = r.Replace("mi", FormatDatePart(DateTime.Minute));
            r = r.Replace("ss", FormatDatePart(DateTime.Second));
            r = r.Replace("ms", FormatDatePart(DateTime.Millisecond, 3));
            r = r.Replace("mmm", FormatDatePart(DateTime.Millisecond, 3));
            return r;
        }

        private static String FormatDatePart(Int32 Val, Int32 Count = 2)
        {
            var a = Val.ToString();
            if (Count == 2)
                return a.Length == 2 ? a : ("0" + a);
            while (a.Length < Count)
                a = "0" + a;
            return a;
        }

        public static String RemoveBracketsFromMethodCall(this String Text)
        {
            Text = (Text ?? "").Trim();
            if (Text.StartsWith("(")) Text = Text.Substring(1);
            if (Text.EndsWith(")")) Text = Text.Substring(0, Text.Length - 1);
            return Text;
        }

        //////////////////////////////

        /*public static IEnumerable<Char> Substring(
            List<Char> Chars,
            Int32 StartIndex,
            Int32? Length = null)
        {
            return Substring((IList<Char>)Chars, StartIndex, Length);
        }

        public static IEnumerable<Char> Substring(
            this IList<Char> Chars,
            Int32 StartIndex,
            Int32? Length = null)
        {
            Int32 max = Length == null ?
                Chars.Count :
                (StartIndex + Length > Chars.Count ? Chars.Count : StartIndex + Length.Value);

            //List<Char> outChars = new List<Char>();
            for (var i = StartIndex; i < max; i++)
                yield return Chars[i];
            //outChars.Add(Chars[i]);
            //return outChars;
        }*/

        public static int IndexOf(this List<char> haystack, List<char> needle, Int32 StartIndex = 0)
        {
            var len = needle.Count;
            var limit = haystack.Count - len;
            for (var i = StartIndex; i <= limit; i++)
            {
                var k = 0;
                for (; k < len; k++)
                {
                    if (needle[k] != haystack[i + k]) break;
                }
                if (k == len) return i;
            }
            return -1;
        }

        public static Int32 IndexOf2(this List<char> Text, List<char> TextPart, Int32 StartIndex = 0)
        {
            int result = -1;
            if (StartIndex < 0)
                StartIndex = 0;

            if (StartIndex >= Text.Count)
                return result;

            if (TextPart == null || TextPart.Count == 0)
                return result;

            var c1 = Text.Count;
            var c2 = TextPart.Count;

            for (var i = StartIndex; i < c1; i++)
            {
                var found = true;

                for (var j = 0; j < c2; j++)
                {
                    if (i + j >= c1)
                        return result;

                    var txtCh = Text[i + j];
                    var partCh = TextPart[j];
                    if (txtCh != partCh)
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                {
                    result = i;
                    break;
                }
            }

            return result;
        }

        public static List<Char> Append(
            this List<Char> Chars1,
            List<Char> Chars2)
        {
            if (Chars2 != null)
                Chars1.AddRange(Chars2);
            return Chars1;
        }

        /* public static List<Char> Append(
             this List<Char> Chars1,
             String Chars2)
         {
             if (Chars2 != null)
                 Chars1.AddRange(Chars2.ToCharArray());
             return Chars1;
         }*/

        public static List<Char> RemoveFromString(
            this List<Char> Chars,
            Int32 StartIndex,
            Int32 Length)
        {
            var len = Length;
            var end = StartIndex + len;
            if (end >= Chars.Count)
            {
                len -= (end - Chars.Count);
                //d = Chars.Count;
            }


            /*for (var i = StartIndex; i < end; i++)
                Chars.RemoveAt(StartIndex);*/
            Chars.RemoveRange(StartIndex, len);
            return Chars;
        }

        public static List<Char> Substring(
            this List<Char> Chars,
            Int32 StartIndex,
            Int32? Length = null)
        {
            /*Int32 max = Length == null ?
                Chars.Count :
                (StartIndex + Length > Chars.Count ? Chars.Count : StartIndex + Length.Value);*/

            //List<Char> outChars = new List<Char>();

            if (Length != null)
            {
                var c = Chars.Count;
                for (var i = StartIndex + Length.Value; i < c; i++)
                    Chars.RemoveAt(Chars.Count - 1);
            }

            for (var i = 0; i < StartIndex; i++)
                Chars.RemoveAt(0);


            return Chars;

            //for (var i = StartIndex; i < max; i++)
            //   yield return Chars[i];
            //outChars.Add(Chars[i]);
            //return outChars;
        }

        public static IEnumerable<Char> Substring2(
            List<Char> Chars,
            Int32 StartIndex,
            Int32? Length = null)
        {
            return Substring2((IList<Char>)Chars, StartIndex, Length);
        }

        public static IEnumerable<Char> Substring2(
            this IList<Char> Chars,
            Int32 StartIndex,
            Int32? Length = null)
        {
            Int32 max = Length == null ?
                Chars.Count :
                (StartIndex + Length > Chars.Count ? Chars.Count : StartIndex + Length.Value);

            List<Char> outChars = new List<Char>();
            for (var i = StartIndex; i < max; i++)
                yield return Chars[i];
        }

        public static IEnumerable<Char> Replace2(
            this IEnumerable<Char> Chars,
            Char From,
            Char? To)
        {
            foreach (Char ch in Chars)
                if (ch == From)
                {
                    if (To != null)
                        yield return To.Value;
                }
                else
                    yield return ch;
        }

        public static String ToString2(
            this IEnumerable<Char> Chars)
        {
            StringBuilder str = new StringBuilder();
            foreach (Char ch in Chars)
                str.Append(ch);
            return str.ToString();
        }

        public static void Trim(
            this IList<Char> Chars,
            Char? trimchar = null)
        {
            for (var i = Chars.Count - 1; i >= 0; i--)
            {
                char ch = Chars[i];
                if (trimchar == null ? Char.IsWhiteSpace(ch) : trimchar == ch)
                    Chars.RemoveAt(i);
                else
                    break;
            }

            while (Chars.Count > 0)
            {
                char ch = Chars[0];
                if (trimchar == null ? Char.IsWhiteSpace(ch) : trimchar == ch)
                    Chars.RemoveAt(0);
                else
                    break;
            }
        }

        public static void TrimEnd(
            this IList<Char> Chars,
            Char? trimchar = null)
        {
            for (var i = Chars.Count - 1; i >= 0; i--)
            {
                char ch = Chars[i];
                if (trimchar == null ? Char.IsWhiteSpace(ch) : trimchar == ch)
                    Chars.RemoveAt(i);
                else
                    break;
            }
        }

        public static IEnumerable<Char> TrimStart(
            this IEnumerable<Char> Chars,
             Char[] Char)
        {
            foreach (var ch in Char)
                Chars = TrimStart(Chars, ch);
            return Chars;
        }

        public static IEnumerable<Char> TrimStart(
            this IEnumerable<Char> Chars,
            Char? trimchar = null)
        {
            Boolean areWhiteChars = true;
            foreach (char ch in Chars)
            {
                if (trimchar == null ? Char.IsWhiteSpace(ch) : trimchar == ch)
                {

                }
                else
                {
                    areWhiteChars = false;
                }

                if (!areWhiteChars)
                    yield return ch;
            }
        }

        public static Boolean StartsWith(
            this IList<Char> Chars,
            String txt,
            Int32 StartIndex = 0)
        {
            var txtArray = txt.ToCharArray();
            return StartsWith(Chars, txtArray, StartIndex);
        }

        public static Boolean StartsWith(
            this IList<Char> Chars,
            IList<Char> txtArray,
            Int32 StartIndex = 0)
        {
            var i = -1;
            //foreach (char ch in Chars)
            for (var j = StartIndex; j < Chars.Count; j++)
            //for( var i=0;i<txtArray.Length; i++)
            {
                var ch = Chars[j];
                i++;

                if (i >= txtArray.Count)
                    break;

                var txtCh = txtArray[i];

                if (txtCh != ch)
                    return false;

                //return ch == startChar;
            }
            return true;
        }

        public static Boolean StartsWith(
            this IEnumerable<Char> Chars,
            Char startChar)
        {
            foreach (char ch in Chars)
            {
                return ch == startChar;
            }
            return false;
        }

        public static Boolean IsNumber(
            IList<Char> Chars)
        {
            if (Chars.Count == 0)
            {
                return false;
            }
            else
            {
                foreach (var lChar in Chars)
                    if (!Char.IsNumber(lChar) && lChar != '.' && lChar != ',' && lChar != '-')
                        return false;
                return true;
            }
        }

        public static Boolean IsString(
            IList<Char> Chars,
            Char StringChar = '\'')
        {
            if (Chars.Count < 2)
            {
                return false;
            }
            else
            {
                if (Chars[0] == StringChar && Chars[Chars.Count - 1] == StringChar)
                {
                    for (int i = 1; i < Chars.Count - 1; i++)
                    {
                        Char? lPrevChar = i > 0 ? (Char?)Chars[i - 1] : null;
                        Char lChar = Chars[i];

                        if (lChar == StringChar)
                        {
                            if (lPrevChar != '\\')
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /*public static Boolean IsDateTime(
            IList<Char> Chars)
        {
            if (Chars.Count == 0)
            {
                return false;
            }
            else
            {
                if (Chars[0] == '#' && Chars[Chars.Count - 1] == '#')
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        */

        public static Char[] StrEquals(
            IList<Char> Source,
            IList<Char[]> ItemsToFind,
            Boolean Insensitive)
        {
            foreach (Char[] item in ItemsToFind)
                if (StrEquals(Source, item, Insensitive))
                    return item;
            return null;
        }

        public static Char[] StrEquals(
            IList<Char> Source,
            IList<Char[]> ItemsToFind,
            Int32 StartIndex,
            Boolean Insensitive)
        {
            foreach (Char[] item in ItemsToFind)
                if (StrEquals(Source, item, StartIndex, Insensitive))
                    return item;
            return null;
        }

        public static Boolean StrEquals(
            IList<Char> Source,
            IList<Char> ItemToFind,
            Boolean Insensitive)
        {
            if (Source == null || ItemToFind == null)
                return false;

            Int32 length = ItemToFind.Count;
            if (length > Source.Count)
                return false;

            for (int i = 0; i < length; i++)
                if (Insensitive)
                {
                    if (Char.ToLowerInvariant(Source[i]) != Char.ToLowerInvariant(ItemToFind[i]))
                        return false;
                }
                else
                {
                    if (Source[i] != ItemToFind[i])
                        return false;
                }

            return true;
        }

        public static Boolean StrEquals(
            IList<Char> Source,
            IList<Char> ItemToFind,
            Int32 StartIndex,
            Boolean Insensitive)
        {
            if (Source == null || ItemToFind == null)
                return false;

            else if (StartIndex + ItemToFind.Count > Source.Count)
                return false;

            else
            {
                Int32 length = StartIndex + ItemToFind.Count;
                Int32 j = 0;
                for (int i = StartIndex; i < length; i++)
                {
                    if (Insensitive)
                    {
                        if (Char.ToLowerInvariant(Source[i]) != Char.ToLowerInvariant(ItemToFind[j]))
                            return false;
                    }
                    else
                    {
                        if (Source[i] != ItemToFind[j])
                            return false;
                    }
                    j++;
                }
                return true;
            }
        }
    }

    public class ReplaceInfo
    {
        public Int32 Index;

        public Int32 Len;
    }
}
