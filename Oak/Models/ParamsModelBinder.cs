using System;
using System.Web.Mvc;
using System.Dynamic;
using System.Collections.Specialized;
using System.IO;
using System.Web;

namespace Oak.Models
{
    public class DynamicParams : Mix
    {
        private IValueProvider valueProvider;

        public DynamicParams(NameValueCollection form, IValueProvider valueProvider)
            : base(form)
        {
            this.valueProvider = valueProvider;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var found = base.TryGetMember(binder, out result);

            if (found) return true;

            var value = valueProvider.GetValue(binder.Name);

            if (value == null)
            {
                result = null;
                return true;
            }

            result = (object)value.AttemptedValue;

            return true;
        }
    }

    public class ParamsModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelName == "params") return new DynamicParams(controllerContext.HttpContext.Request.Form, bindingContext.ValueProvider);
                
            return base.BindModel(controllerContext, bindingContext);
        }
    }
}