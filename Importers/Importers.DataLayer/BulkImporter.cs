using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Importers.DataLayer
{
    public class BulkImporter
    {
        private readonly INonQueryCommandRunner commandRunner;
        private readonly Stopwatch stopwatch;
        private readonly INpgTempTableFiller tempTableFiller;

        /// <summary>
        /// Initializes a new instance of the <see cref="BulkImporter"/> class.
        /// </summary>
        public BulkImporter(Stopwatch stopwatch) : this(stopwatch, new NonQueryCommandRunner(), new NpgsqlTempTableFiller(stopwatch))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BulkImporter"/> class.
        /// </summary>
        /// <param name="commandRunner"></param>
        public BulkImporter(Stopwatch stopwatch, INonQueryCommandRunner commandRunner, INpgTempTableFiller tempTableFiller)
        {
            this.stopwatch = stopwatch;
            this.commandRunner = commandRunner;
            this.tempTableFiller = tempTableFiller;
        }

        /// <summary>
        /// Performs a bulk insert/update/delete based on http://dba.stackexchange.com/questions/73066/efficient-way-to-insert-update-delete-table-records-from-complex-query-in-postgr
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="databaseName"></param>
        /// <param name="targetTable"></param>
        /// <param name="itemsToImport"></param>
        /// <param name="addItemToSerializer"></param>
        public void BulkImport<T>(string databaseName, 
            string targetTable, 
            IEnumerable<T> itemsToImport, 
            Action<T, NpgsqlCopySerializer> addItemToSerializer, 
            List<string> idFieldsToImport = null, 
            List<string> nonIdFieldsToImport = null)
        {
            var tempTableName = "import_temp";
            var idsCsv = string.Join(", ", idFieldsToImport.ToArray());
            var nonIdsCsv = string.Join(", ", nonIdFieldsToImport.ToArray());
            var allFieldsToImport = string.Format("{0}, {1}", idsCsv, string.Join(", ", nonIdFieldsToImport));

            using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings[databaseName].ConnectionString))
            {
                this.commandRunner.Connection = conn;
                this.tempTableFiller.Connection = conn;
                conn.Open();

                this.tempTableFiller.Fill(tempTableName, targetTable, allFieldsToImport, itemsToImport, addItemToSerializer);
                Console.WriteLine("Finished temp table fill after {0}", stopwatch.Elapsed);

                commandRunner.Execute(string.Format("create index {1}_idx on {1} ({0})", idsCsv, tempTableName));
                commandRunner.Execute(string.Format("analyze {0}", tempTableName));

                StringBuilder idMatchClause = new StringBuilder(string.Format("it.{0} = t.{0}", idFieldsToImport[0]));
                idFieldsToImport.Skip(1).ToList().ForEach((id) =>
                {
                    idMatchClause.AppendFormat(" and it.{0} = t.{0}", id);
                });

                var rowsDeleted = commandRunner.Execute(string.Format(@"delete from {0} t where not exists (select 1 from {2} it where {1})", 
                                                            targetTable, 
                                                            idMatchClause, 
                                                            tempTableName));
                Console.WriteLine("Deleted {0} rows", rowsDeleted);

                // TODO: use non ID fields from params
                //nonI
                var updatedRows = commandRunner.Execute(string.Format(@"update {0} t set population = it.population from {2} it where {1} and it.population <> t.population", 
                                                            targetTable, 
                                                            idMatchClause, 
                                                            tempTableName));
                Console.WriteLine("Updated {0} rows", updatedRows);

                // TODO: use non ID fields from params
                // insertCmd.CommandText = @"insert into inventories (product_id, store_id, quantity)
                //                             select x.product_id, x.store_id, x.quantity
                //                             from inv_temp x
                //                             left join inventories i using (product_id, store_id)
                //                             where i.product_id is null";
                Func<string, string> formatWithItPrefix = id => string.Format("it.{0}", id);
                var idsCsvWithItPrefix = string.Join(", ", idFieldsToImport.Select(formatWithItPrefix));
                var nonIdsCsvWithItPrefix = string.Join(", ", nonIdFieldsToImport.Select(formatWithItPrefix));
                var insertedRows = commandRunner.Execute(string.Format(@"insert into {0} ({2}, {5}) select {3}, {6} from {4} it left join {0} t using ({2}) where t.{1} is null", 
                                                             targetTable, 
                                                             idFieldsToImport.First(), 
                                                             idsCsv, 
                                                             idsCsvWithItPrefix, 
                                                             tempTableName,
                                                             nonIdsCsv,
                                                             nonIdsCsvWithItPrefix));
                Console.WriteLine("Added {0} rows", insertedRows);
            }
        }
    }
}