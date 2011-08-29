using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak;
using DynamicBlog.Models;
using Massive;
using Oak.Models;
using System.Web.Mvc;

namespace DynamicBlog.Tests.Models
{
    public class Person : Mix
    {
        public Person()
            : base(new {})
        {
            MixWith.Name = null as string;
        }
    }

    class describe_DynamicForm : nspec
    {
        MvcHtmlString markup;
        dynamic form;
        dynamic person;

        void rendering_markup_for_textbox()
        {
            before = () =>
            {
                person = new Person();
                form = new DynamicForm(person);
                person.Name = "Jane Doe";
            };

            act = () => markup = form.Name_TextBox;

            it["renders a textbox"] = () =>
            {
                markup.should_be("<input id=\"Name\" name=\"Name\" />");
            };
        }
    }
}
