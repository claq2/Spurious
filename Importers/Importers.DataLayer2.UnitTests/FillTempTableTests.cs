using Importers.Datalayer2;
using Npgsql;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Importers.DataLayer2.UnitTests
{
    [TestFixture]
    public class FillTempTableTests
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

                filler.Fill("temp_test", "subdivisions", items);

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