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
    public static class StreamHelper
    {
        public static object JsonToGemini(this Stream s)
        {
            return JsonToDynamic.Parse(new StreamReader(s.FromBeginning()).ReadToEnd());
        }
    }

    public class DynamicParams : Gemini
    {
        private IValueProvider valueProvider;

        public DynamicParams(object dto, IValueProvider valueProvider)
            : base(dto)
        {
            this.valueProvider = valueProvider;

            DeleteMember("__RequestVerificationToken");

            Hash()
                .ToList()
                .Where(s => s.Key.ToLower().EndsWith("id"))
                .ForEach(kvp => SetMember(kvp.Key, IntOrOriginal(kvp.Value)));

            Hash()
                .ToList()
                .Where(s => s.Value != null && s.Value.Equals("null"))
                .ForEach(kvp => SetMember(kvp.Key, null));
        }

        public DynamicParams(Stream body, IValueProvider valueProvider)
            : this(body.JsonToGemini() as object, valueProvider)
        {

        }

        private object IntOrOriginal(dynamic value)
        {
            if (!(value is string)) return value;

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

    public class ParamsModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelName != "params") return base.BindModel(controllerContext, bindingContext);

            if (HasJsonBody(controllerContext)) return new DynamicParams(controllerContext.HttpContext.Request.InputStream, bindingContext.ValueProvider);

            return new DynamicParams(controllerContext.HttpContext.Request.Form, bindingContext.ValueProvider);
        }

        public bool HasJsonBody(ControllerContext controllerContext)
        {
            return controllerContext.HttpContext.Request.ContentType.Contains("application/json");
        }
    }
}