using System;
using System.Configuration;
using System.IO;
using System.Linq;
using CsvHelper;
using System.Diagnostics;
using Importers.DataLayer;
using CsvHelper.Configuration;

namespace PopulationImporter
{
    class Program
    {
        static Stopwatch importStopwatch = new Stopwatch();

        static void Main(string[] args)
        {
            // Sample top of file. Note the first line that isn't CSV.
            // Census Profile -Census Subdivisions(CSDs)
            // Geo_Code,Prov_Name,CD_Name,CSD_Name,CSD_Type,Topic,Characteristics,Note,Total,Flag_Total,Male,Flag_Male,Female,Flag_Female
            // 1001101,Newfoundland and Labrador,Division No.  1,"Division No.  1, Subd. V",Subdivision of unorganized,Population and dwelling counts, Population in 2011,1,62,,,...,,...
            importStopwatch.Start();
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var fileStream = File.OpenText(Path.Combine(userProfile, ConfigurationManager.AppSettings["PopulationFile"]));
            // Advance reader 1 line to skip stupid non-CSV first line.
            fileStream.ReadLine();
            var csv = new CsvReader(fileStream);

            csv.Configuration.RegisterClassMap(new PopulationLineMap());
            var populationLines = csv.GetRecords<PopulationLine>().Where(pl => pl.Characteristics == "Population in 2011");
            var populationLineCollection = new PopulationLineCollection() { Items = populationLines };

            var bulkImporter = new NpgsqlBulkImporter(ConfigurationManager.ConnectionStrings["spurious"].ConnectionString, importStopwatch);

            bulkImporter.BulkImport("subdivisions", populationLineCollection);

            importStopwatch.Stop();
            Console.WriteLine("Imported data in {0}", importStopwatch.Elapsed);
        }

        static void Something<T, U>(string dataFile, CsvClassMap classMap) where T : IStatsCanCsvLine, IItem where U : LineCollection<T>, new()
        {
            importStopwatch.Start();
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var fileStream = File.OpenText(Path.Combine(userProfile, ConfigurationManager.AppSettings[dataFile]));
            // Advance reader 1 line to skip stupid non-CSV first line.
            fileStream.ReadLine();
            var csv = new CsvReader(fileStream);

            csv.Configuration.RegisterClassMap(classMap);
            var populationLines = csv.GetRecords<T>().Where(pl => pl.Characteristics == "Population in 2011");
            var lineCollection = new U();
            lineCollection.Items = populationLines;

            var bulkImporter = new NpgsqlBulkImporter(ConfigurationManager.ConnectionStrings["spurious"].ConnectionString, importStopwatch);

            bulkImporter.BulkImport("subdivisions", lineCollection);
            importStopwatch.Stop();
            Console.WriteLine("Imported data in {0}", importStopwatch.Elapsed);
        }

        class PopulationCsvQuery
        {
            string query = "Population in 2011";
            Func<PopulationLine, bool> queryLambda = (pl) => pl.Characteristics == "Pop";
        }
    }
}