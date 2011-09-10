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

        public DynamicForm(dynamic entity)
        {
            this.entity = entity;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = NewElementMetatDataFor(binder.Name);

            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var metaData = NewElementMetatDataFor(binder.Name);

            result = metaData;

            if (args.Count() == 0) return true;

            var elements = args[0] as IDictionary<string, string>;

            List<string> elementAttributes = ElementAttributes();

            foreach (var kvp in elements)
            {
                if (kvp.Key == "id") metaData.Id = kvp.Value;

                else if (kvp.Key == "value" && metaData.Value == null) metaData.Value = kvp.Value;

                else if (IsAttribute(elementAttributes, kvp)) metaData.Attributes.Add(kvp.Key, kvp.Value.ToString());

                else metaData.Styles.Add(kvp.Key, kvp.Value.ToString());
            }

            return true;
        }

        private static bool IsAttribute(List<string> elementAttributes, KeyValuePair<string, string> kvp)
        {
            return elementAttributes.Contains(kvp.Key) || kvp.Key.StartsWith("data-");
        }

        private ElementMetaData NewElementMetatDataFor(string name)
        {
            return new ElementMetaData
            {
                Id = name,
                Value = PropertyValue(name)
            };
        }

        private object PropertyValue(string name)
        {   
            var property = GetProperty(name);

            if (property != null) return property.GetValue(entity as object, null);

            if (entity is DynamicModel && (entity as DynamicModel).RespondsTo(name)) return (entity as DynamicModel).GetValueFor(name);

            if (entity is Prototype)
            {
                var underlyingValues = entity.Expando as IDictionary<string, object>;

                if (!underlyingValues.ContainsKey(name)) throw new InvalidOperationException("The Prototype that you passed into DynamicForm does not contain the property called " + name + ".");

                return underlyingValues[name];
            }

            throw new InvalidOperationException("The entity that you passed into DynamicForm does not contain the property called " + name + ".");
        }

        private PropertyInfo GetProperty(string name)
        {
            return (entity as object).GetType().GetProperty(name);
        }

        public List<string> ElementAttributes()
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
            list.Add("placeholder");

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