using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Massive;

namespace Oak
{
    public class DynamicDb : Gemini
    {
        public void LogToConsole()
        {
            DynamicRepository.WriteDevLog = true;
            DynamicRepository.LogSql = (o, sql, args) =>
            {
                Console.WriteLine(sql);
            };
        }

        dynamic MethodMissing(dynamic callInfo)
        {
            var repoName = callInfo.Name;

            var repositoryProperty = repoName + "Repository";

            callInfo.Instance.SetMember(repositoryProperty, AssociationByConventions.RepositoryFor(repoName));

            callInfo.Instance.SetMember(
                repoName,
                new DynamicFunction(() => callInfo.Instance.GetMember(repositoryProperty)));

            return callInfo.Instance.GetMember(repoName)();
        }
    }
}