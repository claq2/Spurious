using System;
using System.Collections.Generic;
using System.Linq;

namespace Importers.Datalayer2
{
    public interface IItem
    {
        List<string> DbIdFields { get; }
        List<string> DbDataFields { get; }
        string IdAndDataFieldsAsCsv { get; }
    }
}
