using System;
using System.Collections.Generic;
using System.Linq;

namespace Importers.DataLayer
{
    public interface IItemCollection<T>
    {
        List<string> DbIdFields { get; }
        List<string> DbDataFields { get; }
        IEnumerable<T> Items { get; }
        void SetItems(IEnumerable<T> items);
    }
}
