using Importers.DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LcboImporter
{
    public class ProductCollection : IItemCollection<Product>
    {
        public List<string> DbDataFields
        {
            get { return new List<string> { "name", "category", "volume" }; }
        }

        public List<string> DbIdFields
        {
            get { return new List<string> { "id" }; }
        }

        public IEnumerable<Product> Items { get; private set; }

        public void SetItems(IEnumerable<Product> items) { this.Items = items; }
    }
}
