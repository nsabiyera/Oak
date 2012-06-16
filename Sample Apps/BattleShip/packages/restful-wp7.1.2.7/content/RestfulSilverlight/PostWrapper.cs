using System;

namespace RestfulSilverlight
{
    public class PostWrapper : RestWrapper
    {
        public PostWrapper(string url, object route, string baseUri, RestFacilitator restFacilitator, RestService restService)
            : base(url, route, baseUri, restFacilitator, restService)
        {

        }

        private object _callback;

        public PostWrapper IgnoreResponse()
        {
            return WhenFinished(() => { });
        }

        public PostWrapper WhenFinished(Action action)
        {
            CompleteCallback callback =
               () =>
               {
                   action();
                   if (_nextContainer != null)
                   {
                       _nextContainer.GetType().GetMethod("Go").Invoke(_nextContainer, null);
                   }
               };

            _callback = callback;

            _post = () =>
            {
                _restService.Post(_url, _route, _callback as CompleteCallback);
            };

            return this;
        }

        public PostWrapper WhenFinished<T>(Action<T> action) where T : class, new()
        {
            CompleteCallback<T> callback =
                (t) =>
                {
                    action(t);
                    if (_nextContainer != null)
                    {
                        _nextContainer.GetType().GetMethod("Go").Invoke(_nextContainer, null);
                    }
                };

            _callback = callback;

            _post = () =>
            {
                _restService.Post<T>(_url, _route, _callback as CompleteCallback<T>);
            };

            return this;
        }

        private Action _post;

        public void Go()
        {
            _post();
        }
    }
}
