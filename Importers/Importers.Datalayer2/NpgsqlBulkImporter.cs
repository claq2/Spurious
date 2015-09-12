using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Importers.DataLayer
{
    public class NpgsqlBulkImporter
    {
        private readonly INpgsqlConnectionWrapper wrapper;

        private readonly INpgsqlTempTableFiller tempTableFiller;

        private readonly Stopwatch stopwatch = new Stopwatch();

        public NpgsqlBulkImporter(string connectionString, Stopwatch stopwatch = null)
            : this(new NpgsqlConnectionWrapper(connectionString), new NpgsqlTempTableFiller())
        {
            if (stopwatch != null)
            {
                this.stopwatch = stopwatch;
            }
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
        public void BulkImport<T>(string targetTable, IItemCollection<T> itemCollectionToImport) where T : IItem
        {
            var tempTableName = "import_temp";
            var idsCsv = string.Join(", ", itemCollectionToImport.DbIdFields);
            var nonIdsCsv = string.Join(", ", itemCollectionToImport.DbDataFields);

            using (var conn = wrapper.Connection)
            {
                conn.Open();
                this.tempTableFiller.Fill(tempTableName, targetTable, itemCollectionToImport);
                Console.WriteLine($"Finished temp table fill after {stopwatch.Elapsed}");

                wrapper.ExecuteNonQuery(string.Format("create index {1}_idx on {1} ({0})", idsCsv, tempTableName));
                wrapper.ExecuteNonQuery(string.Format("analyze {0}", tempTableName));

                StringBuilder idMatchClause = new StringBuilder(string.Format("it.{0} = t.{0}", itemCollectionToImport.DbIdFields[0]));
                itemCollectionToImport.DbIdFields.Skip(1).ToList().ForEach((id) =>
                {
                    idMatchClause.AppendFormat(" and it.{0} = t.{0}", id);
                });

                var rowsDeleted = wrapper.ExecuteNonQuery(string.Format(@"delete from {0} t where not exists (select 1 from {2} it where {1})",
                                                            targetTable,
                                                            idMatchClause,
                                                            tempTableName));
                Console.WriteLine($"Deleted {rowsDeleted} rows after {stopwatch.Elapsed}");

                Func<string, string> formatWithItPrefix = id => string.Format("it.{0}", id);
                var nonIdsCsvWithItPrefix = string.Join(", ", itemCollectionToImport.DbDataFields.Select(formatWithItPrefix));

                StringBuilder nonIdNotMatchClause = new StringBuilder(string.Format("it.{0} is distinct from t.{0}", itemCollectionToImport.DbDataFields[0]));
                itemCollectionToImport.DbDataFields.Skip(1).ToList().ForEach((id) =>
                {
                    nonIdNotMatchClause.AppendFormat(" or it.{0} is distinct from t.{0}", id);
                });

                var updatedRows = wrapper.ExecuteNonQuery(string.Format(@"update {0} t set ({3}) = ({4}) from {2} it where {1} and ({5})",
                                                            targetTable,
                                                            idMatchClause,
                                                            tempTableName,
                                                            nonIdsCsv,
                                                            nonIdsCsvWithItPrefix,
                                                            nonIdNotMatchClause));
                Console.WriteLine($"Updated {updatedRows} rows after {stopwatch.Elapsed}");

                var idsCsvWithItPrefix = string.Join(", ", itemCollectionToImport.DbIdFields.Select(formatWithItPrefix));
                var insertedRows = wrapper.ExecuteNonQuery(string.Format(@"insert into {0} ({2}, {5}) select {3}, {6} from {4} it left join {0} t using ({2}) where t.{1} is null",
                                                             targetTable,
                                                             itemCollectionToImport.DbIdFields.First(),
                                                             idsCsv,
                                                             idsCsvWithItPrefix,
                                                             tempTableName,
                                                             nonIdsCsv,
                                                             nonIdsCsvWithItPrefix));
                Console.WriteLine($"Added {insertedRows} rows after {stopwatch.Elapsed}");
            }
        }
    }
}
