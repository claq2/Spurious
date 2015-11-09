using Importers.DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PopulationImporter
{
    internal abstract class LineCollection<T> : IItemCollection<T>
    {
        private string incomeField;

        protected LineCollection(string incomeField)
        {
            this.incomeField = incomeField;
        }

        public List<string> DbIdFields { get { return new List<string> { "id" }; } }
        public List<string> DbDataFields { get { return new List<string> { "province", "name", incomeField }; } }
        public IEnumerable<T> Items { get; private set; }
        public void SetItems(IEnumerable<T> items) { this.Items = items; }
    }
}
