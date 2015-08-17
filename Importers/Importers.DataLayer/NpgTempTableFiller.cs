using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Importers.DataLayer
{
    public class NpgTempTableFiller : INpgTempTableFiller
    {
        private readonly INonQueryCommandRunner commandRunner;
        private readonly Stopwatch stopwatch;

        public NpgsqlConnection Connection { get; set; }

        public NpgTempTableFiller(Stopwatch stopwatch) : this(stopwatch, new NonQueryCommandRunner())
        {
        }

        public NpgTempTableFiller(Stopwatch stopwatch, INonQueryCommandRunner commandRunner)
        {
            this.stopwatch = stopwatch;
            this.commandRunner = commandRunner;
        }

        /// <summary>
        /// Set this instance's Connection property before calling this method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tempTableName"></param>
        /// <param name="prototypeTable"></param>
        /// <param name="allFieldsToImport"></param>
        /// <param name="itemsToImport"></param>
        /// <param name="addItemToSerializer"></param>
        public void Fill<T>(string tempTableName, string prototypeTable, string allFieldsToImport, IEnumerable<T> itemsToImport, Action<T, NpgsqlCopySerializer> addItemToSerializer)
        {
            this.commandRunner.Connection = this.Connection;
            commandRunner.Execute(string.Format("create temp table {1} as (select * from {0} where 0 = 1)",
                    prototypeTable,
                    tempTableName));

            var copyCmd = this.Connection.CreateCommand();
            copyCmd.CommandTimeout = commandRunner.Timeout;
            copyCmd.CommandText = string.Format("copy {0}({1}) from stdin", tempTableName, allFieldsToImport);
            var serializer = new NpgsqlCopySerializer(this.Connection);
            var copyIn = new NpgsqlCopyIn(copyCmd, this.Connection, serializer.ToStream);

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
        }
    }
}
