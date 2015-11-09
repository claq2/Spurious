using Importers.DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PopulationImporter
{
    internal class AverageAfterTaxIncomeLineCollection : LineCollection<IncomeLine>, IItemCollection<IncomeLine>
    {
        internal AverageAfterTaxIncomeLineCollection()
            : base("average_after_tax_income")
        {
        }
    }
}
