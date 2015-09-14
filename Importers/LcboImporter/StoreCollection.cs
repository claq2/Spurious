using Importers.DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LcboImporter
{
    public class StoreCollection : IItemCollection<Store>
    {
        public List<string> DbDataFields
        {
            get { return new List<string> { "name", "city", "latitude", "longitude" }; }
        }

        public List<string> DbIdFields
        {
            get { return new List<string> { "id" }; }
        }

        public IEnumerable<Store> Items
        {
            get; set;
        }
    }
}
