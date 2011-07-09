using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Practices.Unity;
using System.Web.Routing;
using System.Web.SessionState;

namespace DynamicBlog
{
    public class UnityControllerFactory : IControllerFactory
    {
        private IUnityContainer _container;
        private IControllerFactory _innerFactory;
        private List<string> _controllerRegistry;

        public UnityControllerFactory(IUnityContainer container)
            : this(container, new DefaultControllerFactory())
        {

        }

        protected UnityControllerFactory(IUnityContainer container,
            IControllerFactory innerFactory)
        {
            _container = container;
            _innerFactory = innerFactory;
            _controllerRegistry = new List<string>();
        }

        public IController CreateController(RequestContext requestContext,
            string controllerName)
        {
            if (_controllerRegistry.Contains(controllerName.ToLowerInvariant()))
            {
                return _container
                    .Resolve<IController>(controllerName.ToLowerInvariant());
            }

            return _innerFactory.CreateController(requestContext, controllerName);
        }

        public void ReleaseController(IController controller)
        {
            _container.Teardown(controller);
        }

        public SessionStateBehavior GetControllerSessionBehavior(
            RequestContext requestContext, string controllerName)
        {
            return _innerFactory.GetControllerSessionBehavior(
                requestContext,
                controllerName);
        }

        public void RegisterController(string name, Type type)
        {
            var scrubbedName = name.Replace("Controller", "").ToLowerInvariant();

            _controllerRegistry.Add(scrubbedName);
            _container.RegisterType(typeof(IController), type, scrubbedName);
        }
    }
}