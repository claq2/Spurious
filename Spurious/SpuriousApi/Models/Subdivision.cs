using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace SpuriousApi.Models
{
    public class Subdivision
    {
        public int Id { get; set; }
        public int? Population { get; set; }
        public string GeoJSON { get; set; }
    }
}