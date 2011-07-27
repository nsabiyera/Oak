using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using System.Dynamic;

namespace Oak.Tests
{
    class describe_Mix : nspec
    {
        dynamic blog;

        dynamic mix;

        dynamic inheritedMix;

        void before_each()
        {
            blog = new ExpandoObject();
            blog.Title = "Some Name";
            mix = new Mix(blog);
        }

        void when_retrieving_property_from_mix()
        {
            it["calls values for mixed entity"] = () =>
                (mix.Title as string).should_be("Some Name");
        }

        void when_setting_properyt_of_mix()
        {
            act = () => mix.Title = "Some other name";

            it["sets property on underlying expando"] = () =>
                (blog.Title as string).should_be("Some other name");
        }

        void inherited_mixed_with_defined_methods()
        {
            act = () => inheritedMix = new InheritedMix(blog);

            it["calls underlying property"] = () =>
                (inheritedMix.Title as string).should_be("Some Name");

            it["sets underlying property"] = () =>
            {
                inheritedMix.Title = "Some other name";
                (blog.Title as string).should_be("Some other name");
            };

            it["calls defined method"] = () =>
                (inheritedMix.FirstLetter() as string).should_be("S");

            it["calls defined property"] = () =>
                (inheritedMix.FirstName as string).should_be("Some");
        }

        void double_inheritance()
        {
            act = () => inheritedMix = new InheritedInheritedMix(blog);

            it["calls methods on root mix with"] = () =>
                (inheritedMix.Title as string).should_be("Some Name");

            it["calls method on first mix"] = () =>
                (inheritedMix.FirstLetter() as string).should_be("S");

            it["calls method on top most mix"] = () =>
                (inheritedMix.LastLetter() as string).should_be("e");
        }

        void given_a_blog()
        {
            before = () =>
            {
                blog = new ExpandoObject();
                blog.Title = "Working With Oak";
                blog.Body = "Oak is tight, yo.";
            };

            context["given that the dynamic blogged is wrapped with a mix"] = () =>
            {
                before = () =>
                    mix = new BlogEntry(blog);

                it["base properties are still accessible"] = () =>
                    (mix.Title as string).should_be("Working With Oak");

                it["base properties are still settable"] = () =>
                {
                    mix.Title = "Another Title";
                    (blog.Title as string).should_be("Another Title");
                };

                it["new properites provided by BlogEntry mix are available"] = () =>
                    ((bool)mix.IsValid()).should_be_true();

                it["properites defined in the mix do override base properties"] = () =>
                    (mix.Body as string).should_be("");
            };

        }
    }

    public class BlogEntry : Mix
    {
        public BlogEntry(object mixWith)
            : base(mixWith)
        {

        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(MixWith.Title);
        }

        public string Body
        {
            get
            {
                return "";
            }
        }
    }

    public class InheritedInheritedMix : InheritedMix
    {
        public InheritedInheritedMix(object mixWith)
            : base(mixWith)
    	{
    				
    	}

        public string LastLetter()
        {
            return (MixWith.Title as string).Last().ToString();
        }
    }

    public class InheritedMix : Mix
    {
        public InheritedMix(object mixWith)
            : base(mixWith)
        {

        }

        public string FirstLetter()
        {
            return (MixWith.Title as string).First().ToString();
        }

        public string FirstName
        {
            get
            {
                return (MixWith.Title as string).Split(' ').First();
            }
        }
    }
}
