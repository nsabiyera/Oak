using System.Linq;
using System.Reflection;
using NSpec;
using NSpec.Domain;
using NSpec.Domain.Formatters;
using System.IO;
using System;
using System.Diagnostics;


namespace Oak.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            var ri = new RunnerInvocation(Assembly.GetExecutingAssembly().Location, "", false);
            ri.Run();
            
            Console.WriteLine("Done.");
            Console.ReadLine();

        }
    }
}
