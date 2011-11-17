using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace Sleight
{
    public class Mock : DynamicObject
    {
        string currentMember;

        object[] currentParameters;

        Dictionary<string, object> methodStubs;

        List<dynamic> parameterStubs;

        Dictionary<string, List<Execution>> executionHistory;

        public Mock()
        {
            methodStubs = new Dictionary<string, object>();

            parameterStubs = new List<dynamic>();

            executionHistory = new Dictionary<string, List<Execution>>();
        }

        public Mock WithParameters(params object[] values)
        {
            currentParameters = values;

            return this;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = FindReturnValue(binder.Name, args);

            RecordExecution(binder.Name, args, result, TypeArguments(binder));

            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = FindReturnValue(binder.Name, null);

            RecordExecution(binder.Name, null, result, null);

            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            methodStubs[binder.Name] = value;

            RecordExecution(binder.Name, new object[] { value }, null, null);

            return true;
        }

        object FindReturnValue(string binderName, object[] args)
        {
            var paramResult =
                parameterStubs.FirstOrDefault(s => s.MethodName == binderName 
                    && (s.Parameters as object[]).SequenceEqual(args));

            if (paramResult != null) return paramResult.ReturnValue;

            return methodStubs.ValueOrNull(binderName);
        }

        void RecordExecution(string name, object[] args, object returnValue, Type[] typeArguments)
        {
            if (!executionHistory.ContainsKey(name))  executionHistory[name] = new List<Execution>();

            executionHistory[name].Add(new Execution
            {
                Parameter = (args ?? new object[0]).FirstOrDefault(),
                Parameters = args,
                ReturnValue = returnValue,
                TypeArgument = (typeArguments ?? new Type[0]).FirstOrDefault(),
                TypeArguments = typeArguments
            });
        }

        public Mock Stub(string member)
        {
            currentMember = member;

            return this;
        }

        public Execution ExecutionFor(string member)
        {
            if (executionHistory.ValueOrNull(member) != null) return executionHistory[member].Last();

            return null;
        }

        public void Returns(object value)
        {
            if (IsParameterMode())
            {
                parameterStubs.Add(new
                {
                    MethodName = currentMember,
                    Parameters = currentParameters,
                    ReturnValue = value
                });
            }

            else methodStubs[currentMember] = value;

            ResetMode();
        }

        void ResetMode()
        {
            currentParameters = null;
        }

        bool IsParameterMode()
        {
            return !(currentParameters == null);
        }

        private Type[] TypeArguments(InvokeMemberBinder binder)
        {
            var csharpBinder = binder.GetType().GetInterface("Microsoft.CSharp.RuntimeBinder.ICSharpInvokeOrInvokeMemberBinder");

            return (csharpBinder.GetProperty("TypeArguments").GetValue(binder, null) as IList<Type>).ToArray();
        }

        public IEnumerable<Execution> ExecutionsFor(string sayhello)
        {
            return executionHistory.ValueOrNull(sayhello);
        }
    }
}