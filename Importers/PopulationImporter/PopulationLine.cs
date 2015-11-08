using Importers.DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopulationImporter
{
    interface IStatsCanCsvLine
    {
        string Characteristics { get; set; }
    }
    internal class PopulationLine : IItem, IStatsCanCsvLine
    {
        public int GeoCode { get; set; }
        public string CsdName { get; set; }
        public string Province { get; set; }
        public string Topic { get; set; }
        public string Characteristics { get; set; }
        public decimal Total { get; set; }
        public string IdAndDataFieldsAsCsv { get { return $"{this.GeoCode},{this.Province},\"{this.CsdName}\",{Convert.ToInt32(this.Total)}"; } }
    }

    internal class IncomeLine : IItem, IStatsCanCsvLine
    {
        public int GeoCode { get; set; }
        public string CsdName { get; set; }
        public string Province { get; set; }
        public string Topic { get; set; }
        public string Characteristics { get; set; }
        public decimal Total { get; set; }
        public string IdAndDataFieldsAsCsv { get { return $"{this.GeoCode},{this.Province},\"{this.CsdName}\",{Convert.ToInt32(this.Total)}"; } }
    }

}