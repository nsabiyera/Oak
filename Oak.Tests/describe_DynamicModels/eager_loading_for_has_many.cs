using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicModels.Classes;
using Massive;

namespace Oak.Tests.describe_DynamicModels
{
    [Tag("wip")]
    class eager_loading_for_has_many : nspec
    {
        Seed seed = new Seed();

        object book1Id, book2Id;

        dynamic books;

        void before_each()
        {
            seed = new Seed();

            books = new Books();

            seed.PurgeDb();

            seed.CreateTable("Books",
                seed.Id(),
                new { Title = "nvarchar(255)" }).ExecuteNonQuery();

            seed.CreateTable("Chapters",
                seed.Id(),
                new { BookId = "int" },
                new { Name = "nvarchar(255)" }).ExecuteNonQuery();

            book1Id = new { Title = "book 1" }.InsertInto("Books");

            new { BookId = book1Id, Name = "Chapter 1" }.InsertInto("Chapters");

            new { BookId = book1Id, Name = "Chapter 2" }.InsertInto("Chapters");

            book2Id = new { Title = "book 2" }.InsertInto("Books");

            new { BookId = book2Id, Name = "Chapter 1" }.InsertInto("Chapters");

            new { BookId = book2Id, Name = "Chapter 2" }.InsertInto("Chapters");
        }

        void specify_it_eager_loads_child_collections_and_caches_them()
        {
            var allBooks = books.All().Include("Chapters");

            new { BookId = book1Id, Name = "Chapter 3" }.InsertInto("Chapters");

            ((int)allBooks.First().Chapters().Count()).should_be(2);

            new { BookId = book2Id, Name = "Chapter 3" }.InsertInto("Chapters");

            ((int)allBooks.Last().Chapters().Count()).should_be(2);
        }
    }
}
