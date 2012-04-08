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
            _.LastMethodMissing = args;

            _.Instance = args.Instance;

            _.Parameter = args.Parameter;

            _.Parameters = args.Parameters;

            _.ParameterNames = args.ParameterNames;

            return null;
        }
    }
}
