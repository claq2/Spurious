using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace SpuriousApi.Models
{
    public class CensusService
    {
        private readonly string connString = ConfigurationManager.ConnectionStrings["spurious"].ConnectionString;

        public CensusService()
        {

        }

        public IEnumerable<Subdivision> Load100()
        {
            var result = new List<Subdivision>();
            using (var conn = new Npgsql.NpgsqlConnection(connString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = "select id, population, ST_AsGeoJSON(boundry) as boundry from subdivisions limit 100";
                conn.Open();
                var reader = cmd.ExecuteReader();
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

        public Subdivision LoadById(int id)
        {
            Subdivision result = null;
            using (var conn = new Npgsql.NpgsqlConnection(connString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = "select id, population, ST_AsGeoJSON(boundry) as boundry from subdivisions where id = @id";
                var idParam = cmd.CreateParameter();
                idParam.DbType = System.Data.DbType.Int32;
                idParam.ParameterName = "@id";
                idParam.Value = id;
                conn.Open();
                var reader = cmd.ExecuteReader();
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
    }
}
