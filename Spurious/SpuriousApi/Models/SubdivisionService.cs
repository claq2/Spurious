using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using GeoJSON.Net.Geometry;
using GeoJSON.Net.Feature;

namespace SpuriousApi.Models
{
    public class SubdivisionService
    {
        private readonly string connString = ConfigurationManager.ConnectionStrings["spurious"].ConnectionString;
        private readonly Dictionary<AlcoholType, string> alcoholTypeVolumeFieldMap = new Dictionary<AlcoholType, string> { { AlcoholType.Beer, "beer_volume" }, { AlcoholType.Spirits, "spirits_volume" }, { AlcoholType.Wine, "wine_volume" } };

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
            var query = @"select id, 
                            population,
                            name,
                            St_AsGeoJSON(st_centroid(boundry::geometry)) as centre,
                            beer_volume, 
                            wine_volume, 
                            spirits_volume, 
                            (beer_volume + wine_volume + spirits_volume) / population as density
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

                if (result.Any())
                {
                    var stores = await GetStoresForSubdivisions(result.Select(s => s.Id), conn);
                    foreach (var store in stores)
                    {
                        resultDict[store.SubdivisionId].LcboStores.Add(store);
                    }
                }
            }

            return result;
        }

        public async Task<List<Subdivision>> Top10WineDensity()
        {
            var result = new List<Subdivision>();
            var query = @"select id, 
                            population, 
                            name,
                            St_AsGeoJSON(st_centroid(boundry::geometry)) as centre, 
                            wine_volume,
                            wine_volume / population as density
                            from subdivisions
                            where wine_volume > 0
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

                if (result.Any())
                {
                    var stores = await GetStoresForSubdivisions(result.Select(s => s.Id), conn);
                    foreach (var store in stores)
                    {
                        resultDict[store.SubdivisionId].LcboStores.Add(store);
                    }
                }
            }

            return result;
        }

        public async Task<List<Subdivision>> Top10Density(AlcoholType alcoholType)
        {
            var volumeField = alcoholTypeVolumeFieldMap[alcoholType];
            var result = new List<Subdivision>();
            var query = $@"select id, 
                            population, 
                            name,
                            St_AsGeoJSON(st_centroid(boundry::geometry)) as centre, 
                            {volumeField},
                            {volumeField} / population as density
                            from subdivisions
                            where {volumeField} > 0
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

                if (result.Any())
                {
                    var stores = await GetStoresForSubdivisions(result.Select(s => s.Id), conn);
                    foreach (var store in stores)
                    {
                        resultDict[store.SubdivisionId].LcboStores.Add(store);
                    }
                }
            }

            return result;
        }

        public async Task<Feature> BoundaryGeoJson(int subdivId)
        {
            var result = new Feature(new Point(new Position()));
            var query = @"select ST_AsGeoJSON(boundry, 15, 4) as boundary
                            from subdivisions
                            where id = @subdivId";

            using (var conn = new Npgsql.NpgsqlConnection(connString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    conn.Open();
                    cmd.Parameters.AddWithValue("@subdivId", subdivId);
                    var boundary = await cmd.ExecuteScalarAsync() as string;
                    if (boundary.Contains("\"type\":\"MultiPolygon\""))
                    {
                        var multipolygon = JsonConvert.DeserializeObject<MultiPolygon>(boundary);
                        result = new Feature(multipolygon);
                    }
                    else if (boundary.Contains("\"type\":\"Polygon\""))
                    {
                        var polygon = JsonConvert.DeserializeObject<Polygon>(boundary);
                        result = new Feature(polygon);
                    }
                }
            }

            return result;
        }

        private static async Task<List<LcboStore>> GetStoresForSubdivisions(IEnumerable<int> subdivisionIds, Npgsql.NpgsqlConnection conn)
        {
            var result = new List<LcboStore>();
            using (var getStoresCmd = conn.CreateCommand())
            {
                var subdivIds = string.Join(", ", subdivisionIds);
                getStoresCmd.CommandText = $@"select sd.id as subdivision_id,
                                                s.id as id, 
                                                s.name as name, 
                                                s.city as city,
                                                ST_AsGeoJSON(s.location) as location, 
                                                s.beer_volume, 
                                                s.wine_volume, 
                                                s.spirits_volume from subdivisions sd 
                                                inner join stores s on ST_Intersects(s.location, sd.boundry)
                                                where sd.id in ({subdivIds})
                                                order by subdivision_id";
                using (var storesReader = await getStoresCmd.ExecuteReaderAsync())
                {
                    while (storesReader.Read())
                    {
                        result.Add(new LcboStore(storesReader));
                    }
                }
            }

            return result;
        }
    }

    public enum AlcoholType
    {
        Beer,
        Spirits,
        Wine,
    }
}
