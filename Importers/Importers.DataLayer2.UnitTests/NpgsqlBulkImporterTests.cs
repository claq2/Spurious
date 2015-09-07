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
        public void Test()
        {
            var conn = new Mock<IDbConnection>();
            conn.Setup(c => c.Open());
            var wrapper = new Mock<INpgsqlConnectionWrapper>(MockBehavior.Strict);
            wrapper.Setup(w => w.Connection).Returns(conn.Object);
            wrapper.Setup(w => w.ExecuteNonQuery("create index import_temp_idx on import_temp (id)", 9001)).Returns(0);
            wrapper.Setup(w => w.ExecuteNonQuery("analyze import_temp", 9001)).Returns(0);
            var tableFiller = new Mock<INpgsqlTempTableFiller>();
            var importer = new NpgsqlBulkImporter(wrapper.Object, tableFiller.Object);
            var items = new List<PopulationItem>
            {
                new PopulationItem { GeoCode = 1234, Total = 44.5M },
                new PopulationItem { GeoCode = 1235, Total = 123.9M }
            };

            importer.BulkImport("targetTable", items);
        }

        private class PopulationItem : IItem
        {
            public List<string> DbDataFields
            {
                get { return new List<string> { "population" }; }
            }

            public List<string> DbIdFields
            {
                get { return new List<string> { "id" }; }
            }

            public int GeoCode { get; set; }

            public string IdAndDataFieldsAsCsv { get { return string.Format("{0},{1}", this.GeoCode, Convert.ToInt32(this.Total)); } }

            public decimal Total { get; set; }
        }

    }
}
