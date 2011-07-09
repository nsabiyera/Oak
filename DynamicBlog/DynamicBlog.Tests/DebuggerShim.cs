using System;
using NUnit.Framework;
using NSpec.Domain;
using System.Reflection;
using NSpec;

namespace DynamicBlog.Tests
{
    [TestFixture]
    public class DebuggerShim
    {
        [Test]
        public void debug()
        {
            var testClassYouWantToDebug = "";

            var finder = new SpecFinder(
                Assembly.GetExecutingAssembly().Location,
                new Reflector(),
                testClassYouWantToDebug);

            var builder = new ContextBuilder(
                finder,
                new DefaultConventions());

            new ContextRunner(builder).Run();
        }
    }
}
