using System;
using System.Linq;
using Importers.DataLayer;
using CsvHelper.Configuration;
using System.IO;

namespace PopulationImporter
{
    interface ICsvQuery<T>
    {
        Func<T, bool> LinePredicate { get; }

        string TableName { get; }

        CsvClassMap CsvMap { get; }

        IItemCollection<T> ItemCollection { get; }

        StreamReader FileStream { get; }
    }
}
