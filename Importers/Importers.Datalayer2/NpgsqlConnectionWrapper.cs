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
        }

        public INpgsqlCopyTextWriterWrapper BeginTextImport(string copyFromCommand)
        {
            var writer = ((NpgsqlConnection)this.Connection).BeginTextImport(copyFromCommand) as NpgsqlCopyTextWriter;
            return new NpgsqlCopyTextWriterWrapper(writer);
        }

        public IDbCommand CreateCommand()
        {
            return this.Connection.CreateCommand();
        }

        public void Open()
        {
            this.Connection.Open();
        }

        public ConnectionState State { get { return this.Connection.State; } }

        public void Dispose()
        {
            if (this.Connection != null)
            {
                this.Connection.Dispose();
                this.Connection = null;
            }
        }

        public int ExecuteNonQuery(string commandText, int timeoutInSeconds = 9001)
        {
            var command = this.CreateCommand();
            command.CommandText = commandText;
            command.CommandTimeout = timeoutInSeconds;
            return command.ExecuteNonQuery();
        }
    }
}
