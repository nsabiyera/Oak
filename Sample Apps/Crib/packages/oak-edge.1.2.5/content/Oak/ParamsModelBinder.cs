using System;
using System.Web.Mvc;
using System.Dynamic;
using System.Collections.Specialized;
using System.IO;
using System.Web;
using System.Diagnostics;
using System.Linq;

namespace Oak
{
    [DebuggerNonUserCode]
    public class DynamicParams : Gemini
    {
        private IValueProvider valueProvider;

        public DynamicParams(NameValueCollection form, IValueProvider valueProvider)
            : base(form)
        {
            this.valueProvider = valueProvider;

            DeleteMember("__RequestVerificationToken");

            Hash()
                .ToList()
                .Where(s => s.Key.ToLower().EndsWith("id"))
                .ForEach(kvp => SetMember(kvp.Key, IntOrOriginal(kvp.Value)));
        }

        private object IntOrOriginal(dynamic value)
        {
            var parsedInt = 0;

            var parsedGuid = Guid.Empty;

            if (int.TryParse(value, out parsedInt)) return parsedInt;

            if (Guid.TryParse(value, out parsedGuid)) return parsedGuid;

            return value;
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

    [DebuggerNonUserCode]
    public class ParamsModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelName == "params") return new DynamicParams(controllerContext.HttpContext.Request.Form, bindingContext.ValueProvider);
                
            return base.BindModel(controllerContext, bindingContext);
        }
    }
}