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
            var gmlDoc = new XmlDocument();
            gmlDoc.Load(ConfigurationManager.AppSettings["GeographyFile"]);
            var ns = new XmlNamespaceManager(gmlDoc.NameTable);
            ns.AddNamespace("gml", "http://www.opengis.net/gml");
            ns.AddNamespace("fme", "http://www.safe.com/gml/fme");
            var nodes = gmlDoc.SelectNodes("/gml:FeatureCollection/gml:featureMember", ns);
            foreach (XmlNode node in nodes)
            {
                var csidNode = node.SelectSingleNode("fme:gcsd000a11g_e/fme:CSDUID", ns);
                var csdid = Convert.ToInt32(csidNode.InnerText);
                var surfaceNode = node.SelectSingleNode("fme:gcsd000a11g_e/gml:surfaceProperty/gml:Surface", ns);
                var multiSurfaceNode = node.SelectSingleNode("fme:gcsd000a11g_e/gml:multiSurfaceProperty/gml:MultiSurface", ns);
                var gmlNode = surfaceNode != null ? surfaceNode : multiSurfaceNode;
                var gmlText = gmlNode.OuterXml;
                using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["spurious"].ConnectionString))
                {
                    conn.Open();
                    var selectCommand = conn.CreateCommand();
                    selectCommand.CommandType = CommandType.Text;
                    selectCommand.CommandText = "select id from subdivisions where id = " + csdid;
                    var existingId = selectCommand.ExecuteScalar();

                    if (existingId == null)
                    {
                        var insertCommand = conn.CreateCommand();
                        insertCommand.CommandText = @"INSERT INTO subdivisions(
                                                        id, boundry)
                                                        VALUES (" + csdid + ", ST_FlipCoordinates(ST_GeomFromGML('" + gmlText + "')));";
                        insertCommand.CommandType = CommandType.Text;
                        var rowCount = insertCommand.ExecuteNonQuery();
                        Console.WriteLine("Added subdivision {0}.", csdid);
                    }
                    else
                    {
                        var updateCommand = conn.CreateCommand();
                        updateCommand.CommandText = @"update subdivisions set boundry = ST_FlipCoordinates(ST_GeomFromGML('" + gmlText + "')) where id = " + csdid;
                        var rowCount = updateCommand.ExecuteNonQuery();
                        Console.WriteLine("Updated subdivision {0}.", csdid);
                    }
                }
            }

            Console.WriteLine("Node count {0}", nodes.Count);
        }
    }
}
