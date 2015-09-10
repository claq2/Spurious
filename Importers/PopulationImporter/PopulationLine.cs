using Importers.Datalayer2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopulationImporter
{
    internal class PopulationLine : IItem
    {
        public int GeoCode { get; set; }
        public string CsdName { get; set; }
        public string Topic { get; set; }
        public string Characteristics { get; set; }
        public decimal Total { get; set; }
        public string IdAndDataFieldsAsCsv { get { return string.Format("{0},{1}", this.GeoCode, Convert.ToInt32(this.Total)); } }
    }
}