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

            foreach (var kvp in elements) metaData.Set(kvp.Key, kvp.Value);

            return true;
        }

        private ElementMetaData NewElementMetatDataFor(string name)
        {
            var element = new ElementMetaData();

            element.Set("id", name);

            var value = PropertyValue(name);

            element.Set("value", value == null ? null : value.ToString());

            return element;
        }

        private object PropertyValue(string name)
        {
            var property = GetProperty(name);

            if (property != null) return property.GetValue(entity as object, null);

            if (entity is DynamicModel && (entity as DynamicModel).RespondsTo(name)) return (entity as DynamicModel).GetMember(name);

            if (entity is Gemini)
            {
                var underlyingValues = entity.Expando as IDictionary<string, object>;

                if (!underlyingValues.ContainsKey(name)) throw new InvalidOperationException("The Gemini that you passed into DynamicForm does not contain the property called " + name + ".");

                return underlyingValues[name];
            }

            throw new InvalidOperationException("The entity that you passed into DynamicForm does not contain the property called " + name + ".");
        }

        private PropertyInfo GetProperty(string name)
        {
            return (entity as object).GetType().GetProperty(name);
        }
    }
}