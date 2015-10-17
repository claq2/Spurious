using SpuriousApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpuriousApi.ViewModels
{
    public class ListAndMapView
    {
        public string Title { get; set; }
        public IEnumerable<Subdivision> Subdivisions { get; set; }
        // For table header
        public string DensityName { get; set; }
        // Which density property on subdiv to show
        public string DensityPropertyToUse { get; set; }
    }
}