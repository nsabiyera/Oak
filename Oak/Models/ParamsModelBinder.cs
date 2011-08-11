using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Dynamic;

namespace Oak.Models
{
    public class DynamicParams : DynamicObject
    {
        IValueProvider valueProvider;
        public DynamicParams(IValueProvider valueProvider)
        {
            this.valueProvider = valueProvider;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var value = valueProvider.GetValue(binder.Name);

            if(value == null)
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
            if(bindingContext.ModelName == "form")
            {
                dynamic expando = new ExpandoObject();
                var dict = expando as IDictionary<string, object>;
                var form = new FormCollection(controllerContext.HttpContext.Request.Form);
                foreach (string item in form)
                {
                    dict.Add(item, form[item]);
                }

                return expando;
            }

            if (bindingContext.ModelName == "params") return new DynamicParams(bindingContext.ValueProvider);
                
            return base.BindModel(controllerContext, bindingContext);
        }
    }
}