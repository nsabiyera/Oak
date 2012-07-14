using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Massive;

namespace Oak.Tests
{
    public class Records : DynamicRepository { }

    class describe_DynamicRepository : nspec
    {
        Seed seed;

        Records records;

        object recordToInsert;

        void before_each()
        {
            seed = new Seed();

            records = new Records();

            seed.PurgeDb();
        }

        void inserting_a_record_that_has_non_value_type_properties()
        {
            before = () =>
            {
                seed.CreateTable("Records", new dynamic[] 
                { 
                   seed.Id(),
                   new { Name = "nvarchar(255)" }
                }).ExecuteNonQuery();

                recordToInsert = new { Name = "foo", NotAValueType = new { Name = "bar" } };
            };

            act = () => records.Insert(recordToInsert);

            it["the record is inserted (ignoring the reference type)"] = () =>
            {
                var record = records.All().First();

                ((string)record.Name).should_be("foo");
            };
        }

        void updating_every_type_of_sql_column()
        {
            before = () =>
            {
                var sql = seed.CreateTable("Records", new dynamic[] 
                { 
                   new { BigIntColumn = "bigint" },
                   new { BinaryColumn = "binary(4)" },
                   new { BitColumn = "bit" },
                   new { CharColumn = "char(11)" },
                   new { DateColumn = "date" },
                   new { DateTimeColumn = "datetime" },
                   new { DateTimeTwoColumn = "datetime2(7)" },
                   new { DateTimeOffSetColumn = "datetimeoffset(7)" },
                   new { DecimalColumn = "decimal(18, 2)" },
                   new { FloatColumn = "float" },
                   //new { GeographyColumn = "geography" },
                   //new { GeographyColumn = "geometry" },
                   //new { HierarchyIdColumn = "hierarchyid" }
                   new { ImageColumn = "image" },
                   new { IntColumn = "int" },
                   new { MoneyColumn = "money" },
                   new { NTextColumn = "ntext" },
                   new { NumericColumn = "numeric(18, 5)" },
                   new { NCharColumn = "nchar(12)" },
                   new { NVarCharColumn = "varchar(15)" },
                   new { NVarCharMaxColumn = "varchar(max)" },
                   new { RealColumn = "real" },
                   new { SmallDateTimeColumn = "smalldatetime" },
                   new { SmallIntColumn = "smallint" },
                   new { SmallMoneyColumn = "smallmoney" },
                   new { SqlVariantColumn = "sql_variant" },
                   new { TextColumn = "text" },
                   new { TimeColumn = "time(7)" },
                   new { TimeStampColumn = "timestamp" },
                   new { TinyIntColumn = "tinyint" },
                   new { UniqueIdentifierColumn = "uniqueidentifier" },
                   new { VarBinaryColumn = "varbinary(50)" },
                   new { VarBinaryMaxColumn = "varbinary(max)" },
                   new { VarCharColumn = "varchar(50)" },
                   new { VarCharMaxColumn = "varchar(max)" },
                   new { XmlColumn = "xml" }
                });

                sql.ExecuteNonQuery();

                recordToInsert = new
                {
                    BigIntColumn = 10,
                    BinaryColumn = new byte[] { 1, 2, 3, 4 },
                    BitColumn = true,
                    CharColumn = "char column",
                    DateColumn = DateTime.Today,
                    DateTimeColumn = DateTime.Today.AddDays(1).AddHours(1),
                    DateTimeTwoColumn = DateTime.Today.AddDays(2).AddHours(2),
                    DateTimeOffSetColumn = new DateTimeOffset(DateTime.Today.AddDays(3).AddHours(3)),
                    DecimalColumn = 10.05,
                    FloatColumn = (float)15.002,
                    ImageColumn = new byte[] { 2, 3, 4, 5 },
                    IntColumn = 16,
                    MoneyColumn = 9.99,
                    NCharColumn = "nchar column",
                    NTextColumn = "ntext column",
                    NumericColumn = 18.003,
                    NVarCharColumn = "nvarchar column",
                    NVarCharMaxColumn = "nvarchar(max) column",
                    RealColumn = 1.9,
                    SqlVariantColumn = 62,
                    SmallDateTimeColumn = DateTime.Today.AddDays(4),
                    SmallIntColumn = 1,
                    SmallMoneyColumn = .99,
                    TextColumn = "text column",
                    TimeColumn = DateTime.Today.AddHours(1),
                    TinyIntColumn = 2,
                    UniqueIdentifierColumn = Guid.Empty,
                    VarBinaryColumn = new byte[] { 5, 6, 7, 8 },
                    VarBinaryMaxColumn = new byte[] { 10, 11, 12, 13 },
                    VarCharColumn = "varchar column",
                    VarCharMaxColumn = "varcharmax column",
                    XmlColumn = "<person><first>Amir</first><last>Rajan</last></person>"
                };
            };

            act = () => records.Insert(recordToInsert);

            it["each column is updated"] = () =>
            {
                var record = records.All().First();

                ((long)record.BigIntColumn).should_be(10);
                ((object)record.BinaryColumn).should_be(new byte[] { 1, 2, 3, 4 });
                ((bool)record.BitColumn).should_be(true);
                ((string)record.CharColumn).should_be("char column");
                ((DateTime)record.DateColumn).should_be(DateTime.Today);
                ((DateTime)record.DateTimeColumn).should_be(DateTime.Today.AddDays(1).AddHours(1));
                ((DateTime)record.DateTimeTwoColumn).should_be(DateTime.Today.AddDays(2).AddHours(2));
                ((DateTimeOffset)record.DateTimeOffSetColumn).should_be(new DateTimeOffset(DateTime.Today.AddDays(3).AddHours(3)));
                ((Decimal)record.DecimalColumn).should_be(10.05);
                ((int)Convert.ToInt32(record.FloatColumn)).should_be(15);
                ((object)record.ImageColumn).should_be(new byte[] { 2, 3, 4, 5 });
                ((int)record.IntColumn).should_be(16);
                ((decimal)record.MoneyColumn).should_be(9.99);
                ((string)record.NCharColumn).should_be("nchar column");
                ((string)record.NTextColumn).should_be("ntext column");
                ((decimal)record.NumericColumn).should_be(18.003);
                ((string)record.NCharColumn).should_be("nchar column");
                ((string)record.NVarCharColumn).should_be("nvarchar column");
                ((string)record.NVarCharMaxColumn).should_be("nvarchar(max) column");
                ((Single)Convert.ToInt32(record.RealColumn)).should_be(2);
                ((object)record.SqlVariantColumn).should_be(62);
                ((DateTime)record.SmallDateTimeColumn).should_be(DateTime.Today.AddDays(4));
                ((Int16)record.SmallIntColumn).should_be(1);
                ((Decimal)record.SmallMoneyColumn).should_be(.99);
                ((string)record.TextColumn).should_be("text column");
                ((TimeSpan)record.TimeColumn).should_be(DateTime.Today.AddHours(1) - DateTime.Today);
                ((object)record.TimeStampColumn).GetType().should_be(typeof(byte[]));
                ((byte)record.TinyIntColumn).should_be(2);
                ((Guid)record.UniqueIdentifierColumn).should_be(Guid.Empty);
                ((object)record.VarBinaryColumn).should_be(new byte[] { 5, 6, 7, 8 });
                ((object)record.VarBinaryMaxColumn).should_be(new byte[] { 10, 11, 12, 13 });
                ((string)record.VarCharColumn).should_be("varchar column");
                ((string)record.VarCharMaxColumn).should_be("varcharmax column");
                ((string)record.XmlColumn).should_be("<person><first>Amir</first><last>Rajan</last></person>");
            };
        }
    }
}
