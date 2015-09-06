using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Importers.Datalayer2
{
    public class NpgsqlCopyTextWriterWrapper : INpgsqlCopyTextWriterWrapper
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
