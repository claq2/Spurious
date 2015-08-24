using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Importers.DataLayer
{
    public class NpgsqlCopySerializerWrapper: NpgsqlCopySerializer, ICopySerializer
    {
    
        public NpgsqlCopySerializerWrapper(INpgsqlConnectionWrapper connWrapper)
            : base(connWrapper.Connection)
        {
            
        }
    }

    public interface ICopySerializer
    {
        Stream ToStream { get; set; }
        void EndRow();
        void Flush();
        void Close();
        void AddInt32(int fieldValue);
    }

    public interface INpgsqlConnectionWrapper
    {
        NpgsqlConnection Connection { get; }
    }

    public class NpgsqlConnectionWrapper : INpgsqlConnectionWrapper, IDisposable
    {
        public NpgsqlConnection Connection { get; private set; }

        public NpgsqlConnectionWrapper(string connectionString)
        {
            this.Connection = new NpgsqlConnection(connectionString);
        }

        public void Dispose()
        {
            if (Connection != null)
            {
                Connection.Dispose();
                Connection = null;
            }
        }
    }
}
