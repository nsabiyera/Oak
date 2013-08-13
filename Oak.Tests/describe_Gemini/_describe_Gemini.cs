using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using System.Dynamic;
using System.Reflection;
using Oak.Tests.describe_Gemini.Classes;

namespace Oak.Tests.describe_Gemini
{
    [Tag("Gemini")]
    class _describe_Gemini : nspec
    {   
        public dynamic blog;

        public dynamic gemini;

        void before_each()
        {
            blog = new Prototype();
            blog.Title = "Some Name";
            blog.body = "Some Body";
            blog.BodySummary = "Body Summary";
            gemini = new Gemini(blog);
        }
        
        public Gemini Gemini()
        {
            return gemini as Gemini;
        }
    }
}