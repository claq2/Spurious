using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LcboImporter
{
    public sealed class ProductMap : CsvClassMap<Product>
    {
        public ProductMap()
        {
            Map(m => m.Id).Name("id");
            Map(m => m.IsDead).Name("is_dead").TypeConverterOption(true, "t").TypeConverterOption(false, "f");
            Map(m => m.Name).Name("name");
            Map(m => m.Volume).Name("volume_in_milliliters");
            Map(m => m.Category).Name("primary_category").TypeConverter<ProductCategtoryConverter>();
        }
    }
}
