using CsvHelper.Configuration;
using Importers.DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LcboImporter
{
    public class Store : IItem
    {
        public int Id { get; set; }
        public bool IsDead { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }

        public string IdAndDataFieldsAsCsv
        {
            get { return $"{Id},\"{Name}\",{City},{Latitude},{Longitude}"; }
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {4} {2} {3}", Id, Name, Latitude, Longitude, City);
        }
    }
}