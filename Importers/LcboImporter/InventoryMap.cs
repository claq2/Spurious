using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LcboImporter
{
    public sealed class InventoryMap : CsvClassMap<Inventory>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryMap"/> class.
        /// </summary>
        public InventoryMap()
        {
            Map(m => m.ProductId).Name("product_id");
            Map(m => m.StoreId).Name("store_id");
            Map(m => m.IsDead).Name("is_dead").TypeConverterOption(true, "t").TypeConverterOption(false, "f");
            Map(m => m.Quantity).Name("quantity");
        }
    }
}
