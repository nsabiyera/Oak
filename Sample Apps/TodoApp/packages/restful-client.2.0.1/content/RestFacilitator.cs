using System;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Specialized;
using Oak;

namespace RestfulClient
{
    public class RestFacilitator
    {
        public string BaseUri { get; set; }

        public string Authorization { get; set; }

        public RestFacilitator()
            : this(null, null)
        {
        }

        public RestFacilitator(string baseUri)
            : this(baseUri, null)
        {
        }

        public RestFacilitator(string baseUri, string authorization)
        {
            Authorization = authorization;

            BaseUri = baseUri;

            if (BaseUri.EndsWith("/") == false)
            {
                BaseUri = BaseUri + "/";
            }
        }

        public dynamic Get(string url)
        {
            return Get(url, null);
        }

        public dynamic Get(string url, object queryStringData)
        {
            var response = GetRaw(url, queryStringData);

            var stream = response.GetResponseStream();

            return Parse(stream, response.ContentType, response.Headers);
        }

        public WebResponse GetRaw(string url)
        {
            return GetRaw(url, null);
        }

        public WebResponse GetRaw(string url, object queryStringData)
        {
            string uriPath = GenerateUrl(url, queryStringData);

            var uri = new Uri(uriPath, UriKind.Absolute);

            var request = CreateRequest(uri, Authorization);

            return request.GetResponse();
        }

        public string ScrubbedContentType(string contentType, WebHeaderCollection headers)
        {
            if (headers["Application-Type"] != null) return headers["Application-Type"];

            var scrubbedContentType = contentType ?? "";

            if (scrubbedContentType.Contains(";"))
            {
                scrubbedContentType = scrubbedContentType.Substring(0, scrubbedContentType.IndexOf(";"));
            }

            return scrubbedContentType;
        }

        dynamic Parse(Stream stream, string contentType, WebHeaderCollection headers)
        {
            return JsonToDynamic.Parse(new StreamReader(stream).ReadToEnd());
        }

        private static HttpWebRequest CreateRequest(Uri uri, string authorization)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);

            if (!string.IsNullOrEmpty(authorization)) request.Headers["Authorization"] = authorization;

            return request;
        }

        public dynamic Post(string href)
        {
            return Post(href, null);
        }

        public dynamic Post(string href, object payload)
        {
            return ExecuteRequest(href, "POST", payload);
        }

        public void Delete(string href)
        {
            Delete(href, null);
        }

        public dynamic Delete(string href, object payload)
        {
            return ExecuteRequest(href, "DELETE", payload);
        }

        public dynamic Put(string href)
        {
            return Put(href, null);
        }

        public dynamic Put(string href, object payload)
        {
            return ExecuteRequest(href, "PUT", payload);
        }

        private dynamic ExecuteRequest(string href, string method, object payload)
        {
            var uriPath = GenerateUrl(href);

            var uri = new Uri(uriPath, UriKind.Absolute);

            var request = CreateRequest(uri, Authorization);

            request.Method = method;

            request.ContentType = "application/json";

            if (payload == null) request.ContentLength = 0;

            else
            {
                var requestStream = request.GetRequestStream();

                var sr = new StreamWriter(requestStream);

                if (payload is Gemini) sr.Write(DynamicToJson.Convert(payload));

                else sr.Write(JsonConvert.SerializeObject(payload));

                sr.Close();
            }

            var response = request.GetResponse();

            var stream = response.GetResponseStream();

            return Parse(stream, response.ContentType, response.Headers);
        }

        private string GenerateUrl(string url)
        {
            return GenerateUrl(url, null);
        }

        private string GenerateUrl(string url, object queryStringData)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(BaseUri);

            if (!string.IsNullOrEmpty(url))
            {
                if (url != "/")
                {
                    sb.Append(url);
                }

                if (!url.EndsWith("/") && !url.Contains("."))
                {
                    sb.Append("/");
                }
            }

            if (queryStringData != null)
            {
                bool isFirst = true;
                var propertyInfo = queryStringData.GetType().GetProperties().ToList();
                foreach (var property in propertyInfo)
                {
                    string value = property.GetValue(queryStringData, null).ToString();
                    if (isFirst)
                    {
                        sb.Append("?");
                        isFirst = false;
                    }
                    else
                    {
                        sb.Append("&");
                    }

                    sb.Append(property.Name);
                    sb.Append("=");
                    sb.Append(Uri.EscapeDataString(value));
                }
            }

            var uri = sb.ToString();

            return ScrubForDuplicateSlashes(uri);
        }

        private string ScrubForDuplicateSlashes(string uri)
        {
            return "http://" + uri.Replace("http://", "").Replace("//", "/");
        }
    }
}
