using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;

namespace FiniteAutomate
{
    public class State
    {
        public VertexExp V { get; set; }
        public string CurrState { get; set; }
        public string[] NextStateDescr { get; set; }
        public int[] NextState { get; set; }
        //public State(List<string> InputChars, AdjacencyGraph<VertexExp, TaggedEdge<VertexExp, string>> g, VertexExp vertex)
        public State(List<string> InputChars, BidirectionalGraph<object, IEdge<object>> g, object vertex)
        {
            V = (VertexExp)vertex;
            CurrState = Descr((VertexExp)vertex);
            NextStateDescr = new string[InputChars.Count];
            NextState = new int[InputChars.Count];
            //foreach (TaggedEdge<VertexExp, string> e in g.OutEdges(vertex))
            foreach (EdgeExp e in g.OutEdges(vertex))
            {
                if ((e.Tag.IndexOf(",") == -1) && (InputChars.IndexOf(e.Tag) != -1))
                {
                    NextStateDescr[InputChars.IndexOf(e.Tag)] = Descr((VertexExp)e.Target);
                    NextState[InputChars.IndexOf(e.Tag)] = ((VertexExp)e.Target).Id;
                }
                else
                {
                    string[] TagPart = e.Tag.Split(',');
                    foreach (string part in TagPart)
                    {
                        if (InputChars.IndexOf(part.Trim()) != -1)
                        {
                            NextStateDescr[InputChars.IndexOf(part.Trim())] = Descr((VertexExp)e.Target);
                            NextState[InputChars.IndexOf(part.Trim())] = ((VertexExp)e.Target).Id;
                        }
                    }
                }
            }
        }

        public string Descr(VertexExp vertex)
        {
            string Name = " " + vertex.Id.ToString() + " ";
            switch (vertex.Type)
            {
                case 0: return ">" + Name;
                //case -1: return "*" + Name;
                case -1: return Name + "*";
            }
            return Name;
        }
    }
}
