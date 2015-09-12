using Importers.Datalayer2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace BoundryImporter
{
    public class BoundaryItem : IItem
    {
        public int GeoCode { get; set; }
        public string BoundaryGml { get; set; }

        public string IdAndDataFieldsAsCsv
        {
            get { return $"{GeoCode},{BoundaryGml}"; }
        }

        public BoundaryItem(XmlNamespaceManager ns, XmlNode node)
        {
            var csidNode = node.SelectSingleNode("fme:gcsd000a11g_e/fme:CSDUID", ns);
            var csdid = Convert.ToInt32(csidNode.InnerText);
            var surfaceNode = node.SelectSingleNode("fme:gcsd000a11g_e/gml:surfaceProperty/gml:Surface", ns);
            var multiSurfaceNode = node.SelectSingleNode("fme:gcsd000a11g_e/gml:multiSurfaceProperty/gml:MultiSurface", ns);
            var gmlNode = surfaceNode != null ? surfaceNode : multiSurfaceNode;
            var gmlText = gmlNode.OuterXml;
            this.GeoCode = csdid;
            this.BoundaryGml = gmlText;
        }
    }
}