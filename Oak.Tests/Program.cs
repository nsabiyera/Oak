using System.Reflection;
using NSpec.Domain;
using System;


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
