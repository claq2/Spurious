using System;
using System.Collections.Generic;
using System.Linq;

namespace Importers.Datalayer2
{
    public interface IItem
    {
        string IdAndDataFieldsAsCsv { get; }
    }

    public interface IItemCollection<T>
    {
        List<string> DbIdFields { get; }
        List<string> DbDataFields { get; }
        IEnumerable<T> Items { get; }
    }
}
