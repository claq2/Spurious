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
            var query = @"select sb.id, sb.population, ST_AsGeoJSON(sb.boundry) as boundary, sum(s.beer_volume) as beer_volume, sum(s.wine_volume) as wine_volume, sum(s.spirits_volume) as spirits_volume
                            from subdivisions sb
                            inner join stores s on ST_Intersects(sb.boundry, s.location)
                            group by sb.id";
            using (var conn = new Npgsql.NpgsqlConnection(connString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = query;
                conn.Open();
                var reader = await cmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    var subdivision = new Subdivision() { Id = Convert.ToInt32(reader["id"]) };
                    if (reader["population"] != DBNull.Value)
                    {
                        subdivision.Population = Convert.ToInt32(reader["population"]);
                    }

                    if (reader["beer_volume"] != DBNull.Value)
                    {
                        subdivision.Volumes.Beer = Convert.ToInt64(reader["beer_volume"]);
                    }

                    if (reader["wine_volume"] != DBNull.Value)
                    {
                        subdivision.Volumes.Wine = Convert.ToInt64(reader["wine_volume"]);
                    }

                    if (reader["spirits_volume"] != DBNull.Value)
                    {
                        subdivision.Volumes.Spirits = Convert.ToInt64(reader["spirits_volume"]);
                    }

                    if (reader["boundary"] != DBNull.Value)
                    {
                        subdivision.GeoJSON = reader["boundary"] as string;
                    }

                    result.Add(subdivision);
                }
            }
            return result;
        }

    }
}
