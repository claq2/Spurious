using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Data.Common;
using System.Data;
using GeoJSON.Net.Geometry;
using GeoJSON.Net.Feature;

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
            GeoJSON = string.Empty;
            Name = string.Empty;
            CentreGeoJson = string.Empty;
            Volumes = new AlcoholVolumes();
            LcboStores = new List<LcboStore>();
        }

        public Subdivision(DbDataReader reader) : this()
        {
            this.Id = Convert.ToInt32(reader["id"]);

            var columnNames = reader.GetSchemaTable().Rows.OfType<DataRow>().Select(r => (string)r["ColumnName"]);

            if (reader["population"] != DBNull.Value)
            {
                this.Population = Convert.ToInt32(reader["population"]);
            }

            if (reader["beer_volume"] != DBNull.Value)
            {
                this.Volumes.Beer = Convert.ToInt64(reader["beer_volume"]);
            }

            if (reader["wine_volume"] != DBNull.Value)
            {
                this.Volumes.Wine = Convert.ToInt64(reader["wine_volume"]);
            }

            if (reader["spirits_volume"] != DBNull.Value)
            {
                this.Volumes.Spirits = Convert.ToInt64(reader["spirits_volume"]);
            }

            if (columnNames.Contains("boundary") && reader["boundary"] != DBNull.Value)
            {
                var boundary = reader["boundary"] as string;
                var featureWrapper = 
$@"{{ ""type"": ""FeatureCollection"",
    ""features"": [
      {{ ""type"": ""Feature"",
        ""geometry"": {boundary},
        ""properties"":{{}}
      }}
      ]
}}";

                this.GeoJSON = featureWrapper;
            }

            if (reader["name"] != DBNull.Value)
            {
                this.Name = reader["name"] as string;
            }

            if (columnNames.Contains("centre") && reader["centre"] != DBNull.Value)
            {
                this.GeoJsonCentre = reader["centre"] as string;
                var geocentre = Newtonsoft.Json.JsonConvert.DeserializeObject<Point>(this.GeoJsonCentre);
                this.CentreLatitude = ((GeographicPosition)geocentre.Coordinates).Latitude;
                this.CentreLongitude = ((GeographicPosition)geocentre.Coordinates).Longitude;
            }
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int? Population { get; set; }
        public string GeoJSON { get; set; }
        public string GeoJsonCentre { get; set; }
        public double CentreLatitude { get; set; }
        public double CentreLongitude { get; set; }
        public AlcoholVolumes Volumes { get; private set; }
        public float OverallAlcoholDensity { get { return this.Volumes.Total / (float)Population; } }
        public float BeerDensity { get { return this.Volumes.Beer / (float)Population; } }
        public float WineDensity { get { return this.Volumes.Wine / (float)Population; } }
        public float SpiritsDensity { get { return this.Volumes.Spirits / (float)Population; } }
        public string CentreGeoJson { get; set; }
        public List<LcboStore> LcboStores { get; private set; }
    }
}