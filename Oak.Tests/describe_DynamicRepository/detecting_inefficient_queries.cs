using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicRepository.Classes;
using Massive;

namespace Oak.Tests.describe_DynamicRepository
{
    class detecting_inefficient_queries : nspec
    {
        Seed seed;

        object book1Id, book2Id;

        List<string> recordedQueries;

        Books books;

        Chapters chapters;

        void before_each()
        {
            seed = new Seed();

            books = new Books();

            chapters = new Chapters();

            seed.PurgeDb();

            seed.CreateTable("Books",
                new { Id = "int" },
                new { Title = "nvarchar(255)" }).ExecuteNonQuery();

            seed.CreateTable("Chapters",
                new { Id = "int" },
                new { BookId = "int" },
                new { Name = "nvarchar(255)" }).ExecuteNonQuery();

            book1Id = 100;

            new { Id = book1Id, Title = "book 1" }.InsertInto("Books");

            new { Id = 200, BookId = book1Id, Name = "Chapter I" }.InsertInto("Chapters");

            new { Id = 300, BookId = book1Id, Name = "Chapter II" }.InsertInto("Chapters");

            book2Id = 400;

            new { Id = book2Id, Title = "book 2" }.InsertInto("Books");

            new { Id = 500, BookId = book2Id, Name = "Chapter 1" }.InsertInto("Chapters");

            new { Id = 600, BookId = book2Id, Name = "Chapter 2" }.InsertInto("Chapters");

            recordedQueries = new List<string>();

            DynamicRepository.WriteDevLog = true;
            DynamicRepository.LogSql = (sender, sql, args) =>
            {
                recordedQueries.Add(sql);
            };
        }

        //void specify_inefficient_query_is_detected_on_has_many_association()
        //{
        //    var booksReturned = books.All();

        //    booksReturned.ForEach(s => s.Chapters());

        //    var queries = Bullet.InefficientQueries(recordedQueries);

        //    queries.Count().should_be(2);

        //    (queries.First().Query as string).should_contain("where BookId in ('100')");
        //    (queries.First().Reason as string).should_contain("N+1");

        //    (queries.Last().Query as string).should_contain("where BookId in ('400')");
        //    (queries.Last().Reason as string).should_contain("N+1");
        //}

        //void specify_inefficient_query_is_detected_on_belongs_to_association()
        //{
        //    var allChapters = chapters.All();

        //    allChapters.ForEach(s => s.Book());

        //    var queries = Bullet.InefficientQueries(recordedQueries);

        //    queries.Count().should_be(1);

        //    queries.ForEach(s =>
        //    {
        //        Console.WriteLine(s.Query);
        //        Console.WriteLine(s.Reason);
        //    });

        //    (queries.First().Query as string).should_contain("SELECT * FROM Books");
        //    (queries.First().Reason as string).should_contain("N+1");
        //}

        //void specify_inefficient_query_is_detected_on_redundant_selects()
        //{
        //    var booksReturned = books.All();
        //    booksReturned = books.All();
        //    booksReturned = books.All();

        //    var queries = Bullet.InefficientQueries(recordedQueries);

        //    queries.Count().should_be(1);

        //    (queries.First().Query as string).should_contain("SELECT * FROM Books");
        //    (queries.First().Reason as string).should_contain("redundant");
        //}

        //void specify_dissimilar_queries_are_not_considered_ineffecient()
        //{
        //    Bullet.InefficientQueries(new List<string> 
        //    { 
        //        "select * from Blogs",
        //        "select * from Comments"
        //    }).should_be(false);
        //}
    }
}
