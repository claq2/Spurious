using Importers.DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PopulationImporter
{
    internal class PopulationLineCollection : IItemCollection<PopulationLine>
    {
        public List<string> DbIdFields { get { return new List<string> { "id" }; } }
        public List<string> DbDataFields { get { return new List<string> { "province", "name", "population" }; } }
        public IEnumerable<PopulationLine> Items { get; set; }
    }

    internal abstract class LineCollection<T> : IItemCollection<T>
    {
        private string incomeField;

        protected LineCollection(string incomeField, IEnumerable<T> items)
        {
            this.incomeField = incomeField;
            this.Items = items;
        }

        public List<string> DbIdFields { get { return new List<string> { "id" }; } }
        public List<string> DbDataFields { get { return new List<string> { "province", "name", incomeField }; } }
        public IEnumerable<T> Items { get; set; }
    }

    internal class AverageIncomeLineCollection : LineCollection<IncomeLine>, IItemCollection<IncomeLine>
    {
        internal AverageIncomeLineCollection(IEnumerable<IncomeLine> items) : base("average_income", items)
        {
        }
        //public List<string> DbIdFields { get { return new List<string> { "id" }; } }
        //public List<string> DbDataFields { get { return new List<string> { "province", "name", "average_income" }; } }
        //public IEnumerable<IncomeLine> Items { get; set; }
    }

    internal class MedianIncomeLineCollection : LineCollection<IncomeLine>, IItemCollection<IncomeLine>
    {
        internal MedianIncomeLineCollection(IEnumerable<IncomeLine> items) : base("median_income", items)
        {
        }
        //public List<string> DbIdFields { get { return new List<string> { "id" }; } }
        //public List<string> DbDataFields { get { return new List<string> { "province", "name", "median_income" }; } }
        //public IEnumerable<IncomeLine> Items { get; set; }
    }

    internal class AverageAfterTaxIncomeLineCollection : LineCollection<IncomeLine>, IItemCollection<IncomeLine>
    {
        internal AverageAfterTaxIncomeLineCollection(IEnumerable<IncomeLine> items) : base("average_after_tax_income", items)
        {
        }
        //public List<string> DbIdFields { get { return new List<string> { "id" }; } }
        //public List<string> DbDataFields { get { return new List<string> { "province", "name", "average_after_tax_income" }; } }
        //public IEnumerable<IncomeLine> Items { get; set; }
    }

    internal class MedianAfterTaxIncomeLineCollection : LineCollection<IncomeLine>, IItemCollection<IncomeLine>
    {
        internal MedianAfterTaxIncomeLineCollection(IEnumerable<IncomeLine> items) : base("median_after_tax_income", items)
        {
        }
        //public List<string> DbIdFields { get { return new List<string> { "id" }; } }
        //public List<string> DbDataFields { get { return new List<string> { "province", "name", "median_after_tax_income" }; } }
        //public IEnumerable<IncomeLine> Items { get; set; }
    }

}
