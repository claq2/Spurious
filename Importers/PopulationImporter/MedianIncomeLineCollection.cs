using Importers.DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PopulationImporter
{
    internal class MedianIncomeLineCollection : LineCollection<IncomeLine>, IItemCollection<IncomeLine>
    {
        internal MedianIncomeLineCollection()
            : base("median_income")
        {
        }
    }
}
