using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading;
using System.Data.SqlClient;
using System.Configuration;
using System.Reflection;
using System.IO;

namespace Oak
{
    public static class BootStrap
    {
        public static void Init()
        {
            ModelBinders.Binders.Add(new KeyValuePair<Type, IModelBinder>(typeof(object), new ParamsModelBinder()));
        }
    }

    public class ConsoleOutExceptionFilterAttribute : FilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            lock (Massive.DynamicRepository.ConsoleLogLock)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Out.WriteLine("======== Exception Occurred ==========");
                Console.Out.WriteLine(Bullet.ScrubStackTrace(filterContext.Exception.ToString()));
                Console.Out.WriteLine("====================================\n\n");
                Console.ResetColor();
            }
        }
    }

    public class ConsoleOutPayloadFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            lock (Massive.DynamicRepository.ConsoleLogLock)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;

                if (HasPayload(filterContext))
                {
                    Console.Out.WriteLine("\n============ Payload ==========");
                    Console.Out.WriteLine("For thread: " + Thread.CurrentThread.ManagedThreadId + "\n");
                }

                if (HasQueryString(filterContext))
                {
                    Console.Out.WriteLine("Query String:");
                    var qs = filterContext.HttpContext.Request.QueryString;
                    string formattedQueryStrings = string.Join("\n", qs.AllKeys.Select(s => s + ": " + qs[s]));
                    Console.Out.WriteLine(formattedQueryStrings + "\n");
                }

                if (HasForm(filterContext))
                {
                    Console.Out.WriteLine("Content:");
                    Console.Out.WriteLine(new DynamicParams(filterContext.HttpContext.Request.Form, null) + "\n");
                }

                if (HasJson(filterContext))
                {
                    Console.Out.WriteLine("Content:");
                    Console.Out.WriteLine(new DynamicParams(filterContext.HttpContext.Request.InputStream, null) + "\n");
                }

                if (HasPayload(filterContext)) Console.Out.WriteLine("================================");

                Console.ResetColor();
            }
        }

        bool HasQueryString(ActionExecutingContext filterContext)
        {
            return filterContext.HttpContext.Request.QueryString != null && filterContext.HttpContext.Request.QueryString.Count > 0;
        }

        bool HasForm(ActionExecutingContext filterContext)
        {
            return filterContext.HttpContext.Request.Form != null && filterContext.HttpContext.Request.Form.Count > 0;
        }

        bool HasJson(ActionExecutingContext filterContext)
        {
            return filterContext.HttpContext.Request.ContentType == "application/json";
        }

        bool HasPayload(ActionExecutingContext filterContext)
        {
            return HasForm(filterContext) || HasQueryString(filterContext) || HasJson(filterContext);
        }
    }

    public static class DebugBootStrap
    {
        [ThreadStatic]
        public static List<SqlQueryLog> SqlQueries = new List<SqlQueryLog>();

        public static List<Recommendation> Recommendations = new List<Recommendation>();

        public static void Init(HttpApplication mvcApplication)
        {
            if (!IsInDebugMode()) return;

            GlobalFilters.Filters.Add(new ConsoleOutExceptionFilterAttribute());
            GlobalFilters.Filters.Add(new ConsoleOutPayloadFilterAttribute());

            Recommendations = new List<Recommendation> 
            {
                new FirstTimeRecommendation(),
                new SqlServerNameIsIncorrect(),
                new CreateDbRecommendation(),
                new LoginFailedRecommendation(),
                new TutorialCreateBlogs(),
                new TutorialProjectionsRecommendation(),
                new TutorialBlogIsValid(),
                new TutorialCreateComments(),
                new TutorialAddComment(),
                new CreateTableRecommendation(),
                new InvalidColumnRecommendation(),
                new ValidationsFailedRecommendation(),
                new NoDefinitionOnDerivedGeminiRecommendation(),
                new NoDefinitionOnGeminiRecommendation()
            };

            mvcApplication.EndRequest += PrintInefficientQueries;

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

            mvcApplication.Error += WriteHelpfulError(mvcApplication);
        }

        private static EventHandler WriteHelpfulError(HttpApplication mvcApplication)
        {
            return (sender, e) =>
            {
                if (mvcApplication.Response.ContentType != "text/html") return;

                lock (Massive.DynamicRepository.ConsoleLogLock)
                {
                    var lastError = mvcApplication.Server.GetLastError();
                    ReplaceExceptionWithRecommendationIfApplicable(mvcApplication, lastError);
                }
            };
        }

        private static void ReplaceExceptionWithRecommendationIfApplicable(HttpApplication mvcApplication, Exception error)
        {
            var applicable = Recommendations.Where(s => s.CanRecommend(error)).FirstOrDefault();

            List<string> recos = new List<string>();

            recos.Add(OriginalStackTrace(error));

            if (applicable != null)
            {
                recos.Add(applicable.GetRecommendation(error));
                WriteRecommendation(mvcApplication, recos);
            }
        }

        private static void WriteRecommendation(HttpApplication mvcApplication, List<string> applicatableRecommendations)
        {
            var recommendationString = @"
<html>
<head>
    <title>Oak Help</title>
</head>
<body style='font-size: 14px; font-family: Consolas, sans-serif; width: 1200px; margin-left: auto; margin-right: auto'>
    <style type=""text/css"">
        pre { background-color: #F5F5F5; border: solid 1px silver; padding: 10px; overflow-x: auto; white-space: pre-wrap; }
    </style>

    <h2>An error was thrown and Oak has a recommendation:</h2>

    {recommendations}

    <small>recommendations are brought to you by Oak\Boostrap.cs.</small>
</body>
</html>";
            mvcApplication.Server.ClearError();
            mvcApplication.Response.Write(
                recommendationString
                    .Replace("{recommendations}", string.Join("", applicatableRecommendations)));
        }

        private static string OriginalStackTrace(Exception error)
        {
            return @"
<div style='margin: 5px; font-weight: bold'>
    Here is original error was thrown with some noise taken out:
    <pre style=""font-size: 18px"">{message}</pre>
    stacktrace:
    <pre>{scrubbedStackTrace}</pre>
    <div style=""font-size: 12px; height: 30px"">
        <div style=""width: 100px; float: right;"">
         <img src=""http://i.imgur.com/kSZdd.png"" />
        </div>
        <div style=""width: 500px; float: right; padding: 3px"">
            Note: This error can also be seen in the IISExpress console.  Running <pre style=""display: inline; padding: 0px"">rake server</pre> starts up a IIS Express minimized with the following icon:
        </div>
    </div>
</div>".Replace("{message}", System.Web.HttpUtility.HtmlEncode(error.Message))
       .Replace("{scrubbedStackTrace}", Bullet.ScrubStackTrace(error.ToString()));
        }

        static void PrintInefficientQueries(object sender, EventArgs e)
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

                    if (s.Reason.Contains("may not be inefficient")) Console.ForegroundColor = ConsoleColor.DarkYellow;

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

    public class Recommendation
    {
        public virtual bool CanRecommend(Exception e) { return false; }

        public virtual string GetRecommendation(Exception e) { return ""; }

        public bool HasEmptyProject()
        {
            return Assembly
                .GetAssembly(typeof(Recommendation))
                .GetTypes()
                .Where(s => s.Name.EndsWith("Controller"))
                .Count() <= 2;
        }
    }

    public class FirstTimeRecommendation : Recommendation
    {
        public override bool CanRecommend(Exception e)
        {
            return e.ToString().Contains("Views/Home/Index") && HasEmptyProject();
        }

        public override string GetRecommendation(Exception e)
        {
            return @"
<h2>Try Oak using this tutorial</h2>
This is probably the first time you've run Oak for this solution. <strong>Be sure to visit
the website at some point and take a look at the screencasts and sample apps (STRONGLY recommended): 
<a href=""http://amirrajan.github.com/Oak"" target=""_blank"">Oak's Github Page</a></strong>.
If you want to try Oak out in an interactive way, do the following:<br/>
Update HomeController.cs and put the follwing <strong>between the namespace block</strong>:
<h3>Step 1 - Update HomeController.cs</h3>
<pre>
<img src=""http://i.imgur.com/YJrrG.png"" style=""float: right"" />
//here is how you setup up a dynamic repository
public class Blogs : DynamicRepository { }

//this is a dynamic entity that represents our blog
public class Blog : DynamicModel 
{
    public Blog() { } 

    public Blog(object dto) : base(dto) { }
}

//home controller
public class HomeController : Controller
{
    //initialize the blog
    Blogs blogs = new Blogs();

    public ActionResult Index()
    {
        //return all blogs from the database
        ViewBag.Blogs = blogs.All();
        return View();
    }

    //controller action to save a blog
    [HttpPost]
    public ActionResult Index(dynamic @params)
    {
        dynamic blog = new Blog(@params);

        if(!blog.IsValid()) 
        {
            ViewBag.Flash = blog.FirstError();
            return Index();
        }

        blogs.Insert(blog);
        return RedirectToAction(""Index"");
    }

    //controller action to add a comment
    [HttpPost]
    public ActionResult Comments(dynamic @params)
    {
        dynamic blog = blogs.Single(@params.BlogId);

        blog.AddComment(@params);

        return RedirectToAction(""Index"");
    }
}
</pre>
<h3>Step 2 - Add Home/Index.cshtml</h3>
Create an Index.cshtml page for the Index action and add the following code there:
<pre>
<img src=""http://i.imgur.com/YJrrG.png"" style=""float: right"" />
@{
    ViewBag.Title = ""Index"";
}

&lt;h2&gt;Hello World&lt;/h2&gt;
&lt;div&gt;
 If this page came up successfully, you're doing good! Go ahead and create a blog (try to create blogs with duplicate names).
&lt;/div&gt;
&lt;br /&gt;
@if (ViewBag.Flash != null)
{
    &lt;div style=""color:Red""&gt;@ViewBag.Flash&lt;/div&gt;
}

@using (Html.BeginForm())  
{
    @Html.TextBox(""Name"")
    &lt;input type=""submit"" value=""create"" /&gt;
}

@foreach (var blog in ViewBag.Blogs)
{
    &lt;div&gt;
        &lt;pre&gt;@blog&lt;/pre&gt;
        &lt;br /&gt;
        &lt;div&gt;
            Almost there, you have comments listing, lets try to add one.
        &lt;/div&gt;
        &lt;br /&gt;
        @using (Html.BeginForm(""Comments"", ""Home"", FormMethod.Post))
        {
            @Html.Hidden(""BlogId"", blog.Id.ToString() as string)
            @Html.TextBox(""Body"")
            &lt;input type=""submit"" value=""post comment"" /&gt;
        }
        &lt;br /&gt;
        &lt;pre&gt;@blog.Comments()&lt;/pre&gt;
    &lt;/div&gt;
}
</pre>";
        }
    }

    public class SqlServerNameIsIncorrect : Recommendation
    {
        public override bool CanRecommend(Exception e)
        {
            return e is SqlException && e.ToString().Contains("A network-related or instance-specific error occurred");
        }

        public override string GetRecommendation(Exception e)
        {
            var text = e.ToString();

            return @"
<h2>Update the web config with your server instance</h2>
    It doesn't look like I could connect to your SQL Server instace.  Update the web.config to point to a SQL Server instance.  Here is what's there now:
    <pre>{connectionString}</pre>
</body>
</html>
".Replace("{connectionString}", new ConnectionProfile().ConnectionString)
 .Replace("{scrubbedStackTrace}", Bullet.ScrubStackTrace(text));
        }
    }

    public class LoginFailedRecommendation : Recommendation
    {
        public override bool CanRecommend(Exception e)
        {
            return e is SqlException && e.ToString().Contains("Login failed for user");
        }

        public override string GetRecommendation(Exception e)
        {
            var text = e.ToString();

            return @"
<h2>Update the web config with your login credentials</h2>
    It doesn't look like the login credentials in the connection string are correct.  Update the web.config with valid credentials.  Here is what's there now:
    <pre>{connectionString}</pre>
</body>
</html>
".Replace("{connectionString}", new ConnectionProfile().ConnectionString)
 .Replace("{scrubbedStackTrace}", Bullet.ScrubStackTrace(text));
        }
    }

    public class CreateDbRecommendation : Recommendation
    {
        public override bool CanRecommend(Exception e)
        {
            return e is SqlException && e.ToString().Contains("Cannot open database");
        }

        public override string GetRecommendation(Exception e)
        {
            var text = e.ToString();

            return @"
<h2>Create a database</h2>
    You need to create the database that will work with this connection string (retrieved from web.config):
    <pre>{connectionString}</pre>
</body>
</html>
".Replace("{connectionString}", new ConnectionProfile().ConnectionString)
 .Replace("{scrubbedStackTrace}", Bullet.ScrubStackTrace(text));
        }
    }

    public class TutorialCreateBlogs : Recommendation
    {
        public override bool CanRecommend(Exception e)
        {
            return e is SqlException
                && e.ToString().Contains("Invalid object name 'Blogs'.")
                && HasEmptyProject();
        }

        public override string GetRecommendation(Exception e)
        {
            return @"
<h2>Tutorial: Create the Blogs table using SeedController</h2>
It looks like you are trying to access an object in the database that doesn't exist.  
Go to Controller\SeedController.cs and add a method to the Schema class to generate the Blogs table (keep in mind
that the default convention for Oak is a pluralized table name).<br/><br/>
Here is an example of creating this schema:
<pre>
<img src=""http://i.imgur.com/YJrrG.png"" style=""float: right"" />
    public class Schema
    {
        //this is the method you'll want to alter
        public IEnumerable&lt;Func&lt;dynamic&gt;&gt; Scripts()
        {
            //replace all content inside of the Scripts() method with this line
            yield return CreateBlogsTable; //return just the <strong>pointer</strong> to the function
        }

        public string CreateBlogsTable() //here is the function definition
        {
            //this is an example, your table name may be different
            //for more information on schema generation <a href=""https://github.com/amirrajan/Oak/wiki/Creating-schema-using-Oak.Seed"" target=""_blank"">check out the Oak wiki</a> 
            return Seed.CreateTable(""Blogs"",
                Seed.Id(),
                new { Name = ""nvarchar(255)"" },
                new { Body = ""nvarchar(max)"" }
            );
        }

        public void SampleEntries()
        {

        }

        public Seed Seed { get; set; }

        public Schema(Seed seed) { Seed = seed; }
    }
</pre>

After adding the function to create your table.  Run this command to execute 
the script (the console window you use to execute this command must have ruby support):
<pre>rake reset <img src=""http://i.imgur.com/Y2i1G.png"" style=""float: right"" /></pre>";
        }
    }

    public class TutorialCreateComments : Recommendation
    {
        public override bool CanRecommend(Exception e)
        {
            return e is SqlException && e.ToString().Contains("Invalid object name 'Comments'") && HasEmptyProject();
        }

        public override string GetRecommendation(Exception e)
        {
            return @"
<h2>Tutorial: Create the Comments table using SeedController</h2>
It looks like you are trying to access an object in the database that doesn't exist.  
Go to Controller\SeedController.cs and add a method to the Schema class to generate the Comments table (keep in mind
that the default convention for Oak is a pluralized table name).<br/><br/>
Here is an example of creating this schema:
<pre>
<img src=""http://i.imgur.com/YJrrG.png"" style=""float: right"" />
    public class Schema
    {
        //this is the method you'll want to alter
        public IEnumerable&lt;Func&lt;dynamic&gt;&gt; Scripts()
        {
            //replace all content inside of the Scripts() method with this line
            yield return CreateBlogsTable; //return just the <strong>pointer</strong> to the function

            yield return CreateCommentsTable; //return just the <strong>pointer</strong> to the function
        }

        public string CreateBlogsTable() //here is the function definition
        {
            //this is an example, your table name may be different
            //for more information on schema generation <a href=""https://github.com/amirrajan/Oak/wiki/Creating-schema-using-Oak.Seed"" target=""_blank"">check out the Oak wiki</a> 
            return Seed.CreateTable(""Blogs"",
                Seed.Id(),
                new { Name = ""nvarchar(255)"" },
                new { Body = ""nvarchar(max)"" }
            );
        }

        public string CreateCommentsTable() //here is the function definition
        {
            //this is an example, your table name may be different
            //for more information on schema generation <a href=""https://github.com/amirrajan/Oak/wiki/Creating-schema-using-Oak.Seed"" target=""_blank"">check out the Oak wiki</a> 
            return Seed.CreateTable(""Comments"",
                Seed.Id(),
                new { BlogId = ""int"", ForeignKey = ""Blogs(Id)"" },
                new { Body = ""nvarchar(max)"" }
            );
        }

        public void SampleEntries()
        {

        }

        public Seed Seed { get; set; }

        public Schema(Seed seed) { Seed = seed; }
    }
</pre>

After adding the function to create your table.  Run this command to execute 
the script (the console window you use to execute this command must have ruby support):
<pre>rake reset <img src=""http://i.imgur.com/Y2i1G.png"" style=""float: right"" /></pre>";
        }
    }

    public class CreateTableRecommendation : Recommendation
    {
        public override bool CanRecommend(Exception e)
        {
            return e is SqlException && e.ToString().Contains("Invalid object name");
        }

        public override string GetRecommendation(Exception e)
        {
            return @"
<h2>Using SeedController to create tables (in general)</h2>
It looks like you are trying to access an object in the database that doesn't exist.  
Go to Controller\SeedController.cs and add a method to the Schema class to generate your table (keep in mind
that the default convention for Oak is a pluralized table name).<br/><br/>
Here is an example of creating this schema (let's say I want to create a table called Blogs):
<pre>
<img src=""http://i.imgur.com/YJrrG.png"" style=""float: right"" />
    public class Schema //this class already exists in SeedController.cs
    {
        //this is the method you'll want to alter
        public IEnumerable&lt;Func&lt;dynamic&gt;&gt; Scripts()
        {
            //this is just an example, your table name may be different
            yield return CreateBlogsTable; //return just the <strong>pointer</strong> to the function
        }

        public string CreateBlogsTable() //here is the function definition
        {
            //this is an example, your table name may be different
            //for more information on schema generation <a href=""https://github.com/amirrajan/Oak/wiki/Creating-schema-using-Oak.Seed"" target=""_blank"">check out the Oak wiki</a> 
            return Seed.CreateTable(""Blogs"",
                Seed.Id(),
                new { Name = ""nvarchar(255)"" },
                new { Body = ""nvarchar(max)"" }
            );
        }
    }

</pre>

After adding the function to create your table.  Run this command to execute 
the script (the console window you use to execute this command must have ruby support):
<pre>rake reset <img src=""http://i.imgur.com/Y2i1G.png"" style=""float: right"" /></pre>

You can see what the script looks like by running this command: <pre>rake export <img src=""http://i.imgur.com/Y2i1G.png"" style=""float: right"" /></pre>

<h2>Use SeedController to create Sample Entries</h2>

I would also recommend looking at the SampleEntries() method in Controllers/SeedController.cs.  You can use that to generate sample data.  For example:
<pre>
<img src=""http://i.imgur.com/YJrrG.png"" style=""float: right"" />
public void SampleEntries()
{
    //here are a few sample entries
    //for more information on sample entries <a href=""https://github.com/amirrajan/Oak/wiki/Creating-sample-data-using-Oak.Seed"" target=""_blank"">check out the Oak wiki</a> 
    new
    {
        Name = ""Hello World"",
        Body = ""Lorem Ipsum""
    }.InsertInto(""Blogs"");

    new
    {
        Name = ""Hello World 2"",
        Body = ""Lorem Ipsum 2""
    }.InsertInto(""Blogs"");
}
</pre>

You can then run this command to <strong>purge</strong> your database and regen it with sample data: <pre>rake sample <img src=""http://i.imgur.com/Y2i1G.png"" style=""float: right"" /></pre>";
        }
    }

    public class TutorialProjectionsRecommendation : Recommendation
    {
        public override bool CanRecommend(Exception e)
        {
            return e.ToString().Contains("'Oak.Gemini' does not contain a definition for 'Comments'") && HasEmptyProject();
        }

        public override string GetRecommendation(Exception e)
        {
            return @"
<h2>Tutorial: Update blogs repository to return a Blog instead of a Gemini</h2>
<h3>Step 1 - Update the Blogs class located in the HomeController.cs file</h3>
By default dynamic repository returns a ""typeless"" dynamic object (called Gemini).  To get comments
retrieving, the first thing we need to do is return a Blog from the database.  The way we do this 
is by defining a projection:
<pre>
<img src=""http://i.imgur.com/YJrrG.png"" style=""float: right"" />
//our Hello World example
public class Blogs : DynamicRepository 
{
    public Blogs()
    {
        Projection = dto =&gt; new Blog(dto); //this is a projection
    }
}
</pre>

<h3>Step 2 - In HomeController.cs, declare the Comments repository and give the Blog class an association to Comments</h3>
With the projection in place, you will now get back a dynamically typed object 
(as opposed to just a Gemini). Any query that is executed from a ""projected"" 
dynamic repository (in this case Blogs) will go through this mapping.  Now that the project
is in place we can add an association to blog to return comments. <strong>Here is how you 
say ""A blog has many comments""</strong>:
<pre>
<img src=""http://i.imgur.com/YJrrG.png"" style=""float: right"" />
//define the repository to retrieve comments
public class Comments : DynamicRepository 
{ 

}

public class Blog : DynamicModel 
{
    Blogs blogs = new Blogs();
    
    //define comments
    Comments comments = new Comments();

    public Blog() { }

    public Blog(object dto) : base(dto) { }

    //add an Associates method to add the Comments() method
    IEnumerable&lt;dynamic&gt; Associates()
    {
        //and define the association
        //for othere examples of associations <a href=""https://github.com/amirrajan/Oak/wiki/Adding-associations-using-Oak.DynamicModel"" target=""_blank"">check out the Oak wiki</a>
        yield return new HasMany(comments);
    }

    //add a Validates method to add the IsValid() method
    IEnumerable&lt;dynamic&gt; Validates()
    {
        //and define the association
        //for othere examples of validations <a href=""https://github.com/amirrajan/Oak/wiki/Adding-validation-using-Oak.DynamicModel"" target=""_blank"">check out the Oak wiki</a>
        yield return new Uniqueness(""Name"", blogs);
    }
}</pre>
";
        }
    }

    public class NoDefinitionOnGeminiRecommendation : Recommendation
    {
        public override bool CanRecommend(Exception e)
        {
            return e.ToString().Contains("'Oak.Gemini' does not contain a definition for");
        }

        public override string GetRecommendation(Exception e)
        {
            return @"
<h2>Inspect the object or setup a database Projection (general debugging)</h2>

<h3>To inspect the object and debug</h3>
The Gemini object has quite a few <a href=""https://github.com/amirrajan/Oak/wiki/Gemini-First-Look"" target=""_blank"">introspection methods</a> 
to help determine what's inside of it.  Calling the ToString() method will give you a 
nicely formatted string.  In the Immediate Window (Ctrl + Alt + i) you can type the follow to get 
the nicely formatted output (using our Blog example): 
<pre>ViewBag.Blogs.ToString(),nq</pre> 
If you are running this through specwatchr/sidekick, you'll need to attach 
the debugger to use the Immediate Window. 
You can do so by adding the following line in a controller action and refreshing the page:
<pre>
<img src=""http://i.imgur.com/YJrrG.png"" style=""float: right"" />
public class HomeController : Controller
{
    Blogs blogs = new Blogs();

    public ActionResult Index()
    {
        ViewBag.Blogs = blogs.All();

        //add this line to hook up the debugger
        //and then you can type ViewBag.Blogs.ToString(),nq
        //in the Immediate Window (Ctrl + Alt + i)...
        //a Visual Studio dialog will pop up (it may pop up 
        //behind the this window, so keep a look out for it in
        //the task bar).  You can also just manually attach to the 
        //IIS Express process using the Tools->Attach To Process menu item.
        System.Diagnostics.Debugger.Launch(); 

        return View();
    }
}
</pre>

<h3>Add a database projection if you're retrieving records from a database</h3>
By default dynamic repository returns a ""typeless"" dynamic object (called Gemini).  You can return
a dynamic type by doing the following (again using our Blog example):
<pre>
<img src=""http://i.imgur.com/YJrrG.png"" style=""float: right"" />
//our Hello World example
public class Blogs : DynamicRepository 
{
    public Blogs()
    {
        Projection = dto =&gt; new Blog(dto); //this is a projection
    }
}

public class Blog : DynamicModel 
{
    public Blog() { } 

    public Blog(object dto) : base(dto) { }
}
</pre>

With the projection in place, you will now get back a dynamically typed object 
(as opposed to just a Gemini). Any query that is executed from a ""projected"" 
dynamic repository (in this case Blogs) will go through this mapping.
For more information on dynamic repository methods, take a look at 
<a href=""https://github.com/amirrajan/Oak/wiki/Retrieving-and-Saving-data-using-Massive.DynamicRepository"" targe=t""_blank"">Oak's wiki on DynamicRepository</a>.
";
        }
    }

    public class TutorialBlogIsValid : Recommendation
    {
        public override bool CanRecommend(Exception e)
        {
            return e.ToString().Contains("Blog' does not contain a definition for 'IsValid'") && HasEmptyProject();
        }

        public override string GetRecommendation(Exception e)
        {
            return @"
<h2>Tutorial: Add Validations to Blog</h2>
If you are performing validations on your objects you can add validation methods through
Oak.  Update the Blog class in HomeController.cs. <strong>Blog names must be unique</strong>:
<pre>
<img src=""http://i.imgur.com/YJrrG.png"" style=""float: right"" />
public class Blog : DynamicModel 
{
    //initialize blogs
    Blogs blogs = new Blogs();

    public Blog() { }

    public Blog(object dto) : base(dto) { }

    //add a Validates method to add the IsValid() method
    IEnumerable&lt;dynamic&gt; Validates()
    {
        //and define the association
        //for othere examples of validations <a href=""https://github.com/amirrajan/Oak/wiki/Adding-validation-using-Oak.DynamicModel"" target=""_blank"">check out the Oak wiki</a>
        yield return new Uniqueness(""Name"", blogs);
    }
}
</pre>

In the case of the Hello World sample, the code above is how you would get rid of exceptions associated with IsValid();
";
        }
    }

    public class TutorialAddComment : Recommendation
    {
        public override bool CanRecommend(Exception e)
        {
            return e.ToString().Contains("Blog' does not contain a definition for 'AddComment'") && HasEmptyProject();
        }

        public override string GetRecommendation(Exception e)
        {
            return @"
<h2>Tutorial: Declare the AddComment method on Blog</h2>
These dynamic classes can be treated as any other class.  You can add methods on them directly.  The cool thing however
is you can add methods on these classes as implicit methods and they will be accessible publicly. Methods
that have any of the following signatures are considered public automatically:
<ul>
    <li>void SomeMethod()</li>
    <li>void SomeMethodWithParam(dynamic args)</li>
    <li>dynamic SomeFunction()</li>
    <li>dynamic SomeFunctionWithParam(dynamic args)</li>
</ul>
<h3>Step 1 - In HomeController.cs change Blog to have an AddComment method</h3>
<pre>
<img src=""http://i.imgur.com/YJrrG.png"" style=""float: right"" />
public class Blog : DynamicModel
{
    Blogs blogs = new Blogs();

    //define comments
    Comments comments = new Comments();

    public Blog() { }

    public Blog(object dto) : base(dto) { }

    IEnumerable&lt;dynamic&gt; Validates()
    {
        //and define the association
        //for othere examples of validations check out the Oak wiki
        yield return new Uniqueness(""Name"", blogs);
    }

    //add an Associates method to add the Comments() method
    IEnumerable&lt;dynamic&gt; Associates()
    {
        //and define the association
        //for othere examples of associations check out the Oak wiki
        yield return new HasMany(comments);
    }

    //notice that the method is defined implicitly
    void AddComment(dynamic comment)
    {
        //ignore addition if the body is empty
        if(string.IsNullOrEmpty(comment.Body)) return;

        //any dynamic property on this instance can be accessed through the ""_"" property
        var commentToSave = _.Comments().New(comment);

        comments.Insert(commentToSave);
    }
}
</pre>
<h3>Step 2 - Eager load Comments in HomeController's Index method</h3>
Every Blog on the main page will load its comments (hitting the database for each blog).  This is inefficient and
should be fixed.  <strong>Update HomeController's Index method to eager load Comments:</strong>
<pre>
<img src=""http://i.imgur.com/YJrrG.png"" style=""float: right"" />
public ActionResult Index()
{
    //return all blogs from the database
    ViewBag.Blogs = blogs.All();

    //eager load comments
    ViewBag.Blogs.Include(""Comments"");

    return View();
}
</pre>";
        }
    }

    public class ValidationsFailedRecommendation : Recommendation
    {
        public override bool CanRecommend(Exception e)
        {
            if (e.Message.Contains("Validation initialization failed for class ")) return true;

            return false;
        }

        public override string GetRecommendation(Exception e)
        {
            return @"
<h2>Review the IEnumerable&lt;dynamic&gt; Validates() method.<br/>The collection of validation rules is throwing an exception.</h2>
<p>It looks like your IEnumerable&lt;dynamic&gt; Validates() is throwing an exception. The stacktrace above should point out the specific line that is 
throwing the exception.  Most likely, the exception is being thrown because you are trying to access properties that don't exist (yet).</p>

<h3>Multiple ways to define an Error Message</h3>
<p>
There are two ways you can define error messages, the first option is evaluated immediately during initialization (and may throw an exception if the property doesn't exist):
</p>
<pre>
<img src=""http://i.imgur.com/YJrrG.png"" style=""float: right"" />
//example of a ErrorMessage defined inside of the validates method
yield return new Length(""Title"") { Minimum = 1, ErrorMessage = _.Title + "" is too short."" };
</pre>
<p>
The second approach defers the retrieval of the error message that will allow for lazy evaluation:
</p>
<pre>
<img src=""http://i.imgur.com/YJrrG.png"" style=""float: right"" />
//example of a ErrorMessage defined inside of the validates method
yield return new Length(""Title"") { Minimum = 1, ErrorMessage = new DynamicFunction(() => _.Title + "" is too short."") };
</pre>
";
        }
    }

    public class NoDefinitionOnDerivedGeminiRecommendation : Recommendation
    {
        public override bool CanRecommend(Exception e)
        {
            if (!e.Message.Contains("does not contain a definition for")) return false;

            if (e.Message.Contains("Gemini")) return false;

            return true;
        }

        public override string GetRecommendation(Exception e)
        {
            var tokens = e.Message.Split(new[] { "'" }, StringSplitOptions.RemoveEmptyEntries);

            var type = tokens.First();

            var method = tokens.Last();

            return @"
<h2>Declare a method (in general)</h2>
It looks like you are trying to access a method [{method}] on type [{type}] that doesn't exist.
Here are a few ways methods are added to a dynamic object.
<ul>
   <li>Declaring a method through Associations</li>
   <li>Declaring a method through Validations</li>
   <li>Declaring a method on class directly</li>
</ul>

<h3>Declaring a method through Associations</h3>
If you are attempting to retrieve a relationship on an object from the database.  You need to 
define an Association.  Here is an example of an association, <strong>a Blog has many Comments</strong>:

<pre>
<img src=""http://i.imgur.com/YJrrG.png"" style=""float: right"" />
//our Hello World example
public class Blogs : DynamicRepository 
{
    public Blogs()
    {
        Projection = dto =&gt; new Blog(dto); //this is a projection
    }
}

//declare the comments repository
public class Comments : DynamicRepository 
{ 

}

public class Blog : DynamicModel 
{
    //initialize comments
    Comments comments = new Comments();

    public Blog() { } 

    public Blog(object dto) : base(dto) { }

    //add an Associates method to add the Comments() method
    IEnumerable&lt;dynamic&gt; Associates()
    {
        //and define the association
        //for othere examples of associations <a href=""https://github.com/amirrajan/Oak/wiki/Adding-associations-using-Oak.DynamicModel"" target=""_blank"">check out the Oak wiki</a>
        yield return new HasMany(comments);
    }
}
</pre>

In the case of the Hello World sample, the code above is how you would add and access Comments();

<h3>Declaring a method through Validations</h3>
If you are performing validations on your objects you can add validation methods through
Oak.  Here is an example of a validation, <strong>Blog names must be unique</strong>:
<pre>
<img src=""http://i.imgur.com/YJrrG.png"" style=""float: right"" />
public class Blog : DynamicModel 
{
    //initialize blogs
    Blogs blogs = new Blogs();

    public Blog() { }

    public Blog(object dto) : base(dto) { }

    //add a Validates method to add the IsValid() method
    IEnumerable&lt;dynamic&gt; Validates()
    {
        //and define the association
        //for othere examples of validations <a href=""https://github.com/amirrajan/Oak/wiki/Adding-validation-using-Oak.DynamicModel"" target=""_blank"">check out the Oak wiki</a>
        yield return new Uniqueness(""Name"", blogs);
    }
}
</pre>

In the case of the Hello World sample, the code above is how you would get rid of exceptions associated with IsValid();

<h3>Declaring a method on class directly</h3>
These dynamic classes can be treated as any other class.  You can add methods on them directly.  The cool thing however
is you can add methods on these classes as implicit methods and they will be accessible publicly. Methods
that have any of the following signatures are considered public automatically:
<ul>
    <li>void SomeMethod()</li>
    <li>void SomeMethodWithParam(dynamic args)</li>
    <li>dynamic SomeFunction()</li>
    <li>dynamic SomeFunctionWithParam(dynamic args)</li>
</ul>
<strong>Here is an example of a method where a blog can add a comment to itself</strong>
<pre>
<img src=""http://i.imgur.com/YJrrG.png"" style=""float: right"" />
public class Blog : DynamicModel 
{
    Comments comments = new Comments();

    public Blog() { }

    public Blog(object dto) : base(dto) { }

    //notice that the method is defined implicitly
    void AddComment(dynamic comment)
    {
        //ignore addition if the body is empty
        if(string.IsNullOrEmpty(comment.Body)) return;

        //any dynamic property on this instance can be accessed through the ""_"" property
        var commentToSave = _.Comments().New(comment);

        comments.Insert(commentToSave);
    }
}
</pre>

In the case of the Hello World sample, the code above is how you would get rid of exceptions associated with AddComment();

<h3>Side Note: Testing</h3>
The nifty thing about defining dynamic methods implicitly is that they an be 
redefined in testing (and anywhere else for that matter).  Here is an example:
<pre>
<img src=""http://i.imgur.com/YJrrG.png"" style=""float: right"" />
bool commentAdded = false;
dynamic blog = new Blog(new { Name = ""Some Name"" });
blog.AddComment = 
    new DynamicFunction(d => 
    {
        commentAdded = true;
        return null;
    });

blog.AddComment();

Assert.IsTrue(commentAdded);
</pre>
".Replace("{method}", method)
 .Replace("{type}", type);
        }
    }

    public class InvalidColumnRecommendation : Recommendation
    {
        public override bool CanRecommend(Exception e)
        {
            return e.Message.Contains("Invalid column name");
        }

        public override string GetRecommendation(Exception e)
        {
            return @"
<h2>Use SeedController to add a column</h2>
Seed controller can be used to add columns.  It's important to not change existing schema/migration methods in SeedController.  Just
add new ones.  <strong>Here is an example of adding a BlogId column to a Comments table</strong>:
<pre>
<img src=""http://i.imgur.com/YJrrG.png"" style=""float: right"" />
    public class Schema //this class exists in SeedController.cs
    {
        public IEnumerable&lt;Func&lt;dynamic&gt;&gt; Scripts()
        {
            [.... other methods .....]
            yield return AddBlogIdToComments; //return just the <strong>pointer</strong> to the function
        }

        public string AddBlogIdToComments() //here is the function definition
        {
            //this is an example, your table name and columns may be different
            //for more information on schema generation <a href=""https://github.com/amirrajan/Oak/wiki/Creating-schema-using-Oak.Seed"" target=""_blank"">check out the Oak wiki</a> 
            return Seed.AddColumns(""Comments"",
                new { BlogId = ""int"", ForeignKey = ""Blogs(Id)"" }
            );
        }
    }
</pre>

After adding the function to create your table.  Run this command to execute the script:<pre>rake reset <img src=""http://i.imgur.com/Y2i1G.png"" style=""float: right"" /></pre>

You can see what the script looks like by running this command: <pre>rake export <img src=""http://i.imgur.com/Y2i1G.png"" style=""float: right"" /></pre>

";
        }
    }
}