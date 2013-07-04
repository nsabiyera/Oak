using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Oak;

namespace Oak
{
    public class JsonToDynamic
    {
        public static dynamic Parse(string jsonString)
        {
            try
            {
                var result = JsonToDynamic.ToDynamic(JsonConvert.DeserializeObject<JContainer>(jsonString));

                return result;
            }
            catch
            {
                try
                {
                    var jarray = JsonConvert.DeserializeObject<JArray>(jsonString);

                    return JsonToDynamic.ToArray(jarray);
                }
                catch
                {
                    bool valueAsBool = false;
                    int valueAsInt = 0;
                    double valueAsDouble = 0;
                    DateTime valueAsDateTime = DateTime.MinValue;

                    if (bool.TryParse(jsonString, out valueAsBool)) return valueAsBool;

                    if (int.TryParse(jsonString, out valueAsInt)) return valueAsInt;

                    if (double.TryParse(jsonString, out valueAsDouble)) return valueAsDouble;

                    if (DateTime.TryParse(jsonString, out valueAsDateTime)) return valueAsDateTime;

                    return jsonString;
                }
            }
        }

        public static dynamic ToDynamic(object value)
        {
            if (value is JValue) return (value as JValue).Value;

            if (value is JObject)
            {
                var gemini = new Gemini();

                foreach (var item in (value as JObject)) gemini.SetMember(item.Key, ToDynamic(item.Value));

                return gemini;
            }

            if (value is JArray) return ToArray(value as JArray);

            return value;
        }

        public static List<dynamic> ToArray(JArray array)
        {
            var list = new List<dynamic>();

            foreach (JToken item in array) list.Add(ToDynamic(item));

            return list;
        }
    }
}
