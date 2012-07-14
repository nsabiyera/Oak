using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using System.Text.RegularExpressions;
using System.IO;

namespace Oak.Tests.describe_Seed
{
    class _seed : nspec
    {
        protected Seed seed;
        protected string command;
        protected dynamic[] columns;
        ConnectionProfile connectionProfile;

        void before_each()
        {
            connectionProfile = new ConnectionProfile { ConnectionString = "" };

            seed = new Seed(connectionProfile);
        }


        public void CommandShouldBe(string expected)
        {
            command.should_be(ToSingleLine(expected));
        }

        public string ToSingleLine(string s)
        {
            var single = Regex.Replace(s, @"[ ]{2,}", "");

            single = single.Trim().Replace(Environment.NewLine, "");

            return single;
        }

        public bool TableExists(string table)
        {
            return "select * from sysobjects where name = '{0}'".With(table).ExecuteReader().HasRows;
        }

        public void DeleteSqlFiles()
        {
            Directory.GetFiles(Environment.CurrentDirectory, "*.sql").ForEach(s => File.Delete(s));    	
        }

        public IEnumerable<string> Columns(string table)
        {
            var reader = "select name from syscolumns where object_name(id) = '{0}';".With(table).ExecuteReader();

            while (reader.Read()) yield return reader.GetString(0);
        }

        public string StringWithLength(int length)
        {
            var s = "";

            length.Times(() => s += "X");

            return s;
        }
    }
}
