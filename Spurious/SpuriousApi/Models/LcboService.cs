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
                cmd.CommandText = "select id, name, ST_AsGeoJSON(location) as location from stores limit 100";
                conn.Open();
                var reader = await cmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    var subdivision = new LcboStore() { Id = Convert.ToInt32(reader["id"]) };
                    if (reader["name"] != DBNull.Value)
                    {
                        subdivision.Name = reader["name"] as string;
                    }

                    if (reader["location"] != DBNull.Value)
                    {
                        subdivision.GeoJSON = reader["location"] as string;
                    }

                    result.Add(subdivision);
                }
            }

            return result;
        }
    }
}