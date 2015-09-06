using System;
using System.Collections.Generic;
using System.Linq;

namespace Importers.Datalayer2
{
    public interface INpgsqlCopyTextWriterWrapper : IDisposable
    {
        void Cancel();
        void Write(string value);
    }
}
