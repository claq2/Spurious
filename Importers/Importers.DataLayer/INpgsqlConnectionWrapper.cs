using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Importers.DataLayer
{
    public interface INpgsqlConnectionWrapper
    {
        IDbConnection Connection { get; }
        ICopySerializer CreateSerializer();
        INonQueryCommandRunner CommandRunner { get; }
    }
}
