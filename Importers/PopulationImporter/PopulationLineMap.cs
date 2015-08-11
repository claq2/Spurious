using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PopulationImporter
{
    internal sealed class PopulationLineMap : CsvClassMap<PopulationLine>
    {
        public PopulationLineMap()
        {
            Map(m => m.Characteristics).Name("Characteristics");
            Map(m => m.CsdName).Name("CSD_Name");
            Map(m => m.GeoCode).Name("Geo_Code");
            Map(m => m.Topic).Name("Topic");
            Map(m => m.Total).Name("Total").Default(0);
        }
    }
}
