using System;
using System.Collections.Generic;
using System.Linq;

namespace Importers.DataLayer
{
    public interface INpgsqlCopyTextWriterWrapper : IDisposable
    {
        void Cancel();
        void Write(string value);
    }
}
