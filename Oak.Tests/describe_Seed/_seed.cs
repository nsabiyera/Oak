using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using System.Text.RegularExpressions;
using System.IO;
using System.Configuration;

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

        public bool IsSql()
        {
            return !IsSqlCe();
        }

        public bool IsSqlCe()
        {
            foreach(ConnectionStringSettings connectionString in ConfigurationManager.ConnectionStrings)
            {
                if(connectionString.ProviderName.Contains("SqlServerCe")) return true;
            }

            return false;
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
            using (var reader = "SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}'".With(table).ExecuteReader())
            {
                return reader.Read();
            }
        }

        public void DeleteSqlFiles()
        {
            Directory.GetFiles(Environment.CurrentDirectory, "*.sql").ForEach(s => File.Delete(s));    	
        }

        public IEnumerable<string> Columns(string table)
        {
            using (var reader = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}';".With(table).ExecuteReader())
            {
                while (reader.Read()) yield return reader.GetString(0);
            }
        }

        public string StringWithLength(int length)
        {
            var s = "";

            length.Times(() => s += "X");

            return s;
        }
    }
}
