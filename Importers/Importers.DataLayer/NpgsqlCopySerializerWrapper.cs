using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Importers.DataLayer
{
    public class NpgsqlCopySerializerWrapper : NpgsqlCopySerializer, ICopySerializer
    {
        private readonly IDbConnection connection;

        public NpgsqlCopySerializerWrapper(IDbConnection connection)
            : base((NpgsqlConnection)connection)
        {
            this.connection = connection;
        }

        public ICopyIn CreateCopyIn(IDbCommand cmd)
        {
            return new NpgsqlCopyInWrapper(cmd, this.connection, this.ToStream);
        }
    }

    public interface ICopySerializer
    {
        Stream ToStream { get; set; }
        void EndRow();
        void Flush();
        void Close();
        void AddInt32(int fieldValue);
        ICopyIn CreateCopyIn(IDbCommand cmd);
    }

    public class NpgsqlCopyInWrapper : NpgsqlCopyIn, ICopyIn
    {
        public NpgsqlCopyInWrapper(IDbCommand cmd, IDbConnection conn, Stream fromStream)
            : base((NpgsqlCommand)cmd, (NpgsqlConnection)conn, fromStream)
        {
            
        }
    }

    public interface ICopyIn
    {
        void Start();
        void End();
        void Cancel(string message);
    }
}