using System;

namespace RestfulSilverlight
{
    public class AsyncDelegation
    {
        public string BaseUri { get; set; }

        private RestFacilitator _restFacilitator;
        private object _restServiceContainer;
        private RestService _restService;

        public AsyncDelegation(RestService restService)
        {
            _restFacilitator = restService.RestFacilitator;
            _restService = restService;
            BaseUri = restService.BaseUri;
        }

        public GetWrapper<T> Get<T>(string url, object route) where T : class, new()
        {
            _restServiceContainer = new GetWrapper<T>(url, route, BaseUri, _restFacilitator, _restService);
            return _restServiceContainer as GetWrapper<T>;
        }

        public GetWrapper<T> Get<T>(string url) where T : class, new()
        {
            _restServiceContainer = new GetWrapper<T>(url, null, BaseUri, _restFacilitator, _restService);
            return _restServiceContainer as GetWrapper<T>;
        }

        public PostWrapper Post(string url, object route)
        {
            _restServiceContainer = new PostWrapper(url, route, BaseUri, _restFacilitator, _restService);
            return _restServiceContainer as PostWrapper;
        }

        public PostWrapper Post(string url)
        {
            _restServiceContainer = new PostWrapper(url, null, BaseUri, _restFacilitator, _restService);
            return _restServiceContainer as PostWrapper;
        }

        public void Go()
        {
            if (_restServiceContainer != null)
            {
                _restServiceContainer.GetType().GetMethod("Go").Invoke(_restServiceContainer, null);
            }
        }
    }
}
