using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;

namespace Oak.Tests
{
    public class CachedClass : Gemini
    {
        public static int ExecutionCount;

        public Guid Id = Guid.NewGuid();

        static CachedClass()
        {
            Gemini.Extend<CachedClass, Memoize>();
        }

        IEnumerable<dynamic> Memoize()
        {
            yield return (DynamicFunction)SayHello;

            yield return (DynamicFunctionWithParam)SayHelloTo;

            yield return (DynamicFunctionWithParam)SayHelloToValue;
        }

        dynamic SayHello()
        {
            ExecutionCount++;

            return "hello";
        }

        dynamic SayHelloTo(dynamic other)
        {
            ExecutionCount++;

            return other.Name;
        }

        dynamic SayHelloToValue(dynamic value)
        {
            ExecutionCount++;

            return value;
        }
    }

    [Tag("wip")]
    class describe_Memoize : nspec
    {
        dynamic slowClass;

        void before_each()
        {
            slowClass = new CachedClass();
        }

        void specify_method_without_parameters_is_memoized()
        {
            RunBeforeAfterAndCompare(() => slowClass.SayHello());
        }

        void specify_methods_with_parameters_are_memoized()
        {
            RunBeforeAfterAndCompare(() => slowClass.SayHelloTo(Name: "A"));

            RunBeforeAfterAndCompare(() => slowClass.SayHelloTo(Name: "B"));

            RunBeforeAfterAndCompare(() => slowClass.SayHelloTo(new { Name = "C" }));
        }

        void specify_methods_value_type_parameters_are_memoized()
        {
            RunBeforeAfterAndCompare(() => slowClass.SayHelloToValue("A"));
        }

        void specify_method_enumerables_are_memoized()
        {
            string[] list = new[] { "A", "B", "C" };

            RunBeforeAfterAndCompare(() => slowClass.SayHelloToValue(list));
        }

        void specify_memoization_of_value_and_reference_types()
        {
            RunBeforeAfterAndCompare(() => slowClass.SayHelloToValue("A"));

            RunBeforeAfterAndCompare(() => slowClass.SayHelloToValue(new { A = "A" }));
        }

        void specify_memoization_of_nulls()
        {
            RunBeforeAfterAndCompare(() => slowClass.SayHelloToValue(null));
        }

        void RunBeforeAfterAndCompare(Func<dynamic> function)
        {
            var beforeResult = function();

            var afterResult = function();

            CachedClass.ExecutionCount.should_be(1);

            (beforeResult as object).should_be(afterResult as object);

            CachedClass.ExecutionCount = 0;
        }
    }
}
