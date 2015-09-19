using System;
using System.Collections.Generic;
using System.Linq;

namespace SpuriousApi.Models
{
    public class AlcoholVolumes
    {
        public int Beer { get; set; }
        public int Wine { get; set; }
        public int Spirits { get; set; }
        public int Total { get { return this.Beer + this.Wine + this.Spirits; } }
    }
}
