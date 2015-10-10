using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using Newtonsoft.Json;
using GeoJSON.Net.Geometry;
using GeoJSON.Net.Feature;
using System.Collections.Generic;
namespace SpuriousApi.Models
{
    public class LcboStore
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LcboStore"/> class.
        /// </summary>
        public LcboStore()
        {
            GeoJSON = new Feature(new Point(new Position()));
            Id = 0;
            Name = String.Empty;
            Volumes = new AlcoholVolumes();
        }

        public LcboStore(DbDataReader reader) : this()
        {
            this.Id = Convert.ToInt32(reader["id"]);

            var columnNames = reader.GetSchemaTable().Rows.OfType<DataRow>().Select(r => (string)r["ColumnName"]);

            if (columnNames.Contains("name") && reader["name"] != DBNull.Value)
            {
                this.Name = reader["name"] as string;
            }

            if (columnNames.Contains("beer_volume") && reader["beer_volume"] != DBNull.Value)
            {
                this.Volumes.Beer = Convert.ToInt32(reader["beer_volume"]);
            }

            if (columnNames.Contains("wine_volume") && reader["wine_volume"] != DBNull.Value)
            {
                this.Volumes.Wine = Convert.ToInt32(reader["wine_volume"]);
            }

            if (columnNames.Contains("spirits_volume") && reader["spirits_volume"] != DBNull.Value)
            {
                this.Volumes.Spirits = Convert.ToInt32(reader["spirits_volume"]);
            }

            if (columnNames.Contains("city") && reader["city"] != DBNull.Value)
            {
                this.City = reader["city"] as string;
            }

            if (columnNames.Contains("subdivision_id") && reader["subdivision_id"] != DBNull.Value)
            {
                this.SubdivisionId = Convert.ToInt32(reader["subdivision_id"]);
            }

            if (columnNames.Contains("location") && reader["location"] != DBNull.Value)
            {
                var location = reader["location"] as string;
                var geojsonlocation = JsonConvert.DeserializeObject<Point>(location);
                var properties = new Dictionary<string, object>
                {
                    { "name", this.Name },
                    { "city", this.City },
                    { "beerVolume", this.Volumes.Beer },
                    { "wineVolume", this.Volumes.Wine },
                    { "spiritsVolume", this.Volumes.Spirits },
                    { "totalVolume", this.Volumes.Total },
                };

                var feature = new Feature(geojsonlocation, properties);
                this.GeoJSON = feature;
            }
        }

        public Feature GeoJSON { get; internal set; }
        public int Id { get; internal set; }
        public string Name { get; internal set; }
        public AlcoholVolumes Volumes { get; private set; }
        public string City { get; internal set; }
        public int SubdivisionId { get; set; }
    }
}