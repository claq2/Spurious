using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Importers.DataLayer
{
    public class NpgsqlConnectionWrapper : INpgsqlConnectionWrapper, IDisposable
    {
        public IDbConnection Connection { get; private set; }

        public NpgsqlConnectionWrapper(string connectionString)
        {
            this.Connection = new NpgsqlConnection(connectionString);
            this.CommandRunner = new NonQueryCommandRunner() { Connection = this.Connection };
        }

        public ICopySerializer CreateSerializer()
        {
            return new NpgsqlCopySerializerWrapper(this.Connection);
        }

        public INonQueryCommandRunner CommandRunner { get; private set; }

        public void Dispose()
        {
            if (this.Connection != null)
            {
                this.Connection.Dispose();
                this.Connection = null;
            }
        }
    }
}
