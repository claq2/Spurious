using Importers.DataLayer;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BoundryImporter
{
    class Program
    {
        static void Main(string[] args)
        {
            // Massive mem use here somewhere - about 1 GB
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var gmlDoc = new XmlDocument();
            gmlDoc.Load(Path.Combine(userProfile, ConfigurationManager.AppSettings["GeographyFile"]));
            var ns = new XmlNamespaceManager(gmlDoc.NameTable);
            ns.AddNamespace("gml", "http://www.opengis.net/gml");
            ns.AddNamespace("fme", "http://www.safe.com/gml/fme");
            var nodes = gmlDoc.SelectNodes("/gml:FeatureCollection/gml:featureMember", ns);

            var boundaries = new List<BoundaryItem>();
            foreach (XmlNode node in nodes)
            {
                boundaries.Add(new BoundaryItem(ns, node));
            }

            var collection = new BoundaryItemCollection { Items = boundaries };
            var importer = new NpgsqlBulkImporter(ConfigurationManager.ConnectionStrings["spurious"].ConnectionString);
            importer.BulkImport("subdivisions", collection);
            Console.WriteLine("Bulk update complete");

            using (var wrapper = new NpgsqlConnectionWrapper(ConfigurationManager.ConnectionStrings["spurious"].ConnectionString))
            {
                wrapper.Connection.Open();
                var rowsGeoUpdated = wrapper.ExecuteNonQuery("update subdivisions s set (boundry) = ((select ST_FlipCoordinates(ST_GeomFromGML(boundary_gml)) from subdivisions ss where s.id = ss.id))");
                Console.WriteLine($"Updated {rowsGeoUpdated} rows geo data from GML data");
            }

            Console.WriteLine("Node count {0}", nodes.Count);
        }
    }
}
