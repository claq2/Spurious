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

        /// <summary>
        /// Initializes a new instance of the <see cref="BulkImporter"/> class.
        /// </summary>
        public BulkImporter(Stopwatch stopwatch) : this(new NonQueryCommandRunner(), stopwatch)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BulkImporter"/> class.
        /// </summary>
        /// <param name="commandRunner"></param>
        public BulkImporter(INonQueryCommandRunner commandRunner, Stopwatch stopwatch)
        {
            this.commandRunner = commandRunner;
            this.stopwatch = stopwatch;
        }

        /// <summary>
        /// Performs a bulk insert/update/delete based on http://dba.stackexchange.com/questions/73066/efficient-way-to-insert-update-delete-table-records-from-complex-query-in-postgr
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="databaseName"></param>
        /// <param name="targetTable"></param>
        /// <param name="itemsToImport"></param>
        /// <param name="addItemToSerializer"></param>
        public void BulkImport<T>(string databaseName, string targetTable, IEnumerable<T> itemsToImport, Action<T, NpgsqlCopySerializer> addItemToSerializer, List<string> idFieldsToImport = null, List<string> nonIdFieldsToImport = null)
        {
            var idsCsv = string.Join(", ", idFieldsToImport.ToArray());
            var allFieldsToImport = string.Format("{0}, {1}", idsCsv, string.Join(", ", nonIdFieldsToImport));

            // TODO: Turn fieldsToPopulate into List<string>
            using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings[databaseName].ConnectionString))
            {
                this.commandRunner.Connection = conn;
                conn.Open();
                commandRunner.Execute(string.Format("create temp table import_temp as (select * from {0} where 0 = 1)",
                        targetTable));

                var copyCmd = conn.CreateCommand();
                copyCmd.CommandTimeout = commandRunner.Timeout;
                copyCmd.CommandText = string.Format("copy import_temp({0}) from stdin", allFieldsToImport);
                var serializer = new NpgsqlCopySerializer(conn);
                var copyIn = new NpgsqlCopyIn(copyCmd, conn, serializer.ToStream);

                try
                {
                    copyIn.Start();

                    List<T> itemsToImportToList = itemsToImport.ToList();
                    Console.WriteLine("Finished query for CSV records after {0}", stopwatch.Elapsed);
                    itemsToImportToList.ForEach(p =>
                    {
                        addItemToSerializer(p, serializer);
                        serializer.EndRow();
                        serializer.Flush();
                    });

                    copyIn.End();
                    serializer.Close();
                }
                catch (Exception e)
                {
                    try
                    {
                        copyIn.Cancel("Cancel copy on exception");
                    }
                    catch (NpgsqlException se)
                    {
                        if (!se.BaseMessage.Contains("Cancel copy on exception"))
                        {
                            throw new Exception(string.Format("Failed to cancel copy: {0} upon failure: {1}", se, e));
                        }
                    }

                    throw;
                }

                Console.WriteLine("Finished temp table fill after {0}", stopwatch.Elapsed);

                commandRunner.Execute(string.Format("create index import_temp_idx on import_temp ({0})", idsCsv));
                commandRunner.Execute("analyze import_temp");

                StringBuilder idMatchClause = new StringBuilder(string.Format(" it.{0} = t.{0} ", idFieldsToImport[0]));
                idFieldsToImport.Skip(1).ToList().ForEach((id) =>
                {
                    idMatchClause.AppendFormat(" and it.{0} = t.{0} ", id);
                });

                var rowsDeleted = commandRunner.Execute(string.Format(@"delete from {0} t
                                                    where not exists (
                                                    select 1 from import_temp it
                                                    where {1}
                                                    )", targetTable, idMatchClause.ToString()));
                Console.WriteLine("Deleted {0} rows", rowsDeleted);

                var updatedRows = commandRunner.Execute(string.Format(@"update {0} t
                                                    set population = it.population
                                                    from import_temp it
                                                    where {1}
                                                    --it.id = t.id
                                                    --and it.store_id = t.store_id
                                                    and it.population <> t.population", targetTable, idMatchClause));
                Console.WriteLine("Updated {0} rows", updatedRows);

                // TODO: use non ID fields from params
                // insertCmd.CommandText = @"insert into inventories (product_id, store_id, quantity)
                //                             select x.product_id, x.store_id, x.quantity
                //                             from inv_temp x
                //                             left join inventories i using (product_id, store_id)
                //                             where i.product_id is null";
                var idsCsvWithItPrefix = string.Join(", ", idFieldsToImport.Select(id => "it." + id));
                var insertedRows = commandRunner.Execute(string.Format(@"insert into {0} ({2}, population)
                                                    select {3}, it.population
                                                    from import_temp it
                                                    left join {0} t using ({1})
                                                    where t.{1} is null", targetTable, idFieldsToImport.First(), idsCsv, idsCsvWithItPrefix));
                Console.WriteLine("Added {0} rows", insertedRows);
            }
        }
    }
}
