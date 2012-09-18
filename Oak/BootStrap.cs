using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading;

namespace Oak
{
    public static class BootStrap
    {
        public static void Init()
        {
            ModelBinders.Binders.Add(new KeyValuePair<Type, IModelBinder>(typeof(object), new ParamsModelBinder()));

            if (DebugBootStrap.IsInDebugMode() && !DebugBootStrap.IsInitialized)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Howdy! It looks like you've upgraded from an older version of Oak.  Add the following line to your Global.asax.cs to get AWESOME dev logs in your IISExpress window: ");
                Console.WriteLine(@"
//inside of Global.asax.cs
//constructor for your mvc application
public MvcApplication()
{
    DebugBootStrap.Init(this);
}
");
                Console.ResetColor();
            }
        }
    }

    public static class DebugBootStrap
    {
        [ThreadStatic]
        public static List<SqlQueryLog> SqlQueries = new List<SqlQueryLog>();

        public static bool IsInitialized { get; set; }

        static DebugBootStrap()
        {
            IsInitialized = false;
        }

        public static void Init(HttpApplication mvcApplication)
        {
            IsInitialized = true;

            mvcApplication.EndRequest += new EventHandler(mvcApplication_EndRequest);

            if (IsInDebugMode())
            {
                Massive.DynamicRepository.WriteDevLog = true;

                Massive.DynamicRepository.LogSql = (sender, query, args) =>
                {
                    lock (Massive.DynamicRepository.ConsoleLogLock)
                    {
                        if (SqlQueries == null) SqlQueries = new List<SqlQueryLog>();

                        SqlQueries.Add(
                            new SqlQueryLog(sender,
                                query,
                                Environment.StackTrace,
                                Thread.CurrentThread.ManagedThreadId,
                                args));
                    }
                };
            }

            mvcApplication.Error += (sender, e) =>
            {
                lock (Massive.DynamicRepository.ConsoleLogLock)
                {
                    var error = mvcApplication.Server.GetLastError().ToString();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Out.WriteLine("======== Exception Occurred ==========");
                    Console.Out.WriteLine(Bullet.ScrubStackTrace(error));
                    Console.Out.WriteLine("====================================\n\n");
                    Console.ResetColor();
                }
            };
        }

        static void mvcApplication_EndRequest(object sender, EventArgs e)
        {
            SqlQueries = SqlQueries ?? new List<SqlQueryLog>();

            lock (Massive.DynamicRepository.ConsoleLogLock)
            {
                if (SqlQueries.Any())
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Out.WriteLine("======== Queries executed ==========");

                    SqlQueries.ForEach(s =>
                    {
                        Massive.DynamicRepository.LogSqlDelegate(s.Sender, s.Query, s.Args);
                    });

                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Out.WriteLine("====================================\n\n");
                    Console.ResetColor();
                }

                var queries = Bullet.InefficientQueries(SqlQueries);

                queries.ForEach(s =>
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Out.WriteLine("==== Possible Inefficient Query ====");
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

                SqlQueries.Clear();
            }
        }

        public static bool IsInDebugMode()
        {
            return System.Diagnostics.Process.GetCurrentProcess().ProcessName == "iisexpress";
        }
    }
}
