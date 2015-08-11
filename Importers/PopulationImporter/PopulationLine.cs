using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopulationImporter
{
    class PopulationLine
    {
        public int GeoCode { get; set; }
        public string CsdName { get; set; }
        public string Topic { get; set; }
        public string Characteristics { get; set; }
        public decimal Total { get; set; }
    }
}
