using Importers.Datalayer2;
using Moq;
using Npgsql;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Importers.DataLayer2.UnitTests
{
    [TestFixture]
    public class NpgsqlTempTableFillerTests
    {
        [Test]
        public void FillRealTempTable()
        {
            using (INpgsqlConnectionWrapper conn = new NpgsqlConnectionWrapper(ConfigurationManager.ConnectionStrings["spurious"].ConnectionString))
            {
                conn.Open();
                INpgsqlTempTableFiller filler = new NpgsqlTempTableFiller() { Wrapper = conn };

                var items = new List<PopulationItem>();
                items.Add(new PopulationItem { GeoCode = 12345, Total = 456.7M });
                items.Add(new PopulationItem { GeoCode = 12346, Total = 78.1M });

                var itemCollection = new PopulationItemCollection { Items = items };

                filler.Fill("temp_test", "subdivisions", itemCollection);

                var cmd = conn.CreateCommand();
                cmd.CommandText = "select id, population from temp_test";
                var reader = cmd.ExecuteReader();
                var fromDb = Enumerable.Empty<object>().Select(o => new { id = 0, population = 0 }).ToList();
                while (reader.Read())
                {
                    fromDb.Add(new { id = Convert.ToInt32(reader["id"]), population = Convert.ToInt32(reader["population"]) });
                }

                Assert.That(fromDb[0].id, Is.EqualTo(12345));
                Assert.That(fromDb[0].population, Is.EqualTo(457)); // Rounded up

                Assert.That(fromDb[1].id, Is.EqualTo(12346));
                Assert.That(fromDb[1].population, Is.EqualTo(78));
            }
        }

        [Test]
        public void FillTempTableOneIdOneField()
        {
            var items = new List<PopulationItem>();
            items.Add(new PopulationItem { GeoCode = 12345, Total = 456.7M });
            items.Add(new PopulationItem { GeoCode = 12346, Total = 78.1M });

            var itemCollection = new PopulationItemCollection { Items = items };

            var conn = new Mock<IDbConnection>();
            conn.Setup(c => c.Open());

            var writtenLines = new List<string>();
            var writer = new Mock<INpgsqlCopyTextWriterWrapper>();
            writer.Setup(w => w.Write(It.IsAny<string>()))
                .Callback<string>((s) => writtenLines.Add(s));

            var issuedSqlCommands = new List<string>();
            var wrapper = new Mock<INpgsqlConnectionWrapper>();
            wrapper.Setup(w => w.Connection).Returns(conn.Object);
            wrapper.Setup(w => w.ExecuteNonQuery(It.IsAny<string>(), 9001))
                .Callback<string, int>((c, t) => issuedSqlCommands.Add(c))
                .Returns(0);
            wrapper.Setup(w => w.BeginTextImport(It.IsAny<string>()))
                .Callback<string>((c) => issuedSqlCommands.Add(c))
                .Returns(writer.Object);

            var filler = new NpgsqlTempTableFiller(wrapper.Object);
            filler.Fill("tempTable", "prototypeTable", itemCollection);

            Assert.That(issuedSqlCommands.Count, Is.EqualTo(2));
            Assert.That(issuedSqlCommands[0], Is.EqualTo("create temp table tempTable as (select * from prototypeTable where 0 = 1)"));
            Assert.That(issuedSqlCommands[1], Is.EqualTo("copy tempTable (id, population) from stdin with csv"));
            Assert.That(writtenLines.Count, Is.EqualTo(4)); // 2 CSV lines and 2 \n
            Assert.That(writtenLines[0], Is.EqualTo("12345, 457"));
            Assert.That(writtenLines[1], Is.EqualTo("\n"));
            Assert.That(writtenLines[2], Is.EqualTo("12346, 78"));
            Assert.That(writtenLines[3], Is.EqualTo("\n"));
        }

        [Test]
        public void FillTempTableTwoIdsTwoFields()
        {
            var items = new List<TwoIdTwoFieldItem>();
            items.Add(new TwoIdTwoFieldItem { Id1 = 12345, Id2 = 12, Field1 = 456.7M, Field2 = 555 });
            items.Add(new TwoIdTwoFieldItem { Id1 = 12346, Id2 = 22, Field1 = 444, Field2 = 78.1M });

            var itemCollection = new TwoIdTwoFieldItemCollection { Items = items };

            var conn = new Mock<IDbConnection>();
            conn.Setup(c => c.Open());

            var writtenLines = new List<string>();
            var writer = new Mock<INpgsqlCopyTextWriterWrapper>();
            writer.Setup(w => w.Write(It.IsAny<string>()))
                .Callback<string>((s) => writtenLines.Add(s));

            var issuedSqlCommands = new List<string>();
            var wrapper = new Mock<INpgsqlConnectionWrapper>();
            wrapper.Setup(w => w.Connection).Returns(conn.Object);
            wrapper.Setup(w => w.ExecuteNonQuery(It.IsAny<string>(), 9001))
                .Callback<string, int>((c, t) => issuedSqlCommands.Add(c))
                .Returns(0);
            wrapper.Setup(w => w.BeginTextImport(It.IsAny<string>()))
                .Callback<string>((c) => issuedSqlCommands.Add(c))
                .Returns(writer.Object);

            var filler = new NpgsqlTempTableFiller(wrapper.Object);
            filler.Fill("tempTable", "prototypeTable", itemCollection);

            Assert.That(issuedSqlCommands.Count, Is.EqualTo(2));
            Assert.That(issuedSqlCommands[0], Is.EqualTo("create temp table tempTable as (select * from prototypeTable where 0 = 1)"));
            Assert.That(issuedSqlCommands[1], Is.EqualTo("copy tempTable (id1, id2, field1, field2) from stdin with csv"));
            Assert.That(writtenLines.Count, Is.EqualTo(4)); // 2 CSV lines and 2 \n
            Assert.That(writtenLines[0], Is.EqualTo("12345, 12, 457, 555"));
            Assert.That(writtenLines[1], Is.EqualTo("\n"));
            Assert.That(writtenLines[2], Is.EqualTo("12346, 22, 444, 78"));
            Assert.That(writtenLines[3], Is.EqualTo("\n"));
        }

        private class PopulationItemCollection : IItemCollection<PopulationItem>
        {
            public List<string> DbDataFields
            {
                get { return new List<string> { "population" }; }
            }

            public List<string> DbIdFields
            {
                get { return new List<string> { "id" }; }
            }

            public IEnumerable<PopulationItem> Items { get; set; }
        }

        private class PopulationItem : IItem
        {
            public int GeoCode { get; set; }

            public string IdAndDataFieldsAsCsv { get { return string.Format("{0}, {1}", this.GeoCode, Convert.ToInt32(this.Total)); } }

            public decimal Total { get; set; }
        }

        private class TwoIdTwoFieldItemCollection : IItemCollection<TwoIdTwoFieldItem>
        {
            public List<string> DbDataFields
            {
                get { return new List<string> { "field1, field2" }; }
            }

            public List<string> DbIdFields
            {
                get { return new List<string> { "id1, id2" }; }
            }

            public IEnumerable<TwoIdTwoFieldItem> Items { get; set; }
        }

        private class TwoIdTwoFieldItem : IItem
        {
            public int Id1 { get; set; }

            public int Id2 { get; set; }

            public string IdAndDataFieldsAsCsv { get { return $"{Id1}, {Id2}, {Convert.ToInt32(this.Field1)}, {Convert.ToInt32(this.Field2)}"; } }

            public decimal Field1 { get; set; }

            public decimal Field2 { get; set; }
        }
    }
}