using Importers.Datalayer2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PopulationImporter
{
    internal class PopulationLineCollection : IItemCollection<PopulationLine>
    {
        public List<string> DbIdFields { get { return new List<string> { "id" }; } }
        public List<string> DbDataFields { get { return new List<string> { "population" }; } }
        public IEnumerable<PopulationLine> Items { get; set; }
    }
}
