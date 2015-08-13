using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace Importers.DataLayer
{
    public interface INonQueryCommandRunner
    {
        int Timeout { get; }
        IDbConnection Connection { get; set; }
        int Execute(string commandText);
    }
}
