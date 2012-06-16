using System;
using System.Net;
using System.Threading;
using System.IO;

namespace RestfulSilverlight
{
    public class RestFacilitator
    {
        private SynchronizationContext _synchronizationContext;

        public RestFacilitator()
            : this(SynchronizationContext.Current)
        {
        }

        public RestFacilitator(SynchronizationContext synchronizationContext)
        {
            _synchronizationContext = synchronizationContext;

            if (_synchronizationContext == null) throw new InvalidOperationException(
                 "SynchronizationContext.Current is null...which probably means that you are newing up RestFacilitator on a seperate thread (as opposed to the UI thread).  You can do this, but you have to use the overloaded constructor that allows you to pass in the SynchronizationContext from the UI thread.");
        }

        public void Get<T>(string resource, SendOrPostCallback onComplete, SendOrPostCallback onFailure, string authorization) where T : class, new()
        {
            string uriPath = resource;

            Uri uri = new Uri(uriPath, UriKind.Absolute);

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);

            ApplyAuthorization(httpWebRequest, authorization);

            AsyncCallback done = new AsyncCallback(
            (e) =>
            {
                string requestResource = resource;

                HttpWebRequest request = (HttpWebRequest)e.AsyncState;

                ApplyAuthorization(request, authorization);

                HttpWebResponse response;

                try
                {
                    response = (HttpWebResponse)request.EndGetResponse(e);
                }
                catch (Exception ex)
                {
                    if (onFailure != null)
                    {
                        _synchronizationContext.Post(onFailure, ex);
                        return;
                    }

                    throw new InvalidOperationException("Unable to call: " + requestResource + ".", ex);
                }

                Stream stream = response.GetResponseStream();

                var result = new RestConverter().ConstructObject<T>(stream);

                _synchronizationContext.Post(onComplete, result);
            });

            httpWebRequest.BeginGetResponse(done, httpWebRequest);
        }

        public void Post<T>(string resource, string jsonString, SendOrPostCallback onComplete, string authorization) where T : class, new()
        {
            string uriPath = resource;

            Uri uri = new Uri(uriPath, UriKind.Absolute);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

            request.Method = "POST";

            ApplyAuthorization(request, authorization);

            AsyncCallback readResponse = null;

            AsyncCallback writeRequest = null;

            writeRequest = new AsyncCallback(
            (e) =>
            {
                request.ContentType = "application/json";

                Stream stream = request.EndGetRequestStream(e);

                StreamWriter streamWriter = new StreamWriter(stream);

                streamWriter.Write(jsonString);

                streamWriter.Close();

                stream.Close();

                request.BeginGetResponse(readResponse, null);
            });

            readResponse = new AsyncCallback(
            (e) =>
            {
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(e);

                var stream = response.GetResponseStream();

                var result = new RestConverter().ConstructObject<T>(stream);

                _synchronizationContext.Post(onComplete, result);
            });

            request.BeginGetRequestStream(writeRequest, null);
        }

        public void Post(string resource, string jsonString, SendOrPostCallback onComplete, string authorization)
        {
            string uriPath = resource;

            Uri uri = new Uri(uriPath, UriKind.Absolute);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

            ApplyAuthorization(request, authorization);

            request.Method = "POST";

            AsyncCallback readResponse = null;

            AsyncCallback writeRequest = null;

            writeRequest = new AsyncCallback(
            (e) =>
            {
                request.ContentType = "application/json";

                Stream stream = request.EndGetRequestStream(e);

                StreamWriter streamWriter = new StreamWriter(stream);

                streamWriter.Write(jsonString);

                streamWriter.Close();

                stream.Close();

                request.BeginGetResponse(readResponse, null);
            });

            readResponse = new AsyncCallback(
            (e) =>
            {
                _synchronizationContext.Post(onComplete, null);
            });

            request.BeginGetRequestStream(writeRequest, null);
        }

        private void ApplyAuthorization(HttpWebRequest request, string authorization)
        {
            if (string.IsNullOrEmpty(authorization)) return;

            request.Headers["Authorization"] = authorization;
        }
    }
}
