using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using System.Dynamic;
using System.Reflection;

namespace Oak.Tests.describe_Gemini.Classes
{
    public partial class MethodMissingGemini : Gemini
    {
        public MethodMissingGemini()
        {

        }

        dynamic MethodMissing(dynamic args)
        {
            return args.Name + " " +
                args.ParameterNames[0] + ": " +
                args.Parameters[0] + " " +
                args.ParameterNames[1] + ": " +
                args.Parameters[1];
        }
    }
}
