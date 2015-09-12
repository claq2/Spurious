using Importers.DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BoundryImporter
{
    public class BoundaryItemCollection : IItemCollection<BoundaryItem>
    {
        public List<string> DbDataFields
        {
            get { return new List<string> { "boundary_gml" }; }
        }

        public List<string> DbIdFields
        {
            get { return new List<string> { "id" }; }
        }

        public IEnumerable<BoundaryItem> Items { get; set; }
    }
}
