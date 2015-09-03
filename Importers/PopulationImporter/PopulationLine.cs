using Importers.Datalayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopulationImporter
{
    class PopulationLine : IItem
    {
        public int GeoCode { get; set; }
        public string CsdName { get; set; }
        public string Topic { get; set; }
        public string Characteristics { get; set; }
        public decimal Total { get; set; }
        public List<string> DbIdFields { get { return new List<string> { "id" }; } }
        public List<string> DbDataFields { get { return new List<string> { "population" }; } }
        public string IdAndDataFieldsAsCsv { get { return string.Format("{0},{1}", this.GeoCode, Convert.ToInt32(this.Total)); } }
    }
}
