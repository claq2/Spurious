using Importers.DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PopulationImporter
{
    internal class MedianAfterTaxIncomeLineCollection : LineCollection<IncomeLine>, IItemCollection<IncomeLine>
    {
        internal MedianAfterTaxIncomeLineCollection()
            : base("median_after_tax_income")
        {
        }
    }
}
