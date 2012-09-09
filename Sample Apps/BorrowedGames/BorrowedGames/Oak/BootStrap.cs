using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Oak
{
    public static class BootStrap
    {
        public static void Init()
        {
            ModelBinders.Binders.Add(new KeyValuePair<Type, IModelBinder>(typeof(object), new ParamsModelBinder()));

            if (IsInDebugMode())
            {
                Massive.DynamicRepository.WriteDevLog = true;
                var currentLog = Massive.DynamicRepository.LogSql;
                Massive.DynamicRepository.LogSql = (sender, sql, args) =>
                {
                    lock (Massive.DynamicRepository.ConsoleLogLock)
                    {
                        BulletActionFilter.sqlQueries.Add(
                            new Tuple<string, string, int>(
                                sql,
                                Environment.StackTrace,
                                System.Threading.Thread.CurrentThread.ManagedThreadId));
                    }

                    currentLog(sender, sql, args);
                };

                GlobalFilters.Filters.Add(new BulletActionFilter());
            }
        }

        public static bool IsInDebugMode()
        {
            return System.Diagnostics.Process.GetCurrentProcess().ProcessName == "iisexpress";
        }
    }

    public class BulletActionFilter : ActionFilterAttribute
    {
        public static List<Tuple<string, string, int>> sqlQueries = new List<Tuple<string, string, int>>();

        public BulletActionFilter()
        {
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            var queries = Bullet.InefficientQueries(sqlQueries);

            lock (Massive.DynamicRepository.ConsoleLogLock)
            {
                queries.ForEach(s =>
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Out.WriteLine("==== Possible Inefficient Query ====");
                    Console.Out.WriteLine("For request: " + filterContext.HttpContext.Request.Url);
                    Console.Out.WriteLine("For thread: " + s.ThreadId);
                    Console.Out.WriteLine("\n");
                    Console.Out.WriteLine(s.Query);
                    Console.Out.WriteLine("\n");
                    Console.Out.WriteLine(s.Reason);
                    Console.Out.WriteLine("\n");
                    Console.Out.WriteLine(s.StackTrace);
                    Console.Out.WriteLine("\n");
                    Console.Out.WriteLine("====================================\n\n");
                    Console.ResetColor();
                });

                sqlQueries.Clear();
            }
        }
    }
}
