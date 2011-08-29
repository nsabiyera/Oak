using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Dynamic;
using Massive;
using System.Web.Mvc;
using Oak.Models;

namespace DynamicBlog.Models
{
    public class DynamicForm : DynamicObject
    {
        dynamic _entity;

        public DynamicForm(dynamic entity)
        {
            _entity = entity;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var propertyAttempted = binder.Name;
            var propertyName = propertyAttempted.Split('_').First();
            var formType = propertyAttempted.Split('_').Last();

            //object value;
            //_entity.TryGetMember(propertyName, out value);

            if (formType == "TextBox")
            {
                var tb = new TagBuilder("input");
                tb.Attributes.Add("id", propertyName);
                tb.Attributes.Add("name", propertyName);

                result = MvcHtmlString.Create(tb.ToString(TagRenderMode.SelfClosing));d

                return true;
            }

            if (formType == "TextArea")
            {
                var tb = new TagBuilder("textarea");
                tb.Attributes.Add("id", propertyName);
                tb.Attributes.Add("name", propertyName);

                result = MvcHtmlString.Create(tb.ToString());

                return true;
            }

            result = null;
            return false;
        }
    }
}