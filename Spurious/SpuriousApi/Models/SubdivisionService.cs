using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace SpuriousApi.Models
{
    public class SubdivisionService
    {
        private readonly string connString = ConfigurationManager.ConnectionStrings["spurious"].ConnectionString;

        public SubdivisionService()
        {

        }

        public async Task<IEnumerable<Subdivision>> Load100()
        {
            var result = new List<Subdivision>();
            using (var conn = new Npgsql.NpgsqlConnection(connString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = "select id, population, ST_AsGeoJSON(boundry) as boundry from subdivisions limit 100";
                conn.Open();
                var reader = await cmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    var subdivision = new Subdivision() { Id = Convert.ToInt32(reader["id"]) };
                    if (reader["population"] != DBNull.Value)
                    {
                        subdivision.Population = Convert.ToInt32(reader["population"]);
                    }

                    if (reader["boundry"] != DBNull.Value)
                    {
                        subdivision.GeoJSON = reader["boundry"] as string;
                    }

                    result.Add(subdivision);
                }
            }

            return result;
        }

        public async Task<Subdivision> LoadById(int id)
        {
            Subdivision result = null;
            using (var conn = new Npgsql.NpgsqlConnection(connString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = "select id, population, ST_AsGeoJSON(boundry) as boundry from subdivisions where id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                //var idParam = cmd.CreateParameter();
                //idParam.DbType = System.Data.DbType.Int32;
                //idParam.ParameterName = "@id";
                //idParam.Value = id;
                conn.Open();
                var reader = await cmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    var subdivision = new Subdivision() { Id = Convert.ToInt32(reader["id"]) };
                    if (reader["population"] != DBNull.Value)
                    {
                        subdivision.Population = Convert.ToInt32(reader["population"]);
                    }

                    if (reader["boundry"] != DBNull.Value)
                    {
                        subdivision.GeoJSON = reader["boundry"] as string;
                    }

                    result = subdivision;
                }
            }

            return result;
        }

        public async Task<List<Subdivision>> SubdivisionsAndVolumes()
        {
            var result = new List<Subdivision>();
            var query = @"select id, population, name, ST_AsGeoJSON(boundry) as boundary, beer_volume, wine_volume, spirits_volume
                            from subdivisions
                            where province = 'Ontario'";
            using (var conn = new Npgsql.NpgsqlConnection(connString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = query;
                conn.Open();
                var reader = await cmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    result.Add(new Subdivision(reader));
                }
            }

            return result;
        }

        public async Task<List<Subdivision>> Top10AlcoholDensity()
        {
            var result = new List<Subdivision>();
            var query = @"select id, population, name, ST_AsGeoJSON(boundry) as boundary, beer_volume, wine_volume, spirits_volume, (beer_volume + wine_volume + spirits_volume) / population as density
                            from subdivisions
                            where beer_volume > 0
                            order by density desc
                            limit 10";

            var resultDict = new Dictionary<int, Subdivision>();
            using (var conn = new Npgsql.NpgsqlConnection(connString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    conn.Open();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            var subdiv = new Subdivision(reader);
                            result.Add(subdiv);
                            resultDict.Add(subdiv.Id, subdiv);
                        }
                    }
                }

                using (var getStoresCmd = conn.CreateCommand())
                {
                    var subdivIds = string.Join(", ", result.Select(s => s.Id));
                    getStoresCmd.CommandText = $@"select sd.id as subdiv_id, s.id as id, s.name as name, ST_AsGeoJSON(s.location) as location from subdivisions sd inner join stores s on ST_Intersects(s.location, sd.boundry)
                                                                                        where sd.id in ({subdivIds})
                                                                                        order by subdiv_id";
                    using (var storesReader = await getStoresCmd.ExecuteReaderAsync())
                    {
                        while (storesReader.Read())
                        {
                            var store = new LcboStore(storesReader);
                            var subdivId = Convert.ToInt32(storesReader["subdiv_id"]);
                            resultDict[subdivId].LcboStores.Add(store);
                        }
                    }
                }

            }

            return result;
        }
    }
}
