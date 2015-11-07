using CsvHelper;
using Importers.DataLayer;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LcboImporter
{
    class Program
    {
        // lcboapi.com/datasets/latest.zip
        static void Main(string[] args)
        {
            var token = ConfigurationManager.AppSettings["LcboToken"];
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", String.Format("Token {0}", token));
#if DEBUG
            var url = "http://localhost/datasets/latest.zip";
#else
            var url = "https://lcboapi.com/datasets/latest.zip";
#endif
            httpClient.Timeout = new TimeSpan(0, 5, 0);
            var resp = httpClient.GetAsync(url).Result;

            if (resp.IsSuccessStatusCode)
            {
                var content = resp.Content;
                using (var x = content.ReadAsStreamAsync().Result)
                {
                    using (var zip = new ZipArchive(x))
                    {
                        // Stores first
                        var storesEntry = zip.Entries.Single(e => e.FullName.ToLowerInvariant() == "stores.csv");
                        if (ConfigurationManager.AppSettings["ProcessStores"].ToLowerInvariant() == "true")
                        {
                            AddUpdateDeleteStores(storesEntry);
                        }

                        // then products
                        var productsEntry = zip.Entries.Single(e => e.FullName.ToLowerInvariant() == "products.csv");
                        if (ConfigurationManager.AppSettings["ProcessProducts"].ToLowerInvariant() == "true")
                        {
                            AddUpdateDeleteProducts(productsEntry);
                        }

                        // then inventories
                        var inventoriesEntry = zip.Entries.Single(e => e.FullName.ToLowerInvariant() == "inventories.csv");
                        if (ConfigurationManager.AppSettings["ProcessInventories"].ToLowerInvariant() == "true")
                        {
                            AddUpdateDeleteInventories(inventoriesEntry);
                        }

                        // Populate store volume columns
                        PopulateStoresVolumes();

                        // Update all subdivisions' volumes
                        UpdateSubdivisionVolumes();
                    }
                }
            }
            else
            {
                Console.WriteLine($"Couldn't get latest dataset! Error was {resp.StatusCode}");
            }
        }

        private static void PopulateStoresVolumes()
        {
            Console.WriteLine("Starting stores volumes at {0:hh:mm:ss.fff}", DateTime.Now);
            var volumesTimer = new Stopwatch();
            volumesTimer.Start();
            using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["spurious"].ConnectionString))
            {
                conn.Open();
                var updateVolumesCommand = conn.CreateCommand();
                updateVolumesCommand.CommandText = @"update stores 
                                                        set (beer_volume, wine_volume, spirits_volume) = 
                                                        (
	                                                        (select sum(i.quantity * p.volume)
	                                                        from inventories i
	                                                        inner join products p on p.id = i.product_id
	                                                        where i.store_id = stores.id
	                                                        and p.category = 'Beer')

	                                                        ,(select sum(i.quantity * p.volume)
	                                                        from inventories i
	                                                        inner join products p on p.id = i.product_id
	                                                        where i.store_id = stores.id
	                                                        and p.category = 'Wine')

	                                                        ,(select sum(i.quantity * p.volume)
	                                                        from inventories i
	                                                        inner join products p on p.id = i.product_id
	                                                        where i.store_id = stores.id
	                                                        and p.category = 'Spirit')
                                                        )";
                var rowsUpdated = updateVolumesCommand.ExecuteNonQuery();
                if (rowsUpdated < 500)
                {
                    Console.WriteLine("Not all volumes updated! Only {0} updated!", rowsUpdated);
                }

                volumesTimer.Stop();
                Console.WriteLine("Finished stores volumes update at {0:hh:mm:ss.fff}, taking {1}", DateTime.Now, volumesTimer.Elapsed);
            }
        }

        private static void AddUpdateDeleteStores(ZipArchiveEntry entry)
        {
            IEnumerable<Store> stores = null;

            using (var entryStream = entry.Open())
            {
                Console.WriteLine("stores.csv is {0} bytes", entry.Length);
                Console.WriteLine("Starting stores at {0:hh:mm:ss.fff}", DateTime.Now);
                var storeTimer = new Stopwatch();
                storeTimer.Start();
                var reader = new StreamReader(entryStream);
                var csv = new CsvReader(reader);
                csv.Configuration.RegisterClassMap(new StoreMap());
                stores = csv.GetRecords<Store>().Where(s => !s.IsDead);

                var storesCollection = new StoreCollection() { Items = stores };
                var importer = new NpgsqlBulkImporter(ConfigurationManager.ConnectionStrings["spurious"].ConnectionString, storeTimer);
                importer.BulkImport("stores", storesCollection);

                using (var wrapper = new NpgsqlConnectionWrapper(ConfigurationManager.ConnectionStrings["spurious"].ConnectionString))
                {
                    wrapper.Connection.Open();
                    var rowsGeoUpdated = wrapper.ExecuteNonQuery("update stores s set (location) = ((select ST_SetSRID(ST_MakePoint(longitude, latitude), 4326) from stores ss where s.id = ss.id))");
                    Console.WriteLine($"Updated {rowsGeoUpdated} rows geo data from lat/long data");
                }

                storeTimer.Stop();
                Console.WriteLine("Finished stores at {0:hh:mm:ss.fff}, taking {1}", DateTime.Now, storeTimer.Elapsed);
            }
        }

        private static void AddUpdateDeleteProducts(ZipArchiveEntry entry)
        {
            IEnumerable<Product> products = null;
            using (var entryStream = entry.Open())
            {
                Console.WriteLine("products.csv is {0} bytes", entry.Length);
                Console.WriteLine("Starting products at {0:hh:mm:ss.fff}", DateTime.Now);
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var reader = new StreamReader(entryStream);
                var csv = new CsvReader(reader);
                csv.Configuration.RegisterClassMap(new ProductMap());
                products = csv.GetRecords<Product>().Where(p => !p.IsDead);
                var productCollection = new ProductCollection { Items = products };
                var importer = new NpgsqlBulkImporter(ConfigurationManager.ConnectionStrings["spurious"].ConnectionString, stopwatch);
                importer.BulkImport("products", productCollection);

                stopwatch.Stop();
                Console.WriteLine("Finished products at {0:hh:mm:ss.fff}, taking {1}", DateTime.Now, stopwatch.Elapsed);
            }
        }

        private static void AddUpdateDeleteInventories(ZipArchiveEntry inventoriesEntry)
        {
            Console.WriteLine("inventories.csv is {0} bytes", inventoriesEntry.Length);
            IEnumerable<Inventory> inventories = null;

            using (var entryStream = inventoriesEntry.Open())
            {
                Console.WriteLine("Starting inventories at {0:hh:mm:ss.fff}", DateTime.Now);
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var reader = new StreamReader(entryStream);
                var csv = new CsvReader(reader);
                csv.Configuration.RegisterClassMap(new InventoryMap());
                StringBuilder invForImport = new StringBuilder();
                inventories = csv.GetRecords<Inventory>().Where(i => !i.IsDead);

                var inventoryCollection = new InventoryCollection { Items = inventories };
                var importer = new NpgsqlBulkImporter(ConfigurationManager.ConnectionStrings["spurious"].ConnectionString, stopwatch);
                importer.BulkImport("inventories", inventoryCollection);

                stopwatch.Stop();
                Console.WriteLine("Finished inventories at {0:hh:mm:ss.fff}, taking {1}", DateTime.Now, stopwatch.Elapsed);
            }
        }

        private static void UpdateSubdivisionVolumes()
        {
            // update subdivisions sd set (beer_volume, wine_volume, spirits_volume) = ((select sum(s.beer_volume) from stores s where ST_Intersects(sd.boundry, s.location)), (select sum(s.wine_volume) from stores s where ST_Intersects(sd.boundry, s.location)), (select sum(s.spirits_volume) from stores s where ST_Intersects(sd.boundry, s.location)));
            Console.WriteLine("Starting subdivisions volumes at {0:hh:mm:ss.fff}", DateTime.Now);
            var volumesTimer = new Stopwatch();
            volumesTimer.Start();
            using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["spurious"].ConnectionString))
            {
                conn.Open();
                var updateVolumesCommand = conn.CreateCommand();
                updateVolumesCommand.CommandTimeout = 9001;
                updateVolumesCommand.CommandText = @"update subdivisions sd 
                                                        set (beer_volume, wine_volume, spirits_volume) = 
                                                        (
                                                            (select sum(s.beer_volume) 
                                                            from stores s 
                                                            where ST_Intersects(sd.boundry, s.location)), 
                                        
                                                            (select sum(s.wine_volume) 
                                                            from stores s 
                                                            where ST_Intersects(sd.boundry, s.location)), 

                                                            (select sum(s.spirits_volume) 
                                                            from stores s 
                                                            where ST_Intersects(sd.boundry, s.location))
                                                        )";
                var rowsUpdated = updateVolumesCommand.ExecuteNonQuery();
                if (rowsUpdated < 500)
                {
                    Console.WriteLine("Not all volumes updated! Only {0} updated!", rowsUpdated);
                }

                volumesTimer.Stop();
                Console.WriteLine("Finished subdivisions volumes update at {0:hh:mm:ss.fff}, taking {1}", DateTime.Now, volumesTimer.Elapsed);
            }
        }
    }
}
