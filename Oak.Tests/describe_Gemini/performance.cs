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

namespace Oak.Tests.describe_Gemini
{
    class performance : nspec
    {
        DateTime finish;
        Seed seed = new Seed();
        DateTime start;
        Persons peoples = new Persons();

        void specify_it_runs_fast_enough()
        {
            CreateTable();

            Insert100kRows();

            PrimeTheDbForSelectStatement();

            var nhTime = TimeToRetrieve100kRowsFromNHibernate();

            var oakTime = TimeToRetrieve100kRowsFromOak();

            oakTime.should_be_less_or_equal_to(nhTime);
        }

        int TotalSeconds(TimeSpan time)
        {
            return Convert.ToInt32(time.TotalSeconds);
        }

        void CreateTable()
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

        private void Insert100kRows()
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

        private static void PrimeTheDbForSelectStatement()
        {
            var reader = "select * from Person".ExecuteReader();

            reader.Close();
        }
    }
}
