using System;
using System.Collections.Generic;
using System.Text;

namespace Oak
{
    public class GeminiInfo
    {
        public static string Parse(dynamic o)
        {
            var sb = new StringBuilder();
            Parse(sb, "this", o, 0);
            return sb.ToString();
        }

        public static void Parse(StringBuilder stringBuilder, string name, dynamic o, int tab)
        {
            if (o is Gemini)
            {
                WriteName(stringBuilder, tab, name, o.GetType().Name, null);

                WriteDictionary(stringBuilder, o.Hash(), tab);
            }
            else if (o is IEnumerable<dynamic>)
            {
                WriteName(stringBuilder, tab + 1, name, "IEnumerable<dynamic>", null);
                int index = 0;
                foreach (var r in o as IEnumerable<dynamic>)
                {
                    Parse(stringBuilder, string.Format("[{0}]", index), r, tab + 2);
                    index++;
                }
            }
            else if (o is Prototype)
            {
                WriteName(stringBuilder, tab, name, o.GetType().Name, null);

                WriteDictionary(stringBuilder, o, tab);
            }
            else
            {
                if (o != null) WriteName(stringBuilder, tab, name, o.GetType().Name, o);

                else WriteName(stringBuilder, tab, name, "null", null);
            }
        }

        public static void WriteDictionary(StringBuilder stringBuilder, IDictionary<string, object> o, int tab)
        {
            foreach (var kvp in o) Parse(stringBuilder, kvp.Key, kvp.Value, tab + 1);
        }

        public static void WriteName(StringBuilder stringBuilder, int tabIndent, string name, string meta, dynamic value)
        {
            WriteTabs(stringBuilder, tabIndent);
            stringBuilder.Append(name);

            if (string.IsNullOrEmpty(meta) == false)
            {
                stringBuilder.Append(" (" + meta + ")");
            }

            if (value != null && !(value is Delegate))
            {
                stringBuilder.Append(": ");

                stringBuilder.Append(value.ToString());
            }

            stringBuilder.Append(Environment.NewLine);
        }

        public static void WriteTabs(StringBuilder stringBuilder, int count)
        {
            for (int i = 0; i < count; i++)
            {
                stringBuilder.Append("  ");
            }
        }
    }
}