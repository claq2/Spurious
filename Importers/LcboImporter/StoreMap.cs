using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LcboImporter
{
    public sealed class StoreMap : CsvClassMap<Store>
    {
        public StoreMap()
        {
            Map(m => m.Id).Name("id");
            Map(m => m.IsDead).Name("is_dead").TypeConverterOption(true, "t").TypeConverterOption(false, "f");
            Map(m => m.Name).Name("name");
            Map(m => m.City).Name("city");
            Map(m => m.Latitude).Name("latitude");
            Map(m => m.Longitude).Name("longitude");
        }
    }
}