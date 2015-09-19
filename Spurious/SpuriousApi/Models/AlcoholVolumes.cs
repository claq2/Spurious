using System;
using System.Collections.Generic;
using System.Linq;

namespace SpuriousApi.Models
{
    public class AlcoholVolumes
    {
        public long Beer { get; set; }
        public long Wine { get; set; }
        public long Spirits { get; set; }
        public long Total { get { return this.Beer + this.Wine + this.Spirits; } }
    }
}
