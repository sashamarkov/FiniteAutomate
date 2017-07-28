using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;

namespace FiniteAutomate
{
    public class EdgeExp : Edge<object>
    {
        public string Tag { get; set; }
        public int Type { get; set; }

        public EdgeExp(VertexExp source, VertexExp target, string tag)
            : base(source, target)
        {
            Tag = tag;
        }
    }
}
