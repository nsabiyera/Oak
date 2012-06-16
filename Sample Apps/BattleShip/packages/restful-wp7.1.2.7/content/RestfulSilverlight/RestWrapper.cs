using System;

namespace RestfulSilverlight
{
    public class RestWrapper
    {
        protected object _nextContainer;
        protected string _baseUri;
        protected RestFacilitator _restFacilitator;
        protected RestService _restService;
        protected object _route;
        protected string _url;

        public RestWrapper(string url, object route, string baseUri, RestFacilitator restFacilitator, RestService restService)
        {
            _url = url;
            _route = route;
            _baseUri = baseUri;
            _restFacilitator = restFacilitator;
            _restService = restService;
        }

        public GetWrapper<T> ThenGet<T>(string url, object route) where T : class, new()
        {
            _nextContainer = new GetWrapper<T>(url, route, _baseUri, _restFacilitator, _restService);
            return _nextContainer as GetWrapper<T>;
        }

        public GetWrapper<T> ThenGet<T>(string url) where T : class, new()
        {
            _nextContainer = new GetWrapper<T>(url, null, _baseUri, _restFacilitator, _restService);
            return _nextContainer as GetWrapper<T>;
        }

        public PostWrapper ThenPost(string url, object route)
        {
            _nextContainer = new PostWrapper(url, route, _baseUri, _restFacilitator, _restService);
            return _nextContainer as PostWrapper;
        }

        public PostWrapper ThenPost(string url)
        {
            _nextContainer = new PostWrapper(url, null, _baseUri, _restFacilitator, _restService);
            return _nextContainer as PostWrapper;
        }

        public PostWrapper ThenPostAndExpect<T>(string url, object route) where T : class, new()
        {
            _nextContainer = new PostWrapper(url, route, _baseUri, _restFacilitator, _restService);
            return _nextContainer as PostWrapper;
        }

        public PostWrapper ThenPostAndExpect<T>(string url) where T : class, new()
        {
            _nextContainer = new PostWrapper(url, null, _baseUri, _restFacilitator, _restService);
            return _nextContainer as PostWrapper;
        }
    }
}
