using CsvHelper;
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

                        // Populate volume columns
                        PopulateVolumes();
                    }
                }
            }
            else
            {
                Console.WriteLine("Couldn't get latest dataset!");
            }
        }

        private static void PopulateVolumes()
        {
            Console.WriteLine("Starting volumes at {0:hh:mm:ss.fff}", DateTime.Now);
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
                Console.WriteLine("Finished volumes update at {0:hh:mm:ss.fff}, taking {1}", DateTime.Now, volumesTimer.Elapsed);
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
                stores = csv.GetRecords<Store>();
                using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["spurious"].ConnectionString))
                {
                    conn.Open();
                    foreach (Store store in stores)
                    {
                        const string selectText = "select id from stores where id = :id";
                        const string deleteText = "delete from stores where id = :id";
                        const string updateText = "update stores set name = :name, city = :city, location = ST_SetSRID(ST_MakePoint(:long, :lat), 4326) where id = :id";
                        const string insertText = "insert into stores(id, name, city, location) values (:id, :name, :city, ST_SetSRID(ST_MakePoint(:long, :lat), 4326))";

                        if (store.IsDead)
                        {
                            // Remove dead store
                            var deleteCmd = conn.CreateCommand();
                            deleteCmd.CommandText = deleteText;

                            var idParam = deleteCmd.CreateParameter();
                            idParam.Value = store.Id;
                            idParam.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Integer;
                            idParam.ParameterName = "id";

                            deleteCmd.Parameters.Add(idParam);
                            var rowsDeleted = deleteCmd.ExecuteNonQuery();
                            if (rowsDeleted != 0 && rowsDeleted != 1)
                            {
                                Console.WriteLine("Deleted {0} rows!", rowsDeleted);
                            }
                        }
                        else
                        {
                            // Add or update store
                            var selectCmd = conn.CreateCommand();
                            selectCmd.CommandText = selectText;

                            var idParam = selectCmd.CreateParameter();
                            idParam.Value = store.Id;
                            idParam.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Integer;
                            idParam.ParameterName = "id";

                            selectCmd.Parameters.Add(idParam);
                            var existingId = selectCmd.ExecuteScalar();
                            selectCmd.Parameters.Clear();

                            var cityParam = new NpgsqlParameter("city", NpgsqlTypes.NpgsqlDbType.Text);
                            cityParam.Value = store.City;

                            var longitudeParam = new NpgsqlParameter("long", NpgsqlTypes.NpgsqlDbType.Real);
                            longitudeParam.Value = store.Longitude;

                            var latitudeParam = new NpgsqlParameter("lat", NpgsqlTypes.NpgsqlDbType.Real);
                            latitudeParam.Value = store.Latitude;

                            if (existingId != null)
                            {
                                // Update
                                var updateCmd = conn.CreateCommand();
                                updateCmd.CommandText = updateText;

                                var nameParam = updateCmd.CreateParameter();
                                nameParam.Value = store.Name;
                                nameParam.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Text;
                                nameParam.ParameterName = "name";

                                updateCmd.Parameters.Add(idParam);
                                updateCmd.Parameters.Add(nameParam);
                                updateCmd.Parameters.Add(cityParam);
                                updateCmd.Parameters.Add(longitudeParam);
                                updateCmd.Parameters.Add(latitudeParam);
                                var rowsUpdatedCount = updateCmd.ExecuteNonQuery();
                                if (rowsUpdatedCount != 1)
                                {
                                    Console.WriteLine("Updated {0} rows!", rowsUpdatedCount);
                                }
                            }
                            else
                            {
                                // Insert
                                var insertCmd = conn.CreateCommand();
                                insertCmd.CommandText = insertText;

                                var nameParam = insertCmd.CreateParameter();
                                nameParam.Value = store.Name;
                                nameParam.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Text;
                                nameParam.ParameterName = "name";

                                insertCmd.Parameters.Add(idParam);
                                insertCmd.Parameters.Add(nameParam);
                                insertCmd.Parameters.Add(cityParam);
                                insertCmd.Parameters.Add(longitudeParam);
                                insertCmd.Parameters.Add(latitudeParam);
                                var rowsInsertedCount = insertCmd.ExecuteNonQuery();
                                if (rowsInsertedCount != 1)
                                {
                                    Console.WriteLine("Inserted {0} rows!", rowsInsertedCount);
                                }
                            }
                        }
                    }
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
                products = csv.GetRecords<Product>();
                using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["spurious"].ConnectionString))
                {
                    conn.Open();
                    foreach (var product in products)
                    {
                        const string selectText = "select id from products where id = :id";
                        const string deleteText = "delete from products where id = :id";
                        const string updateText = "update products set name = :name, category = :category, volume = :volume where id = :id";
                        const string insertText = "insert into products(id, name, category, volume) values (:id, :name, :category, :volume)";
                        if (product.IsDead)
                        {
                            // Remove dead store
                            var deleteCmd = conn.CreateCommand();
                            deleteCmd.CommandText = deleteText;

                            var idParam = deleteCmd.CreateParameter();
                            idParam.Value = product.Id;
                            idParam.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Integer;
                            idParam.ParameterName = "id";

                            deleteCmd.Parameters.Add(idParam);
                            var rowsDeleted = deleteCmd.ExecuteNonQuery();
                            if (rowsDeleted != 0 && rowsDeleted != 1)
                            {
                                Console.WriteLine("Deleted {0} rows!", rowsDeleted);
                            }
                        }
                        else
                        {
                            // Add or update product
                            var selectCmd = conn.CreateCommand();
                            selectCmd.CommandText = selectText;

                            var idParam = selectCmd.CreateParameter();
                            idParam.Value = product.Id;
                            idParam.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Integer;
                            idParam.ParameterName = "id";

                            selectCmd.Parameters.Add(idParam);
                            var existingId = selectCmd.ExecuteScalar();
                            selectCmd.Parameters.Clear();

                            var nameParam = new NpgsqlParameter("name", NpgsqlTypes.NpgsqlDbType.Text);
                            nameParam.Value = product.Name;

                            var categoryParam = new NpgsqlParameter("category", NpgsqlTypes.NpgsqlDbType.Text);
                            categoryParam.Value = product.Category;

                            var volumeParam = new NpgsqlParameter("volume", NpgsqlTypes.NpgsqlDbType.Integer);
                            volumeParam.Value = product.Volume;

                            if (existingId != null)
                            {
                                // Update
                                var updateCmd = conn.CreateCommand();
                                updateCmd.CommandText = updateText;

                                updateCmd.Parameters.Add(idParam);
                                updateCmd.Parameters.Add(nameParam);
                                updateCmd.Parameters.Add(categoryParam);
                                updateCmd.Parameters.Add(volumeParam);
                                var rowsUpdatedCount = updateCmd.ExecuteNonQuery();
                                if (rowsUpdatedCount != 1)
                                {
                                    Console.WriteLine("Updated {0} rows!", rowsUpdatedCount);
                                }
                            }
                            else
                            {
                                // Insert
                                var insertCmd = conn.CreateCommand();
                                insertCmd.CommandText = insertText;

                                insertCmd.Parameters.Add(idParam);
                                insertCmd.Parameters.Add(nameParam);
                                insertCmd.Parameters.Add(categoryParam);
                                insertCmd.Parameters.Add(volumeParam);
                                var rowsInsertedCount = insertCmd.ExecuteNonQuery();
                                if (rowsInsertedCount != 1)
                                {
                                    Console.WriteLine("Inserted {0} rows!", rowsInsertedCount);
                                }
                            }
                        }
                    }
                }

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
                inventories = csv.GetRecords<Inventory>();

                // Bulk import
                using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["spurious"].ConnectionString))
                {
                    conn.Open();
                    using (var tempTableCmd = conn.CreateCommand())
                    {
                        tempTableCmd.CommandText = "create temp table inv_temp as (select * from inventories where 0 = 1)";
                        tempTableCmd.ExecuteNonQuery();
                    }

                    var copyCmd = conn.CreateCommand();
                    copyCmd.CommandTimeout = 9001;
                    copyCmd.CommandText = "copy inv_temp(product_id, store_id, quantity) from stdin";
                    var serializer = new NpgsqlCopySerializer(conn);
                    var copyIn = new NpgsqlCopyIn(copyCmd, conn, serializer.ToStream);

                    try
                    {
                        copyIn.Start();
                        foreach (Inventory inventory in inventories)
                        {
                            if (!inventory.IsDead)
                            {
                                serializer.AddInt32(inventory.ProductId);
                                serializer.AddInt32(inventory.StoreId);
                                serializer.AddInt32(inventory.Quantity);
                                serializer.EndRow();
                                serializer.Flush();
                            }
                        }

                        copyIn.End();
                        serializer.Close();
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            copyIn.Cancel("Cancel copy on exception");
                        }
                        catch (NpgsqlException se)
                        {
                            if (!se.BaseMessage.Contains("Cancel copy on exception"))
                            {
                                throw new Exception(string.Format("Failed to cancel copy: {0} upon failure: {1}", se, e));
                            }
                        }

                        throw;
                    }

                    using (var createIndexOnTemp = conn.CreateCommand())
                    {
                        createIndexOnTemp.CommandText = "create index inv_temp_idx on inv_temp (product_id, store_id)";
                        createIndexOnTemp.ExecuteNonQuery();
                    }

                    using (var analyzeTemp = conn.CreateCommand())
                    {
                        analyzeTemp.CommandText = "analyze inv_temp";
                        analyzeTemp.ExecuteNonQuery();
                    }

                    using (var deleteCmd = conn.CreateCommand())
                    {
                        deleteCmd.CommandTimeout = 9001;
                        deleteCmd.CommandText = @"delete from inventories i
                                                    where not exists (
                                                    select 1 from inv_temp x
                                                    where x.product_id = i.product_id
                                                    and x.store_id = i.store_id)";
                        var rowsDeleted = deleteCmd.ExecuteNonQuery();
                        Console.WriteLine("Deleted {0} rows", rowsDeleted);
                    }

                    using (var updateCmd = conn.CreateCommand())
                    {
                        updateCmd.CommandTimeout = 9001;
                        updateCmd.CommandText = @"update inventories i
                                                    set quantity = x.quantity
                                                    from inv_temp x
                                                    where x.product_id = i.product_id
                                                    and x.store_id = i.store_id
                                                    and x.quantity <> i.quantity";
                        var updatedRows = updateCmd.ExecuteNonQuery();
                        Console.WriteLine("Updated {0} rows", updatedRows);
                    }

                    using (var insertCmd = conn.CreateCommand())
                    {
                        insertCmd.CommandTimeout = 9001;
                        insertCmd.CommandText = @"insert into inventories (product_id, store_id, quantity)
                                                    select x.product_id, x.store_id, x.quantity
                                                    from inv_temp x
                                                    left join inventories i using (product_id, store_id)
                                                    where i.product_id is null";
                        var insertedRows = insertCmd.ExecuteNonQuery();
                        Console.WriteLine("Added {0} rows", insertedRows);
                    }
                }

                stopwatch.Stop();
                Console.WriteLine("Finished inventories at {0:hh:mm:ss.fff}, taking {1}", DateTime.Now, stopwatch.Elapsed);
            }
        }
    }
}
