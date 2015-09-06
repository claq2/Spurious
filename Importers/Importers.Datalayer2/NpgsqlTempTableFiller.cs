using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Importers.Datalayer2
{
    public class NpgsqlTempTableFiller
    {
        public NpgsqlConnection Connection { get; set; }

        /// <summary>
        /// Set this instance's Connection property before calling this method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tempTableName"></param>
        /// <param name="prototypeTable"></param>
        /// <param name="itemsToImport"></param>
        public void Fill<T>(string tempTableName, string prototypeTable,  IEnumerable<T> itemsToImport) where T : IItem
        {
            if (this.Connection.State == System.Data.ConnectionState.Closed)
            {
                this.Connection.Open();
            }

            var command = this.Connection.CreateCommand();
            command.CommandText = string.Format("create temp table {1} as (select * from {0} where 0 = 1)",
                                      prototypeTable,
                                      tempTableName);
            command.ExecuteNonQuery();
            string fields = string.Join(",", itemsToImport.First().DbIdFields.Concat(itemsToImport.First().DbDataFields));
            var copyCommand = string.Format("copy {0}({1}) from stdin with csv", tempTableName, string.Join(",", fields));

            using (NpgsqlCopyTextWriter writer = (NpgsqlCopyTextWriter)this.Connection.BeginTextImport(copyCommand))
            {
                try
                {
                    foreach (T item in itemsToImport)
                    {
                        writer.Write(item.IdAndDataFieldsAsCsv);
                        writer.Write("\n");
                    }
                }
                catch (Exception e)
                {
                    try
                    {
                        writer.Cancel();
                    }
                    catch (NpgsqlException se)
                    {
                        if (se.Message.Contains("Expected ErrorResponse when cancelling COPY but got"))
                        {
                            throw new Exception(string.Format("Failed to cancel copy: {0} upon failure: {1}", se, e));
                        }
                    }

                    throw;
                }
            }

        }
    }
}
