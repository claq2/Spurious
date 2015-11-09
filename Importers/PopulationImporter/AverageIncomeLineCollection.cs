using Importers.DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PopulationImporter
{
    internal class AverageIncomeLineCollection : LineCollection<IncomeLine>, IItemCollection<IncomeLine>
    {
        internal AverageIncomeLineCollection()
            : base("average_income")
        {
        }
    }
}
