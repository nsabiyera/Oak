using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using System.Text.RegularExpressions;

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


        protected void CommandShouldBe(string expected)
        {
            command.should_be(ToSingleLine(expected));
        }

        protected string ToSingleLine(string s)
        {
            var single = Regex.Replace(s, @"[ ]{2,}", "");

            single = single.Trim().Replace(Environment.NewLine, "");

            return single;
        }
    }
}
