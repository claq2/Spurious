using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Diagnostics;
using Moq;
namespace Importers.DataLayer.UnitTests
{
    [TestFixture]
    public class BulkImportTests
    {
        [Test]
        public void OneIdAndOneNonIdSqlStrings()
        {
            var tempTableFiller = new Mock<INpgTempTableFiller>();
            tempTableFiller.Setup(t => t.Fill("import_temp", "targettable", "id1, field1", It.IsAny<IEnumerable<string>>(), null));

            var issuedSqlCommands = new List<string>();
            var commandRunner = new Mock<INonQueryCommandRunner>();
            commandRunner.Setup(c => c.Execute(It.IsAny<string>())).Callback<string>(c => issuedSqlCommands.Add(c));

            var stopwatch = new Stopwatch();
            var bulkImporter = new BulkImporter(stopwatch, commandRunner.Object, tempTableFiller.Object);

            bulkImporter.BulkImport("spurious",
                "targettable",
                new List<string> { "item" },
                null,
                new List<string> { "id1" },
                new List<string> { "field1" });

            Assert.That(issuedSqlCommands.Count, Is.EqualTo(5));
            Assert.That(issuedSqlCommands[0], Is.EqualTo("create index import_temp_idx on import_temp (id1)"));
            Assert.That(issuedSqlCommands[1], Is.EqualTo("analyze import_temp"));
            Assert.That(issuedSqlCommands[2], Is.EqualTo("delete from targettable t where not exists (select 1 from import_temp it where it.id1 = t.id1)"));
            Assert.That(issuedSqlCommands[3], Is.EqualTo("update targettable t set population = it.population from import_temp it where it.id1 = t.id1 and it.population <> t.population"));
            Assert.That(issuedSqlCommands[4], Is.EqualTo("insert into targettable (id1, population) select it.id1, it.population from import_temp it left join targettable t using (id1) where t.id1 is null"));
        }
    }
}
