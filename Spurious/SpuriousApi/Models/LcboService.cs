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
                    var store = new LcboStore(reader);
                    result.Add(store);
                }
            }

            return result;
        }

        public async Task<List<LcboStore>> StoresInArea(string geoJson)
        {
            var result = new List<LcboStore>();
            var query = $@"select s.id, s.name, s.beer_volume, s.wine_volume, s.spirits_volume
                            from stores s
                             where ST_Intersects(ST_GeomFromGeoJSON('{geoJson}'), s.location)";
            using (var conn = new Npgsql.NpgsqlConnection(connString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = query;
                conn.Open();
                var reader = await cmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    var store = new LcboStore(reader);
                    result.Add(store);
                }
            }
            return result;
        }
    }
}