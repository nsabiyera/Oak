using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Dynamic;

namespace Oak.Models
{
    public class DynamicValueProvider : DynamicObject
    {
        IValueProvider valueProvider;
        public DynamicValueProvider(IValueProvider valueProvider)
        {
            this.valueProvider = valueProvider;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = (object)valueProvider.GetValue(binder.Name).AttemptedValue;
            return true;
        }
    }

    public class DynamicModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelName != "params")
                return base.BindModel(controllerContext, bindingContext);

            return new DynamicValueProvider(bindingContext.ValueProvider);
        }
    }
}