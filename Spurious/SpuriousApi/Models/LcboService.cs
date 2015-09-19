using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace SpuriousApi.Models
{

    public class LcboService
    {
        private readonly string connString = ConfigurationManager.ConnectionStrings["spurious"].ConnectionString;

        public async Task<List<LcboStore>> GetLcboStores()
        {
            var result = new List<LcboStore>();
            using (var conn = new Npgsql.NpgsqlConnection(connString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = "select id, name, ST_AsGeoJSON(location) as location, beer_volume, wine_volume, spirits_volume from stores limit 100";
                conn.Open();
                var reader = await cmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    var store = new LcboStore() { Id = Convert.ToInt32(reader["id"]) };
                    if (reader["name"] != DBNull.Value)
                    {
                        store.Name = reader["name"] as string;
                    }

                    if (reader["location"] != DBNull.Value)
                    {
                        store.GeoJSON = reader["location"] as string;
                    }

                    if (reader["beer_volume"] != DBNull.Value)
                    {
                        store.Volumes.Beer = Convert.ToInt32(reader["beer_volume"]);
                    }

                    if (reader["wine_volume"] != DBNull.Value)
                    {
                        store.Volumes.Wine = Convert.ToInt32(reader["wine_volume"]);
                    }

                    if (reader["spirits_volume"] != DBNull.Value)
                    {
                        store.Volumes.Spirits = Convert.ToInt32(reader["spirits_volume"]);
                    }

                    result.Add(store);
                }
            }

            return result;
        }

        public async Task<List<LcboService>> StoresInArea(string geoJson)
        {
            return null;
        }
    }
}