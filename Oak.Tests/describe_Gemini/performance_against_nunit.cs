using System;
using System.Collections.Generic;
using System.Linq;
using NSpec;
using Oak.Tests.describe_Gemini.Classes;
using FluentNHibernate.Automapping;
using NHibernate;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate.Linq;
using System.Configuration;
using System.Data.SqlClient;
using Dapper;

namespace Oak.Tests.describe_Gemini
{
    [Tag("wip,performance")]
    abstract class performance : nspec
    {
        public DateTime finish;
        public Seed seed = new Seed();
        public DateTime start;
        public Persons peoples = new Persons();

        void before_each()
        {
            CreateTable();

            Insert100kRows();

            PrimeTheDbForSelectStatement();
        }

        public void CreateTable()
        {
            seed.PurgeDb();

            seed.CreateTable("Person", new dynamic[] 
            { 
                seed.Id(),
                new { FirstName = "nvarchar(255)" },
                new { MiddleName = "nvarchar(255)" },
                new { LastName = "nvarchar(255)" },
            }).ExecuteNonQuery();
        }

        public int TotalSeconds(TimeSpan time)
        {
            Console.WriteLine(time.TotalSeconds);

            return Convert.ToInt32(time.TotalSeconds);
        }

        public static void PrimeTheDbForSelectStatement()
        {
            var reader = "select * from Person".ExecuteReader();

            reader.Close();
        }

        public void Insert100kRows()
        {
            var lotsOfPeople = new List<dynamic>();

            for (int i = 0; i < 100000; i++)
            {
                lotsOfPeople.Add(new
                {
                    FirstName = Guid.NewGuid().ToString(),
                    MiddleName = Guid.NewGuid().ToString(),
                    LastName = Guid.NewGuid().ToString()
                });
            }

            peoples.Save(lotsOfPeople.ToArray());
        }
    }

    //[Tag("wip,performance")]
    //class performance_dynamic_model_against_dapper : performance
    //{
    //    void specify_it_runs_fast_enough()
    //    {
    //        var dynamicModelTime = TimeToRetrieve100kRowsFromDynamicModel();

    //        var dapperTime = TimeToRetrive100kRowsFromDapper();

    //        dynamicModelTime.should_be_less_or_equal_to(dapperTime);
    //    }

    //    private int TimeToRetrieve100kRowsFromDynamicModel()
    //    {
    //        start = DateTime.Now;

    //        peoples.Projection = d => new Gemini(d);

    //        var oakPeople = peoples.All().ToList();

    //        oakPeople.Count.should_be(100000);

    //        finish = DateTime.Now;

    //        return TotalSeconds(finish - start);
    //    }

    //    private int TimeToRetrive100kRowsFromDapper()
    //    {
    //        start = DateTime.Now;

    //        using (var connection = new SqlConnection(new ConnectionProfile().ConnectionString))
    //        {
    //            connection.Open();
    //            var posts = connection.Query<Person>("select * from Person");
    //            posts.Count().should_be(100000);
    //        }

    //        finish = DateTime.Now;

    //        return TotalSeconds(finish - start);
    //    }
    //}

    [Tag("performance")]
    class performance_dynamic_model_against_gemini : performance
    {
        void specify_it_runs_fast_enough()
        {
            var justGeminiTime = TimeToRetrieve100kRowsFromGemini();

            var dynamicModelTime = TimeToRetrieve100kRowsFromDynamicModel();

            dynamicModelTime.should_be_less_or_equal_to(justGeminiTime);
        }

        private int TimeToRetrieve100kRowsFromGemini()
        {
            start = DateTime.Now;

            peoples.Projection = d => new Gemini(d);

            var oakPeople = peoples.All().ToList();

            oakPeople.Count.should_be(100000);

            finish = DateTime.Now;

            return TotalSeconds(finish - start);
        }

        private int TimeToRetrieve100kRowsFromDynamicModel()
        {
            start = DateTime.Now;

            peoples.Projection = d => new DynamicPerson(d);

            var oakPeople = peoples.All().ToList();

            oakPeople.Count.should_be(100000);

            finish = DateTime.Now;

            return TotalSeconds(finish - start);
        }
    }

    [Tag("performance")]
    class performance_against_nunit : performance
    {
        void specify_it_runs_fast_enough()
        {
            var nhTime = TimeToRetrieve100kRowsFromNHibernate();

            var oakTime = TimeToRetrieve100kRowsFromOak();

            oakTime.should_be_less_or_equal_to(nhTime);
        }

        private static AutoPersistenceModel CreateMappings()
        {
            return AutoMap
                .AssemblyOf<Person>()
                .Where(t => t == typeof(Person));
        }

        private static ISessionFactory BuildSessionFactory()
        {
            AutoPersistenceModel model = CreateMappings();
            return Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2008.ConnectionString(ConfigurationManager.ConnectionStrings["OakTestsConnectionString"].ConnectionString))
                .Mappings(m => m.AutoMappings.Add(model))
                .BuildSessionFactory();
        }

        private int TimeToRetrieve100kRowsFromNHibernate()
        {
            var sessionFactory = BuildSessionFactory();

            using (ISession session = sessionFactory.OpenSession())
            {
                start = DateTime.Now;

                var nhPeople = session.Query<Person>().ToList();

                nhPeople.Count.should_be(100000);

                finish = DateTime.Now;

                return TotalSeconds(finish - start);
            }
        }

        private int TimeToRetrieve100kRowsFromOak()
        {
            start = DateTime.Now;

            var oakPeople = peoples.All().ToList();

            oakPeople.Count.should_be(100000);

            finish = DateTime.Now;

            return TotalSeconds(finish - start);
        }
    }
}
