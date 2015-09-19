using System;
using System.Collections.Generic;
using System.Linq;

namespace SpuriousApi.Models
{
    public class SubdivAndVolumes
    {
        public Subdivision Subdivision { get; set; }
        public List<AlcoholVolumes> Volumes { get; set; }
    }
}
