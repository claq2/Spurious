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

        static void Main(string[] args)
        {
            var averageIncomeQuery = new AverageIncomeCsvQuery();
            ImportCsvQuery(averageIncomeQuery);

            var medianIncomeQuery = new MedianIncomeCsvQuery();
            ImportCsvQuery(medianIncomeQuery);

            var medianAfterTaxIncomeQuery = new MedianAfterTaxIncomeCsvQuery();
            ImportCsvQuery(medianAfterTaxIncomeQuery);

            var averageAfterTaxIncomeQuery = new AverageAfterTaxIncomeCsvQuery();
            ImportCsvQuery(averageAfterTaxIncomeQuery);

            var populationQuery = new PopulationCsvQuery();
            ImportCsvQuery(populationQuery);
        }

        static void ImportCsvQuery<T>(ICsvQuery<T> thequery) where T : IItem
        {
            var importStopwatch = new Stopwatch();
            importStopwatch.Start();
            using (var csv = new CsvReader(thequery.FileStream))
            {

                csv.Configuration.RegisterClassMap(thequery.CsvMap);
                var populationLines = csv.GetRecords<T>().Where(thequery.LinePredicate);
                thequery.ItemCollection.SetItems(populationLines);
                var bulkImporter = new NpgsqlBulkImporter(ConfigurationManager.ConnectionStrings["spurious"].ConnectionString, importStopwatch);

                bulkImporter.BulkImport(thequery.TableName, thequery.ItemCollection);
            }

            importStopwatch.Stop();
            Console.WriteLine($"Imported {typeof(T)} data in {importStopwatch.Elapsed}");
            importStopwatch.Reset();
        }
    }
}