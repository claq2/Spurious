using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml;
using CsvHelper;
using System.Diagnostics;
namespace PopulationImporter
{
    class Program
    {
        static Stopwatch populationStopwatch = new Stopwatch();

        //declare @p geometry
        //set @p = geometry::Point(1.5, 1.1, 0)
        ///****** Script for SelectTopNRows command from SSMS  ******/
        //SELECT TOP 1000 [Boundry]
        //      ,[Id]
        //      ,[Name]
        //      ,[Population]
        //      ,[CensusSubdivisionId]
        //  FROM [CensusZones].[dbo].[Subdivisions]
        //  where [Boundry].STIntersects(@p) = 1
        //  select @p
        static void Main(string[] args)
        {
            // Sample top of file. Note the first line that isn't CSV.
            // Census Profile -Census Subdivisions(CSDs)
            // Geo_Code,Prov_Name,CD_Name,CSD_Name,CSD_Type,Topic,Characteristics,Note,Total,Flag_Total,Male,Flag_Male,Female,Flag_Female
            // 1001101,Newfoundland and Labrador,Division No.  1,"Division No.  1, Subd. V",Subdivision of unorganized,Population and dwelling counts, Population in 2011,1,62,,,...,,...
            populationStopwatch.Start();
            var fileStream = File.OpenText(ConfigurationManager.AppSettings["PopulationFile"]);
            // Advance rader 1 line to skip stupid non-CSV first line.
            fileStream.ReadLine();
            var csv = new CsvReader(fileStream);

            csv.Configuration.RegisterClassMap(new PopulationLineMap());
            var populationLinesx = csv.GetRecords<PopulationLine>().Where(pl => pl.Characteristics == "Population in 2011");

            Action<PopulationLine, NpgsqlCopySerializer> addPopulationItemToSerializer = (p, s) =>
                {
                    s.AddInt32(p.GeoCode);
                    s.AddInt32(Convert.ToInt32(p.Total));
                };

            BulkImport("spurious", "subdivisions", "id, population", populationLinesx, addPopulationItemToSerializer, new List<string> { "id" }, new List<string> { "population" });

            populationStopwatch.Stop();
            Console.WriteLine("Imported population data in {0}", populationStopwatch.Elapsed);
        }

        /// <summary>
        /// Performs a bulk insert/update/delete based on http://dba.stackexchange.com/questions/73066/efficient-way-to-insert-update-delete-table-records-from-complex-query-in-postgr
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="databaseName"></param>
        /// <param name="targetTable"></param>
        /// <param name="fieldsToPopulateCsv"></param>
        /// <param name="itemsToImport"></param>
        /// <param name="addItemToSerializer"></param>
        private static void BulkImport<T>(string databaseName, string targetTable, string fieldsToPopulateCsv, IEnumerable<T> itemsToImport, Action<T, NpgsqlCopySerializer> addItemToSerializer, List<string> idFieldsToImport = null, List<string> nonIdFieldsToImport = null)
        {
            // TODO: Turn fieldsToPopulate into List<string>
            using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings[databaseName].ConnectionString))
            {
                conn.Open();
                using (var tempTableCmd = CreateCommandWithTimeout(conn))
                {
                    tempTableCmd.CommandText = string.Format("create temp table import_temp as (select * from {0} where 0 = 1)",
                        targetTable);
                    tempTableCmd.ExecuteNonQuery();
                }

                var copyCmd = CreateCommandWithTimeout(conn);
                var idsCsv = string.Join(", ", idFieldsToImport.ToArray());
                var allFieldsToImport = idsCsv + ", " + string.Join(", ", nonIdFieldsToImport);
                copyCmd.CommandText = string.Format("copy import_temp({0}) from stdin", allFieldsToImport);
                var serializer = new NpgsqlCopySerializer(conn);
                var copyIn = new NpgsqlCopyIn(copyCmd, conn, serializer.ToStream);

                try
                {
                    copyIn.Start();

                    itemsToImport.ToList().ForEach(p =>
                    {
                        addItemToSerializer(p, serializer);
                        serializer.EndRow();
                        serializer.Flush();
                    });

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

                Console.WriteLine("Finished temp table fill after {0}", populationStopwatch.Elapsed);

                using (var createIndexOnTemp = CreateCommandWithTimeout(conn))
                {
                    createIndexOnTemp.CommandText = string.Format("create index import_temp_idx on import_temp ({0})", idsCsv);
                    createIndexOnTemp.ExecuteNonQuery();
                }

                using (var analyzeTemp = CreateCommandWithTimeout(conn))
                {
                    analyzeTemp.CommandText = "analyze import_temp";
                    analyzeTemp.ExecuteNonQuery();
                }

                //var ids = new List<string> { "id" };
                StringBuilder idMatchClause = new StringBuilder(string.Format(" it.{0} = t.{0} ", idFieldsToImport[0]));
                idFieldsToImport.Skip(1).ToList().ForEach((id) =>
                {
                    idMatchClause.AppendFormat(" and it.{0} = t.{0} ", id);
                });

                using (var deleteCmd = CreateCommandWithTimeout(conn))
                {
                    deleteCmd.CommandText = string.Format(@"delete from {0} t
                                                    where not exists (
                                                    select 1 from import_temp it
                                                    where {1}
                                                    )", targetTable, idMatchClause.ToString());
                    var rowsDeleted = deleteCmd.ExecuteNonQuery();
                    Console.WriteLine("Deleted {0} rows", rowsDeleted);
                }

                using (var updateCmd = CreateCommandWithTimeout(conn))
                {
                    // TODO: use non ID fields from params
                    updateCmd.CommandText = string.Format(@"update {0} t
                                                    set population = it.population
                                                    from import_temp it
                                                    where {1}
                                                    --it.id = t.id
                                                    --and it.store_id = t.store_id
                                                    and it.population <> t.population", targetTable, idMatchClause);
                    var updatedRows = updateCmd.ExecuteNonQuery();
                    Console.WriteLine("Updated {0} rows", updatedRows);
                }

                using (var insertCmd = CreateCommandWithTimeout(conn))
                {
                    // TODO: use non ID fields from params
                    // insertCmd.CommandText = @"insert into inventories (product_id, store_id, quantity)
                    //                             select x.product_id, x.store_id, x.quantity
                    //                             from inv_temp x
                    //                             left join inventories i using (product_id, store_id)
                    //                             where i.product_id is null";

                    var idsCsvWithItPrefix = string.Join(", ", idFieldsToImport.Select(id => "it." + id));
                    insertCmd.CommandText = string.Format(@"insert into {0} ({2}, population)
                                                    select {3}, it.population
                                                    from import_temp it
                                                    left join {0} t using ({1})
                                                    where t.{1} is null", targetTable, idFieldsToImport.First(), idsCsv, idsCsvWithItPrefix);
                    var insertedRows = insertCmd.ExecuteNonQuery();
                    Console.WriteLine("Added {0} rows", insertedRows);
                }
            }
        }

        private static NpgsqlCommand CreateCommandWithTimeout(NpgsqlConnection connection)
        {
            var result = connection.CreateCommand();
            result.CommandTimeout = 9001;
            return result;
        }
    }
}
