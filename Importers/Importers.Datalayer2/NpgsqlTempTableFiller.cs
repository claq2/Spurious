using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Importers.DataLayer
{
    public class NpgsqlTempTableFiller : INpgsqlTempTableFiller
    {
        public INpgsqlConnectionWrapper Wrapper { get; set; }

        public NpgsqlTempTableFiller()
        {
        }

        public NpgsqlTempTableFiller(INpgsqlConnectionWrapper wrapper)
        {
            this.Wrapper = wrapper;
        }

        /// <summary>
        /// Set this instance's Connection property before calling this method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tempTableName"></param>
        /// <param name="prototypeTable"></param>
        /// <param name="itemsToImport"></param>
        public void Fill<T>(string tempTableName, string prototypeTable,  IItemCollection<T> itemsToImport) where T : IItem
        {
            this.Wrapper.ExecuteNonQuery($"create temp table {tempTableName} as (select * from {prototypeTable} where 0 = 1)");

            string fields = string.Join(", ", itemsToImport.DbIdFields.Concat(itemsToImport.DbDataFields));
            var copyCommand = $"copy {tempTableName} ({fields}) from stdin with csv";

            using (var writer = this.Wrapper.BeginTextImport(copyCommand))
            {
                try
                {
                    foreach (T item in itemsToImport.Items)
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
