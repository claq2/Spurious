using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Importers.Datalayer2
{
    public interface INpgsqlConnectionWrapper
    {
        System.IO.TextWriter BeginTextImport(string copyFromCommand);
        ConnectionState State { get; }
        void Open();
        IDbCommand CreateCommand();
        IDbConnection Connection { get; }
    }
}
