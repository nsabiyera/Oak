using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Dynamic;
using Massive;
using System.Web.Mvc;
using System.Reflection;
using System.Diagnostics;

namespace Oak
{
    public class Hash : Dictionary<string, string>
    {
        
    }

    public class DynamicForm : DynamicObject
    {
        dynamic entity;

        List<string> InputAttributes;

        public DynamicForm(dynamic entity)
        {
            this.entity = entity;
            InputAttributes = InputeAttributes();
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var propertyName = binder.Name;

            var metaData = new ElementMetaData
            {
                Id = binder.Name,
                Value = PropertyValue(binder.Name)
            };

            result = metaData;
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var propertyName = binder.Name;

            var metaData = new ElementMetaData
            {
                Id = binder.Name
            };

            if(args.Count() == 1)
            {
                var elements = args[0] as IDictionary<string, string>;

                foreach(var kvp in elements)
                {
                    if (InputAttributes.Contains(kvp.Key)) //direct html input attribute
                    {
                        metaData.Attributes.Add(kvp.Key, kvp.Value.ToString());
                    }
                    else
                    {
                        metaData.Styles.Add(kvp.Key, kvp.Value.ToString());
                    }
                }
            }

            result = metaData;
            return true;//do some checking
        }

        private object PropertyValue(string name)
        {
            var property = GetProperty(name);

            if (property != null) return property.GetValue(entity as object, null);

            if (entity is Mix)
            {
                var underlyingValues = entity.MixWith as IDictionary<string, object>;

                if (!underlyingValues.ContainsKey(name)) throw new InvalidOperationException("The Mix that you passed into DynamicForm does not contain the property called " + name + ".");

                return underlyingValues[name];
            }

            throw new InvalidOperationException("The entity that you passed into DynamicForm does not contain the property called " + name + ".");
        }

        private PropertyInfo GetProperty(string name)
        {
            return (entity as object).GetType().GetProperty(name);
        }

        public List<string> InputeAttributes()
        {
            var list = new List<string>();

            list.Add("accept");
            list.Add("align");
            list.Add("alt");
            list.Add("checked");
            list.Add("disabled");
            list.Add("maxlength");
            list.Add("name");
            list.Add("readonly");
            list.Add("size");
            list.Add("src");
            list.Add("type");
            
            list.Add("accesskey");
            list.Add("class");
            list.Add("dir");
            list.Add("id");
            list.Add("lang");
            list.Add("style");
            list.Add("tabindex");
            list.Add("title");

            list.Add("onblur");
            list.Add("onchange");
            list.Add("onclick");
            list.Add("ondbclick");
            list.Add("onfocus");
            list.Add("onmousedown");
            list.Add("onmousemove");
            list.Add("onmouseout");
            list.Add("onmouseover");
            list.Add("onmouseup");
            list.Add("onkeydown");
            list.Add("onkeypress");
            list.Add("onkeyup");
            list.Add("onselect");

            return list;
        }
    }
}