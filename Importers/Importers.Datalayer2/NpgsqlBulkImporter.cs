using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace Importers.Datalayer2
{
    class NpgsqlBulkImporter
    {
        INpgsqlConnectionWrapper wrapper;

        INpgsqlTempTableFiller tempTableFiller;

        Stopwatch stopwatch;

        public NpgsqlBulkImporter(string connectionString) : this(new NpgsqlConnectionWrapper(connectionString), new NpgsqlTempTableFiller())
        {

        }

        public NpgsqlBulkImporter(INpgsqlConnectionWrapper wrapper, INpgsqlTempTableFiller filler)
        {
            this.wrapper = wrapper;
            this.tempTableFiller = filler;
            this.tempTableFiller.Connection = wrapper;
        }

        public void BulkImport<T>(string databaseName, string targetTable, IEnumerable<T> itemsToImport) where T : IItem
        {
            var tempTableName = "import_temp";
            var idsCsv = string.Join(", ", itemsToImport.First().DbIdFields);
            var nonIdsCsv = string.Join(", ", itemsToImport.First().DbDataFields);
            var allFieldsToImport = itemsToImport.First().IdAndDataFieldsAsCsv;

            using (var conn = wrapper.Connection)
            {
                conn.Open();
                this.tempTableFiller.Fill(tempTableName, targetTable, itemsToImport);
                Console.WriteLine("Finished temp table fill after {0}", stopwatch.Elapsed);

                conn.Execute(string.Format("create index {1}_idx on {1} ({0})", idsCsv, tempTableName), commandTimeout: 90);
                conn.Execute(string.Format("analyze {0}", tempTableName), commandTimeout: 90);
            }
        }
    }
}
