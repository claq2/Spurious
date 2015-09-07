using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Importers.Datalayer2
{
    public interface INpgsqlConnectionWrapper : IDisposable
    {
        INpgsqlCopyTextWriterWrapper BeginTextImport(string copyFromCommand);
        ConnectionState State { get; }
        void Open();
        IDbCommand CreateCommand();
        IDbConnection Connection { get; }
        int ExecuteNonQuery(string commandText, int timeoutInSeconds = 9001);
    }
}
