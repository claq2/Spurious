using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace Importers.Datalayer2
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
            return ((NpgsqlConnection)this.Connection).BeginTextImport(copyFromCommand);
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
    }

    public interface INpgsqlCopyTextWriterWrapper
    {
        void Cancel();
        void Write(string value);
    }
    public class NpgsqlCopyTextWriterWrapper : IDisposable, INpgsqlCopyTextWriterWrapper
    {
        readonly NpgsqlCopyTextWriter writer;
        public NpgsqlCopyTextWriterWrapper(NpgsqlCopyTextWriter writer)
        {
            this.writer = writer;
        }

        public void Cancel()
        {
            this.writer.Cancel();
        }

        public void Dispose()
        {
            if (this.writer != null)
            {
                this.writer.Dispose();
            }
        }

        public void Write(string value)
        {
            this.writer.Write(value);
        }
    }
}
