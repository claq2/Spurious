using System;
using System.Configuration;
using System.IO;
using System.Linq;
using Importers.DataLayer;
using CsvHelper.Configuration;

namespace PopulationImporter
{
    class PopulationCsvQuery : ICsvQuery<PopulationLine>
    {
        private readonly IItemCollection<PopulationLine> itemCollection = new PopulationLineCollection();

        public Func<PopulationLine, bool> LinePredicate { get { return (pl) => pl.Characteristic == "Population in 2011"; } }

        public string TableName { get { return "subdivisions"; } }

        public CsvClassMap CsvMap { get { return new PopulationLineMap(); } }

        public IItemCollection<PopulationLine> ItemCollection { get { return itemCollection; } }

        public StreamReader FileStream
        {
            get
            {
                var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var filename = Path.Combine(userProfile, ConfigurationManager.AppSettings["PopulationFile"]);
                var result = File.OpenText(filename);
                // Advance reader 1 line to skip stupid non-CSV first line.
                result.ReadLine();
                return result;
            }
        }
    }

    abstract class AbstractIncomeCsvQuery : ICsvQuery<IncomeLine>
    {
        public abstract Func<IncomeLine, bool> LinePredicate { get; }

        public string TableName { get { return "subdivisions"; } }

        public CsvClassMap CsvMap { get { return new IncomeLineMap(); } }

        public abstract IItemCollection<IncomeLine> ItemCollection { get; }

        public StreamReader FileStream
        {
            get
            {
                var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var filename = Path.Combine(userProfile, ConfigurationManager.AppSettings["IncomeFile"]);
                var result = File.OpenText(filename);
                return result;
            }
        }
    }

    class AverageIncomeCsvQuery : AbstractIncomeCsvQuery, ICsvQuery<IncomeLine>
    {
        private readonly IItemCollection<IncomeLine> itemCollection = new AverageIncomeLineCollection();

        public override Func<IncomeLine, bool> LinePredicate { get { return (pl) => pl.Characteristic.Trim() == "Average income ($)"; } }

        public override IItemCollection<IncomeLine> ItemCollection { get { return itemCollection; } }
    }

    class MedianIncomeCsvQuery : AbstractIncomeCsvQuery, ICsvQuery<IncomeLine>
    {
        private readonly IItemCollection<IncomeLine> itemCollection = new MedianIncomeLineCollection();

        public override Func<IncomeLine, bool> LinePredicate
        {
            get { return (il) => il.Characteristic.Trim() == "Median income ($)"; }
        }

        public override IItemCollection<IncomeLine> ItemCollection
        {
            get { return this.itemCollection; }
        }
    }

    class MedianAfterTaxIncomeCsvQuery : AbstractIncomeCsvQuery, ICsvQuery<IncomeLine>
    {
        private readonly IItemCollection<IncomeLine> itemCollection = new MedianAfterTaxIncomeLineCollection();

        public override Func<IncomeLine, bool> LinePredicate
        {
            get { return (il) => il.Characteristic.Trim() == "Median after-tax income ($)"; }
        }

        public override IItemCollection<IncomeLine> ItemCollection
        {
            get { return this.itemCollection; }
        }
    }

    class AverageAfterTaxIncomeCsvQuery : AbstractIncomeCsvQuery, ICsvQuery<IncomeLine>
    {
        private readonly IItemCollection<IncomeLine> itemCollection = new AverageAfterTaxIncomeLineCollection();

        public override Func<IncomeLine, bool> LinePredicate
        {
            get { return (il) => il.Characteristic.Trim() == "Average after-tax income ($)"; }
        }

        public override IItemCollection<IncomeLine> ItemCollection
        {
            get { return this.itemCollection; }
        }
    }
}
