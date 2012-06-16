using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization.Json;
using System.IO;

namespace RestfulSilverlight
{
    public class RestConverter
    {
        public object ConstructObject<T>(Stream stream) where T : class, new()
        {
            if (typeof(T) == typeof(Hash))
            {
                StreamReader streamReader = new StreamReader(stream);

                JToken j = JObject.Parse(streamReader.ReadToEnd());

                return ToDictionary(j);
            }
            else if (typeof(T) == typeof(Hashes))
            {
                StreamReader streamReader = new StreamReader(stream);

                JToken j = JArray.Parse(streamReader.ReadToEnd());

                object[] enumerable = ToDictionary(j) as object[];

                var hashes = new Hashes();

                foreach (object item in enumerable)
                {
                    hashes.Add(item as Hash);
                }

                return hashes;
            }
            else
            {
                DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(T));

                T result = new T();

                result = dataContractJsonSerializer.ReadObject(stream) as T;

                return result;
            }
        }

        public object ToDictionary(JToken token)
        {
            if (token is JObject)
            {
                Hash dictionary = new Hash();

                (from childToken in ((JToken)token) where childToken is JProperty select childToken as JProperty).ToList().ForEach(property =>
                {
                    dictionary.Add(property.Name, ToDictionary(property.Value));
                });

                return dictionary;
            }

            if (token is JValue)
            {
                return ((JValue)token).Value;
            }

            if (token is JArray)
            {
                object[] array = new object[((JArray)token).Count];
                int index = 0;
                foreach (JToken arrayItem in ((JArray)token))
                {
                    array[index] = ToDictionary(arrayItem);
                    index++;
                }

                return array;
            }

            throw new ArgumentException(string.Format("Unknown token type '{0}'", token.GetType()), "token");
        }
    }
}
