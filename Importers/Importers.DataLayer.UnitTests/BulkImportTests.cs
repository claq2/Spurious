using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Diagnostics;

namespace Importers.DataLayer.UnitTests
{
    [TestFixture]
    public class BulkImportTests
    {
        [Test]
        public void OneIdAndOneNonIdSqlStrings()
        {
            var stopwatch = new Stopwatch();
            var commandRunner = new TestNonQueryCommandRunner();
            var bulkImporter = new BulkImporter(stopwatch, commandRunner);

            bulkImporter.BulkImport("spurious",
                "targettable",
                new List<string> { "item" },
                null,
                new List<string> { "id1" },
                new List<string> { "field1" });
        }

        private class TestNonQueryCommandRunner : INonQueryCommandRunner
        {
            public List<string> SqlCommands { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="TestNonQueryCommandRunner"/> class.
            /// </summary>
            public TestNonQueryCommandRunner()
            {
                this.SqlCommands = new List<string>();
            }

            public IDbConnection Connection { get; set; }

            public int Timeout
            {
                get { return 9001; }
            }

            public int Execute(string commandText)
            {
                this.SqlCommands.Add(commandText);
                return 1;
            }
        }

    }
}
