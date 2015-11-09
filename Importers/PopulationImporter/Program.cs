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
            var populationQuery = new PopulationCsvQuery();
            Something2(populationQuery);

            // Sample top of file. Note the first line that isn't CSV.
            // Census Profile -Census Subdivisions(CSDs)
            // Geo_Code,Prov_Name,CD_Name,CSD_Name,CSD_Type,Topic,Characteristics,Note,Total,Flag_Total,Male,Flag_Male,Female,Flag_Female
            // 1001101,Newfoundland and Labrador,Division No.  1,"Division No.  1, Subd. V",Subdivision of unorganized,Population and dwelling counts, Population in 2011,1,62,,,...,,...
            //importStopwatch.Start();
            //var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            //var fileStream = File.OpenText(Path.Combine(userProfile, ConfigurationManager.AppSettings["PopulationFile"]));
            //// Advance reader 1 line to skip stupid non-CSV first line.
            //fileStream.ReadLine();
            //var csv = new CsvReader(fileStream);

            //csv.Configuration.RegisterClassMap(new PopulationLineMap());
            //var populationLines = csv.GetRecords<PopulationLine>().Where(pl => pl.Characteristics == "Population in 2011");
            //var populationLineCollection = new PopulationLineCollection();
            //populationLineCollection.SetItems(populationLines);

            //var bulkImporter = new NpgsqlBulkImporter(ConfigurationManager.ConnectionStrings["spurious"].ConnectionString, importStopwatch);

            //bulkImporter.BulkImport("subdivisions", populationLineCollection);

            //importStopwatch.Stop();
            //Console.WriteLine("Imported data in {0}", importStopwatch.Elapsed);
        }

        static void Something2<T>(ICsvQuery<T> thequery) where T : IItem
        {
            importStopwatch.Start();
            var fileStream = File.OpenText(thequery.Filename);
            // Advance reader 1 line to skip stupid non-CSV first line.
            fileStream.ReadLine();
            var csv = new CsvReader(fileStream);

            csv.Configuration.RegisterClassMap(thequery.CsvMap);
            var populationLines = csv.GetRecords<T>().Where(thequery.LinePredicate);
            thequery.ItemCollection.SetItems(populationLines);

            var bulkImporter = new NpgsqlBulkImporter(ConfigurationManager.ConnectionStrings["spurious"].ConnectionString, importStopwatch);

            bulkImporter.BulkImport(thequery.TableName, thequery.ItemCollection);
            importStopwatch.Stop();
            Console.WriteLine($"Imported {typeof(T)} data in {0}", importStopwatch.Elapsed);
        }

        class PopulationCsvQuery : ICsvQuery<PopulationLine>
        {
            private readonly IItemCollection<PopulationLine> itemCollection = new PopulationLineCollection();
            public Func<PopulationLine, bool> LinePredicate { get { return (pl) => pl.Characteristics == "Population in 2011"; } }
            public string TableName { get { return "subdivisions"; } }
            public CsvClassMap CsvMap { get { return new PopulationLineMap(); } }
            public IItemCollection<PopulationLine> ItemCollection { get { return itemCollection; } }
            public string Filename
            {
                get
                {
                    var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    return Path.Combine(userProfile, ConfigurationManager.AppSettings["PopulationFile"]);
                }
            }
        }

        interface ICsvQuery<T>
        {
            Func<T, bool> LinePredicate { get; }// = (pl) => pl.Characteristics == "Population in 2011";
            string TableName { get; }// = "subdivisions";
            CsvClassMap CsvMap { get; }// = new PopulationLineMap();
            IItemCollection<T> ItemCollection { get; }// = new PopulationLineCollection();
            string Filename { get; }
        }

    }
}