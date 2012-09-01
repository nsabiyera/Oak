using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oak;
using Oak.Controllers;

namespace BorrowedGames.SchemaGen
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0) throw new InvalidOperationException("first argument should be a connection string.");

            Console.WriteLine("Purging and regenerating schema for " + args[0] + ".");

            var connection = new ConnectionProfile { ConnectionString = args[0] };

            var seed = new Seed(connection);

            var schema = new Schema(seed);

            seed.PurgeDb();

            seed.ExecuteTo(schema.Scripts(), schema.Current());

            Console.WriteLine("Done.");
        }
    }
}