using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using System.Dynamic;
using System.Reflection;

namespace Oak.Tests.describe_Gemini.Classes
{
    public class InheritedPrivateGemini : PrivateGemini
    {
        
    }

    public class DoubleInheritedPrivateGemini : InheritedPrivateGemini
    {
        
    }

    public class PrivateGemini : Gemini
    {
        public bool Altered;

        dynamic HelloString()
        {
            return "hello";
        }

        dynamic HelloException()
        {
            throw new InvalidOperationException();
        }

        dynamic Hello(dynamic name)
        {
            return "hello " + name;
        }

        dynamic HelloException(dynamic name)
        {
            throw new InvalidOperationException();
        }

        dynamic HelloFullName(dynamic fullName)
        {
            return "hello " + fullName.firstName + " " + fullName.lastName;
        }

        void Alter()
        {
            Altered = true;
        }

        void AlterException()
        {
            throw new InvalidOperationException();   
        }

        void SetAltered(dynamic value)
        {
            Altered = value;
        }

        void SetAlteredException(dynamic value)
        {
            throw new InvalidOperationException();
        }

        IEnumerable<dynamic> Names()
        {
            return new[] { "name1", "name2" };
        }

        IEnumerable<dynamic> NamesWithPrefix(dynamic prefix)
        {
            return Names().Select(s => prefix + s);
        }

        IEnumerable<dynamic> NamesWithArgs(dynamic args)
        {
            return Names().Select(s => args.prefix + s);
        }
    }
}
