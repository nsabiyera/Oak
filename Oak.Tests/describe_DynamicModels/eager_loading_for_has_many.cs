using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicModels.Classes;
using Massive;

namespace Oak.Tests.describe_DynamicModels
{
    class eager_loading_for_has_many : nspec
    {
        Seed seed;

        object book1Id, book2Id;

        dynamic books;

        void before_each()
        {
            seed = new Seed();

            books = new Books();

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
        }

        void it_eager_loads_child_collections_and_caches_them()
        {
            List<string> sqlQueries = new List<string>();

            DynamicRepository.WriteDevLog = true;

            DynamicRepository.LogSql = new Action<object, string, object[]>(
                (sender, sql, @params) =>
                {
                    sqlQueries.Add(sql);
                });

            var allBooks = books.All().Include("Chapters");

            ((int)allBooks.First().Chapters().Count()).should_be(2);

            var chapters = allBooks.First().Chapters();

            (chapters.First().Name as string).should_be("Chapter I");

            (chapters.Last().Name as string).should_be("Chapter II");

            ((int)allBooks.Last().Chapters().Count()).should_be(2);

            chapters = allBooks.Last().Chapters();

            (chapters.First().Name as string).should_be("Chapter 1");

            (chapters.Last().Name as string).should_be("Chapter 2");

            sqlQueries.Count.should_be(2);
        }

        void specify_eager_loaded_collections_retain_creation_methods()
        {
            var firstBook = books.All().Include("Chapters").First();

            var chapter = firstBook.Chapters().New(new { Name = "Chapter 3" });

            new Chapters().Insert(chapter);

            firstBook = books.All().Include("Chapters").First();

            ((int)firstBook.Chapters().Count()).should_be(3);
        }
    }
}
