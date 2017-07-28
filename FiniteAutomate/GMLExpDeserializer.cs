using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using QuickGraph;
using System.Diagnostics.Contracts;
using System.Collections;

namespace FiniteAutomate
{
    class GMLExpDeserializer
    {
        XmlReader reader;
        BidirectionalGraph<object, IEdge<object>> graph;
        Hashtable fastGraph = new Hashtable();
        string graphMLNamespace = "";

        public GMLExpDeserializer(XmlReader reader)
        {
            this.reader = reader;
            graph = new BidirectionalGraph<object, IEdge<object>>();
        }

        public BidirectionalGraph<object, IEdge<object>> Deserialize()
        {
            ReadHeader();
            ReadGraphHeader();
            ReadElements();
            return graph;
        }

        private void ReadHeader()
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "graphml")
                {
                    graphMLNamespace = reader.NamespaceURI;
                    return;
                }
            }

            throw new ArgumentException("Не найден узел graphml");
        }

        private void ReadGraphHeader()
        {
            if (!reader.ReadToDescendant("graph", graphMLNamespace))
                throw new ArgumentException("Не найден узел graph");
        }

        private void ReadElements()
        {
            Contract.Requires(reader.Name == "graph" && reader.NamespaceURI == graphMLNamespace, "Неправильная позиция указателя");

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.NamespaceURI == graphMLNamespace)
                {
                    switch (reader.Name)
                    { 
                        case "node":
                            ReadVertex();
                            break;
                        case "edge":
                            ReadEdge();
                            break;
                        default:
                            throw new InvalidOperationException(String.Format("Неправильная позиция указателя {0}:{1}", reader.NamespaceURI, reader.Name));
                    }
                }
            }
        }

        private static string ReadAttributeValue(XmlReader reader, string attributeName)
        {
            Contract.Requires(reader != null);
            Contract.Requires(attributeName != null);
            reader.MoveToAttribute(attributeName);
            if (!reader.ReadAttributeValue())
                throw new ArgumentException("Атрибут " + attributeName + " отсутствует");
            return reader.Value;
        }

        private VertexExp AddVertex(int id, int type)
        {
            FastState state;
            if (fastGraph[id] == null)
            {
                state = new FastState(id, (FastStateType)type);
                fastGraph.Add(id, state);
                state.Vertex = new VertexExp(state.Id, (int)state.Type);
                graph.AddVertex(state.Vertex);

            }
            else
            {
                state = fastGraph[id] as FastState;
                state.Type = (FastStateType)type;
            }
            return state.Vertex;
        }

        private void ReadVertex()
        {
            Contract.Assert(reader.NodeType == XmlNodeType.Element && reader.Name == "node" && reader.NamespaceURI == graphMLNamespace);

            using (var subReader = reader.ReadSubtree())
            {
                int id = int.Parse(ReadAttributeValue(reader, "id"));
                int type = int.Parse(ReadAttributeValue(reader, "type"));
                AddVertex(id, type);
            }
        }

        private void ReadEdge()
        {
            Contract.Assert(reader.NodeType == XmlNodeType.Element && reader.Name == "edge" && reader.NamespaceURI == graphMLNamespace);

            using (var subReader = reader.ReadSubtree())
            {
                int id = int.Parse(ReadAttributeValue(reader, "id"));
                
                int sourceId = int.Parse(ReadAttributeValue(reader, "source"));
                int sourceType = int.Parse(ReadAttributeValue(reader, "source_type"));
                VertexExp source = AddVertex(sourceId, sourceType);
                
                int targetId = int.Parse(ReadAttributeValue(reader, "target"));
                int targetType = int.Parse(ReadAttributeValue(reader, "target_type"));
                VertexExp target = AddVertex(targetId, targetType);
                
                string tag = ReadAttributeValue(reader, "tag");

                var e = new EdgeExp(source, target, tag); 
                //EdgeExp e = new EdgeExp(source, target, tag);
                graph.AddEdge(e);
            }
        }
    }
}