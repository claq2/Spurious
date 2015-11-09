using Importers.DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LcboImporter
{
    public class InventoryCollection : IItemCollection<Inventory>
    {
        public List<string> DbDataFields
        {
            get { return new List<string> { "quantity" }; }
        }

        public List<string> DbIdFields
        {
            get { return new List<string> { "product_id", "store_id" }; }
        }

        public IEnumerable<Inventory> Items { get; private set; }

        public void SetItems(IEnumerable<Inventory> items) { this.Items = items; }
    }
}
