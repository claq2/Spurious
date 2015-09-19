using System;

namespace SpuriousApi.Models
{
    public class LcboStore
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LcboStore"/> class.
        /// </summary>
        public LcboStore()
        {
            GeoJSON = String.Empty;
            Id = 0;
            Name = String.Empty;
            Volumes = new AlcoholVolumes();
        }

        public string GeoJSON { get; internal set; }
        public int Id { get; internal set; }
        public string Name { get; internal set; }
        public AlcoholVolumes Volumes { get; set; }
    }
}