using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using QuickGraph;

namespace FiniteAutomate
{
    class GMLExpSerializer
    {
        XmlWriter writer;
        BidirectionalGraph<object, IEdge<object>> graph;

        public GMLExpSerializer(XmlWriter writer, BidirectionalGraph<object, IEdge<object>> graph)
        {
            this.writer = writer;
            this.graph = graph;
        }

        public void Serialize()
        {
            string graphMLNamespace = "http://graphml.graphdrawing.org/xmlns";
            writer.WriteStartDocument();
            writer.WriteStartElement("", "graphml", graphMLNamespace);
            writer.WriteStartElement("graph", graphMLNamespace);
            writer.WriteAttributeString("id", "G");
            writer.WriteAttributeString("edgedefault", graph.IsDirected ? "directed" : "undirected");
            writer.WriteAttributeString("parse.nodes", graph.VertexCount.ToString());
            writer.WriteAttributeString("parse.edges", graph.EdgeCount.ToString());
            writer.WriteAttributeString("parse.order", "nodesfirst");
            writer.WriteAttributeString("parse.nodeids", "free");
            writer.WriteAttributeString("parse.edgeids", "free");

            foreach (VertexExp v in graph.Vertices)
            {
                writer.WriteStartElement("node", graphMLNamespace);
                writer.WriteAttributeString("id", v.Id.ToString());
                writer.WriteAttributeString("type", v.Type.ToString());
                writer.WriteEndElement();
            }

            foreach (EdgeExp e in graph.Edges)
            {
                writer.WriteStartElement("edge", graphMLNamespace);
                writer.WriteAttributeString("id", e.GetHashCode().ToString());
                writer.WriteAttributeString("source", ((VertexExp)e.Source).Id.ToString());
                writer.WriteAttributeString("source_type", ((VertexExp)e.Source).Type.ToString());
                writer.WriteAttributeString("target", ((VertexExp)e.Target).Id.ToString());
                writer.WriteAttributeString("target_type", ((VertexExp)e.Target).Type.ToString());
                writer.WriteAttributeString("tag", e.Tag);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndDocument();
        }
    }
}