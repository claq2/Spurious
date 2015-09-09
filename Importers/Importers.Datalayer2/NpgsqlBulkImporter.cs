using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Importers.Datalayer2
{
    public class NpgsqlBulkImporter
    {
        INpgsqlConnectionWrapper wrapper;

        INpgsqlTempTableFiller tempTableFiller;

        Stopwatch stopwatch = new Stopwatch();

        public NpgsqlBulkImporter(string connectionString) : this(new NpgsqlConnectionWrapper(connectionString), new NpgsqlTempTableFiller())
        {
            
        }

        public NpgsqlBulkImporter(INpgsqlConnectionWrapper wrapper, INpgsqlTempTableFiller filler)
        {
            this.wrapper = wrapper;
            this.tempTableFiller = filler;
            this.tempTableFiller.Wrapper = wrapper;
            stopwatch.Start();
        }

        /// <summary>
        /// Performs a bulk insert/update/delete based on http://dba.stackexchange.com/questions/73066/efficient-way-to-insert-update-delete-table-records-from-complex-query-in-postgr
        /// </summary>
        public void BulkImport<T>(string targetTable, IEnumerable<T> itemsToImport) where T : IItem
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

                wrapper.ExecuteNonQuery(string.Format("create index {1}_idx on {1} ({0})", idsCsv, tempTableName));
                wrapper.ExecuteNonQuery(string.Format("analyze {0}", tempTableName));

                StringBuilder idMatchClause = new StringBuilder(string.Format("it.{0} = t.{0}", itemsToImport.First().DbIdFields[0]));
                itemsToImport.First().DbIdFields.Skip(1).ToList().ForEach((id) =>
                {
                    idMatchClause.AppendFormat(" and it.{0} = t.{0}", id);
                });

                var rowsDeleted = wrapper.ExecuteNonQuery(string.Format(@"delete from {0} t where not exists (select 1 from {2} it where {1})",
                                                            targetTable,
                                                            idMatchClause,
                                                            tempTableName));
                Console.WriteLine("Deleted {0} rows", rowsDeleted);

                // TODO: use non ID fields from params
                Func<string, string> formatWithItPrefix = id => string.Format("it.{0}", id);
                var nonIdsCsvWithItPrefix = string.Join(", ", itemsToImport.First().DbDataFields.Select(formatWithItPrefix));

                StringBuilder nonIdNotMatchClause = new StringBuilder(string.Format("it.{0} <> t.{0}", itemsToImport.First().DbDataFields[0]));
                itemsToImport.First().DbDataFields.Skip(1).ToList().ForEach((id) =>
                {
                    nonIdNotMatchClause.AppendFormat(" and it.{0} <> t.{0}", id);
                });

                var updatedRows = wrapper.ExecuteNonQuery(string.Format(@"update {0} t set ({3}) = ({4}) from {2} it where {1} and {5}",
                                                            targetTable,
                                                            idMatchClause,
                                                            tempTableName,
                                                            nonIdsCsv,
                                                            nonIdsCsvWithItPrefix,
                                                            nonIdNotMatchClause));
                Console.WriteLine("Updated {0} rows", updatedRows);
            }
        }
    }
}
