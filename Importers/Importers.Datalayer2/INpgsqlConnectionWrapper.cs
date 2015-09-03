using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Importers.DataLayer2
{
    public interface INpgsqlConnectionWrapper
    {
        IDbConnection Connection { get; }
    }
}
