using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace SpuriousApi.Models
{
    public class Subdivision
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Subdivision"/> class.
        /// </summary>
        public Subdivision()
        {
            Id = 0;
            Population = null;
            GeoJSON = String.Empty;
            Volumes = new AlcoholVolumes();
        }

        public int Id { get; set; }
        public int? Population { get; set; }
        public string GeoJSON { get; set; }
        public AlcoholVolumes Volumes { get; private set; }
        public float OverallAlcoholDensity { get { return this.Volumes.Total / (float)Population; } }
        public float BeerDensity { get { return this.Volumes.Beer / (float)Population; } }
        public float WineDensity { get { return this.Volumes.Wine / (float)Population; } }
        public float SpiritsDensity { get { return this.Volumes.Spirits / (float)Population; } }
    }
}