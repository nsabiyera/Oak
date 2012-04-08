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
    class gemini_methods : _describe_Gemini
    {
        void calling_dynamically_defined_methods()
        {
            context["method is defined as a dynamic function that takes in one dynamic parameter"] = () =>
            {
                act = () =>
                {
                    gemini = new ParameterlessGemini();
                    gemini.CreateNewGemini = new DynamicFunctionWithParam(d => new Gemini(d));
                };

                it["calls method with parameter specified"] = () =>
                {
                    dynamic newGemini = gemini.CreateNewGemini(new { FirstName = "Amir" });

                    (newGemini.FirstName as string).should_be("Amir");
                };

                it["calls method with even if parameter is not specified"] = () =>
                {
                    dynamic newGemini = gemini.CreateNewGemini();

                    (newGemini as object).should_not_be_null();
                };

                it["calls method with parameters specified as named parameters"] = () =>
                {
                    dynamic newGemini = gemini.CreateNewGemini(FirstName: "Amir");

                    (newGemini.FirstName as string).should_be("Amir");
                };

                it["ignores unnamed parameters if name parameters have been specified"] = () =>
                {
                    dynamic newGemini = gemini.CreateNewGemini("Unnamed", FirstName: "Amir", LastName: "Rajan");

                    (newGemini.FirstName as string).should_be("Amir");

                    (newGemini.LastName as string).should_be("Rajan");
                };
            };

            context["method is defined as a dynamic method that takes in one dynamic parameter"] = () =>
            {
                act = () =>
                {
                    gemini = new ParameterlessGemini();
                    gemini.AlterGemini = new DynamicMethodWithParam(d => gemini.Property = d ?? "Default");
                };

                it["calls method with parameter specified"] = () =>
                {
                    gemini.AlterGemini("Other");

                    (gemini.Property as string).should_be("Other");
                };

                it["calls method with even if parameter is not specified"] = () =>
                {
                    gemini.AlterGemini();

                    (gemini.Property as string).should_be("Default");
                };
            };
        }

        void method_aliasing()
        {
            it["methods can be aliased"] = () =>
            {
                dynamic aliasGemini = new AliasGemini();

                (aliasGemini.SayHello() as string).should_be(aliasGemini.Hello() as string);
            };
        }

        void method_missing()
        {
            it["method missing is called if method doesn't exist but method missing is defined"] = () =>
            {
                dynamic methodMissingGemini = new MethodMissingGemini();

                methodMissingGemini.ThisIsAMissingMethod(parameter1: "Test", parameter2: "Test2");

                (methodMissingGemini.ParameterNames[0] as string).should_be("parameter1");

                (methodMissingGemini.Parameters[0] as string).should_be("Test");

                (methodMissingGemini.ParameterNames[1] as string).should_be("parameter2");

                (methodMissingGemini.Parameters[1] as string).should_be("Test2");
            };

            it["method missing has accessed to the instance of gemini that it was invoked on"] = () =>
            {
                dynamic methodMissingGemini = new MethodMissingGemini();

                methodMissingGemini.ThisIsAMissingMethod(parameter1: "Test", parameter2: "Test2");

                (methodMissingGemini.Instance as object).should_be(methodMissingGemini as object);
            };

            it["parameter values can be accessed via ParameterMethod of the object passed into method missing"] = () =>
            {
                dynamic methodMissingGemini = new MethodMissingGemini();

                methodMissingGemini.ThisIsAMissingMethod("UnNamed1", "UnNamed2", parameter1: "Test", parameter2: "Test2");

                (methodMissingGemini.Parameter.parameter1 as string).should_be("Test");

                (methodMissingGemini.Parameter.parameter2 as string).should_be("Test2");

                (methodMissingGemini.Parameters[0] as string).should_be("UnNamed1");
            };
        }

    }
}
