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
    [DebuggerNonUserCode]
    public class DynamicForm : DynamicObject
    {
        dynamic entity;

        public DynamicForm(dynamic entity)
        {
            this.entity = entity;
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
    }
}