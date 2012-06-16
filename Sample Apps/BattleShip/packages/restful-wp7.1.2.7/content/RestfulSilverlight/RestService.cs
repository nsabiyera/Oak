using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Newtonsoft.Json;
using System.Text;

namespace RestfulSilverlight
{
    public delegate void CompleteCallback();
    public delegate void CompleteCallback<T>(T result);

    public class RestService
    {
        private string _baseUri;
        public string BaseUri
        {
            get
            {
                return _baseUri;
            }
        }

        private RestFacilitator _restFacilitator;
        public RestFacilitator RestFacilitator
        {
            get
            {
                return _restFacilitator;
            }
        }

        private string _authorization;
        public string Authorization
        {
            get
            {
                return _authorization;
            }
        }

        public RestService(RestFacilitator restFacilitator, string baseUri, string authorization = null)
        {
            _restFacilitator = restFacilitator;
            _baseUri = baseUri;
            _authorization = authorization;
            if (_baseUri.EndsWith("/") == false)
            {
                _baseUri = _baseUri + "/";
            }
        }

        protected SendOrPostCallback Wrap<T>(CompleteCallback<T> complete) where T : class, new()
        {
            if (complete != null)
            {
                SendOrPostCallback post = (data) => complete(data as T);
                return post;
            }

            return null;
        }

        private string GenerateGetUri(string url, object data)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(BaseUri);

            if (!string.IsNullOrEmpty(url))
            {
                if (url != "/")
                {
                    sb.Append(url);
                }

                if (url.EndsWith("/") == false && !url.Contains("."))
                {
                    sb.Append("/");
                }
            }

            if (data != null)
            {
                bool isFirst = true;
                var propertyInfo = data.GetType().GetProperties().ToList();
                foreach (var property in propertyInfo)
                {
                    string value = property.GetValue(data, null).ToString();
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

        private string GeneratePostUri(string url)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(BaseUri);

            if (!string.IsNullOrEmpty(url))
            {
                if (url != "/")
                {
                    sb.Append(url);
                }

                if (url.EndsWith("/") == false)
                {
                    sb.Append("/");
                }
            }

            return ScrubForDuplicateSlashes(sb.ToString());
        }

        private Dictionary<string, string> GeneratePostDictonary(object resource)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            var propertyInfo = resource.GetType().GetProperties().ToList();
            foreach (var property in propertyInfo)
            {
                string value = property.GetValue(resource, null).ToString();
                parameters.Add(property.Name, value);
            }

            return parameters;
        }

        public void Call(string methodName, params object[] args)
        {
            this.GetType().GetMethod(methodName).Invoke(this, args);
        }

        public void Get<T>(string url, object resource, CompleteCallback<T> completeCallback, CompleteCallback<Exception> failedCallback) where T : class, new()
        {
            url = url ?? string.Empty;

            string fullyQualifiedUri = GenerateGetUri(url, resource);

            _restFacilitator.Get<T>(fullyQualifiedUri, Wrap<T>(completeCallback), Wrap<Exception>(failedCallback), _authorization);
        }

        public void Post<T>(string url, object data, CompleteCallback<T> completeCallback) where T : class, new()
        {
            string fullyQualifiedUri = GeneratePostUri(url);

            string jsonString = string.Empty;

            try
            {
                jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            }
            catch (JsonSerializationException ex)
            {
                throw new InvalidOperationException("A serialization exception occurred.  Make sure you have [assembly: InternalsVisibleTo(\"Newtonsoft.Json.WindowsPhone\")] in your AssemblyInfo.cs file so that anonymous types can be serialized by JSON.net.", ex);
            }

            _restFacilitator.Post<T>(fullyQualifiedUri, jsonString, Wrap<T>(completeCallback), _authorization);
        }

        public void Post(string url, object data, CompleteCallback completeCallback)
        {
            string fullyQualifiedUri = GeneratePostUri(url);

            string jsonString = string.Empty;

            try
            {
                jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            }
            catch (JsonSerializationException ex)
            {
                throw new InvalidOperationException("A serialization exception occurred.  Make sure you have [assembly: InternalsVisibleTo(\"Newtonsoft.Json.WindowsPhone\")] in your AssemblyInfo.cs file so that anonymous types can be serialized by JSON.net.", ex);
            }

            _restFacilitator.Post(fullyQualifiedUri, jsonString, (d) => completeCallback(), _authorization);
        }
    }
}
