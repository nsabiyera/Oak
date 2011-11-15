using System;
using NUnit.Framework;
using NSpec.Domain;
using System.Reflection;
using NSpec;
using NSpec.Domain.Formatters;

namespace DynamicBlog.Tests
{
    [TestFixture]
    public class DebuggerShim
    {
        [Test]
        public void debug()
        {
            var testClassYouWantToDebug = "select_many";

            var finder = new SpecFinder(
                Assembly.GetExecutingAssembly().Location,
                new Reflector(),
                testClassYouWantToDebug);

            var builder = new ContextBuilder(
                finder,
                new DefaultConventions());

            new ContextRunner(builder, new ConsoleFormatter()).Run();
        }
    }
}
