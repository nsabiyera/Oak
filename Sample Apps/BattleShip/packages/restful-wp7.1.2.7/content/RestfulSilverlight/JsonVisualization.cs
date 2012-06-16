using System;
using System.Collections.Generic;

namespace RestfulSilverlight
{
    public class JsonVisualization
    {
        string jsonResult = "";

        public string JsonResult
        {
            get { return jsonResult; }
        }

        public void Parse(string name, object result, int tabIndent)
        {
            if (result is Hash)
            {
                WriteName(tabIndent, name, "Hash", null);

                Dictionary<string, object> dictionary = result.ToDictionary();
                foreach (KeyValuePair<string, object> kvp in dictionary)
                {
                    Parse(kvp.Key, kvp.Value, tabIndent + 1);
                }
            }
            else if (result is IEnumerable<object>)
            {
                WriteName(tabIndent, name, "Hashes Array []", null);

                foreach (var r in result as IEnumerable<object>)
                {
                    Parse("[]", r, tabIndent + 1);
                    WriteNewLine();
                }
            }
            else
            {
                if (result != null)
                {
                    WriteName(tabIndent, name, null, result);
                }
            }
        }

        void WriteName(int tabIndent, string name, string meta, object value)
        {
            WriteTabs(tabIndent);
            jsonResult += name;

            if (string.IsNullOrEmpty(meta) == false)
            {
                jsonResult += " (" + meta + ")";
            }

            jsonResult += ": ";

            if (value != null)
            {
                jsonResult += value.ToString();
            }

            jsonResult += Environment.NewLine;
        }

        void WriteTabs(int count)
        {
            for (int i = 0; i < count; i++)
            {
                jsonResult += "\t\t";
            }
        }

        void WriteNewLine()
        {
            jsonResult += Environment.NewLine;
        }
    }
}
