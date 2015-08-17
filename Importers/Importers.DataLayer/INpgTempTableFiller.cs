using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Importers.DataLayer
{
    public interface INpgTempTableFiller
    {
        NpgsqlConnection Connection { get; set; }

        /// <summary>
        /// Set this instance's Connection property before calling this method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tempTableName"></param>
        /// <param name="prototypeTable"></param>
        /// <param name="allFieldsToImport"></param>
        /// <param name="itemsToImport"></param>
        /// <param name="addItemToSerializer"></param>
        void Fill<T>(string tempTableName, string targetTable, string allFieldsToImport, IEnumerable<T> itemsToImport, Action<T, NpgsqlCopySerializer> addItemToSerializer);
    }
}
