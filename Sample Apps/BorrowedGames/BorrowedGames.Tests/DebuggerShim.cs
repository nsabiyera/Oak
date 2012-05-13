using System;
using System.Collections.Generic;
using NSpec.Domain;
using System.Reflection;
using NSpec;
using NSpec.Domain.Formatters;
using System.Linq;

public class DebuggerShim
{
    public void debug()
    {
        //the specification class you want to test
        //this can be a regular expression
        var testClassYouWantToDebug = "wip";

        //initialize NSpec's specfinder
        var finder = new SpecFinder(
            Assembly.GetExecutingAssembly().Location,
            new Reflector(),
            "");

        //initialize NSpec's builder
        var tags = new Tags().Parse(testClassYouWantToDebug);

        var builder = new ContextBuilder(
            finder, tags,
            new DefaultConventions());

        //initialize the root context
        var contexts = builder.Contexts();

        //build the tests
        contexts.Build();

        //run the tests that were found
        contexts.Run();

        //contexts.Count.Is(1);

        //print the output
        new ConsoleFormatter().Write(contexts);

        //assert that there aren't any failures
        contexts.Failures().Count().should_be(0);
    }
}