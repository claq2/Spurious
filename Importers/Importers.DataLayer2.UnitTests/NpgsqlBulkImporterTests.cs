using Importers.Datalayer2;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using System.Data;

namespace Importers.DataLayer2.UnitTests
{
    [TestFixture]
    public class NpgsqlBulkImporterTests
    {
        [Test]
        public void OneIdOneNonIdBulkImport()
        {
            var items = new List<ItemWithOneIdOneNonId>
            {
                new ItemWithOneIdOneNonId { Id = 1234, Total = 44.5M },
                new ItemWithOneIdOneNonId { Id = 1235, Total = 123.9M }
            };

            var itemCollection = new ItemWithOneIdOneNonIdCollection
            {
                Items = items
            };

            var conn = new Mock<IDbConnection>();
            conn.Setup(c => c.Open());

            var issuedSqlCommands = new List<string>();

            var wrapper = new Mock<INpgsqlConnectionWrapper>();
            wrapper.Setup(w => w.Connection).Returns(conn.Object);
            wrapper.Setup(w => w.ExecuteNonQuery(It.IsAny<string>(), 9001))
                .Callback<string, int>((c, t) => issuedSqlCommands.Add(c))
                .Returns(0);

            var tableFiller = new Mock<INpgsqlTempTableFiller>();
            tableFiller.Setup(tf => tf.Fill("import_temp", "targetTable", itemCollection));

            var importer = new NpgsqlBulkImporter(wrapper.Object, tableFiller.Object);
            importer.BulkImport("targetTable", itemCollection);

            Assert.That(issuedSqlCommands.Count, Is.EqualTo(5));
            Assert.That(issuedSqlCommands[0], Is.EqualTo("create index import_temp_idx on import_temp (id)"));
            Assert.That(issuedSqlCommands[1], Is.EqualTo("analyze import_temp"));
            Assert.That(issuedSqlCommands[2], Is.EqualTo("delete from targetTable t where not exists (select 1 from import_temp it where it.id = t.id)"));
            Assert.That(issuedSqlCommands[3], Is.EqualTo("update targetTable t set (total) = (it.total) from import_temp it where it.id = t.id and (it.total is distinct from t.total)"));
            Assert.That(issuedSqlCommands[4], Is.EqualTo("insert into targetTable (id, total) select it.id, it.total from import_temp it left join targetTable t using (id) where t.id is null"));
            tableFiller.VerifyAll();
        }

        [Test]
        public void TwoIdsTwoNonIdsBulkImport()
        {
            var items = new List<ItemWithTwoIdsTwoNonIds>
            {
                new ItemWithTwoIdsTwoNonIds { Id1 = 1234, Id2 = 5, Total1 = 44.5M, Total2 = 12M },
                new ItemWithTwoIdsTwoNonIds { Id1 = 1235, Id2 = 9, Total1 = 123.9M, Total2 = 14M }
            };

            var itemCollection = new ItemWithTwoIdsTwoNonIdsCollection
            {
                Items = items
            };

            var conn = new Mock<IDbConnection>();
            conn.Setup(c => c.Open());

            var issuedSqlCommands = new List<string>();

            var wrapper = new Mock<INpgsqlConnectionWrapper>();
            wrapper.Setup(w => w.Connection).Returns(conn.Object);
            wrapper.Setup(w => w.ExecuteNonQuery(It.IsAny<string>(), 9001))
                .Callback<string, int>((c, t) => issuedSqlCommands.Add(c))
                .Returns(0);

            var tableFiller = new Mock<INpgsqlTempTableFiller>();
            tableFiller.Setup(tf => tf.Fill("import_temp", "targetTable", itemCollection));

            var importer = new NpgsqlBulkImporter(wrapper.Object, tableFiller.Object);
            importer.BulkImport("targetTable", itemCollection);

            Assert.That(issuedSqlCommands.Count, Is.EqualTo(5));
            Assert.That(issuedSqlCommands[0], Is.EqualTo("create index import_temp_idx on import_temp (id1, id2)"));
            Assert.That(issuedSqlCommands[1], Is.EqualTo("analyze import_temp"));
            Assert.That(issuedSqlCommands[2], Is.EqualTo("delete from targetTable t where not exists (select 1 from import_temp it where it.id1 = t.id1 and it.id2 = t.id2)"));
            Assert.That(issuedSqlCommands[3], Is.EqualTo("update targetTable t set (total1, total2) = (it.total1, it.total2) from import_temp it where it.id1 = t.id1 and it.id2 = t.id2 and (it.total1 is distinct from t.total1 or it.total2 is distinct from t.total2)"));
            Assert.That(issuedSqlCommands[4], Is.EqualTo("insert into targetTable (id1, id2, total1, total2) select it.id1, it.id2, it.total1, it.total2 from import_temp it left join targetTable t using (id1, id2) where t.id1 is null"));
            tableFiller.VerifyAll();
        }

        private class ItemWithOneIdOneNonId : IItem
        {
            public int Id { get; set; }
            public decimal Total { get; set; }
            public string IdAndDataFieldsAsCsv { get { return string.Format("{0},{1}", this.Id, Convert.ToInt32(this.Total)); } }
        }

        private class ItemWithOneIdOneNonIdCollection : IItemCollection<ItemWithOneIdOneNonId>
        {
            public List<string> DbDataFields
            {
                get { return new List<string> { "total" }; }
            }

            public List<string> DbIdFields
            {
                get { return new List<string> { "id" }; }
            }

            public IEnumerable<ItemWithOneIdOneNonId> Items { get; set; }
        }

        private class ItemWithTwoIdsTwoNonIdsCollection : IItemCollection<ItemWithTwoIdsTwoNonIds>
        {
            public List<string> DbDataFields
            {
                get { return new List<string> { "total1", "total2" }; }
            }

            public List<string> DbIdFields
            {
                get { return new List<string> { "id1", "id2" }; }
            }

            public IEnumerable<ItemWithTwoIdsTwoNonIds> Items { get; set; }
        }

        private class ItemWithTwoIdsTwoNonIds : IItem
        {
            public int Id1 { get; set; }

            public int Id2 { get; set; }

            public string IdAndDataFieldsAsCsv { get { return string.Format("{0},{1},{2},{3}", this.Id1, this.Id2, Convert.ToInt32(this.Total1), Convert.ToInt32(this.Total2)); } }

            public decimal Total1 { get; set; }

            public decimal Total2 { get; set; }
        }
    }
}
