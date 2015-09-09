using System;
using System.Collections.Generic;
using System.Linq;

namespace Importers.Datalayer2
{
    public interface INpgsqlTempTableFiller
    {
        INpgsqlConnectionWrapper Wrapper { get; set; }

        /// <summary>
        /// Set this instance's Connection property before calling this method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tempTableName"></param>
        /// <param name="prototypeTable"></param>
        /// <param name="itemsToImport"></param>
        void Fill<T>(string tempTableName, string prototypeTable, IItemCollection<T> itemsToImport) where T : IItem;
    }
}
