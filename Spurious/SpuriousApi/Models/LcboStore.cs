using System;
using System.Data;
using System.Data.Common;
using System.Linq;

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

        public LcboStore(DbDataReader reader) : this()
        {
            this.Id = Convert.ToInt32(reader["id"]);

            var columnNames = reader.GetSchemaTable().Rows.OfType<DataRow>().Select(r => (string)r["ColumnName"]);

            if (columnNames.Contains("name") && reader["name"] != DBNull.Value)
            {
                this.Name = reader["name"] as string;
            }

            if (columnNames.Contains("location") && reader["location"] != DBNull.Value)
            {
                this.GeoJSON = reader["location"] as string;
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
        }

        public string GeoJSON { get; internal set; }
        public int Id { get; internal set; }
        public string Name { get; internal set; }
        public AlcoholVolumes Volumes { get; private set; }
    }
}