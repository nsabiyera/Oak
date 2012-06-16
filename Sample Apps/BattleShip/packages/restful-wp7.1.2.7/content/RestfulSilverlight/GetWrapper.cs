using System;

namespace RestfulSilverlight
{
    public class GetWrapper<T> : RestWrapper where T : class, new()
    {
        public GetWrapper(string url, object route, string baseUri, RestFacilitator restFacilitator, RestService restService)
            : base(url, route, baseUri, restFacilitator, restService)
        {

        }

        private object _callback;

        public GetWrapper<T> WhenFinished(Action<T> action)
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
            return this;
        }

        private object _failureCallback;

        public GetWrapper<T> IfFailure(Action<Exception> action)
        {
            CompleteCallback<Exception> callback =
            (t) =>
            {
                action(t);
            };

            _failureCallback = callback;
            return this;
        }

        private Func<object> _deferredRouteRetrieval;

        public GetWrapper<T> ForRoute(Func<object> deferredRouteRetrieval)
        {
            _deferredRouteRetrieval = deferredRouteRetrieval;
            return this;
        }

        public void Go()
        {
            if (_deferredRouteRetrieval != null)
            {
                _route = _deferredRouteRetrieval();
            }

            _restService.Get<T>(_url, _route, _callback as CompleteCallback<T>, _failureCallback as CompleteCallback<Exception>);
        }
    }
}
