using QuickGraph;
using System;
using QuickGraph.Algorithms.ShortestPath;
using QuickGraph.Algorithms.Observers;
using System.Collections.Generic;

namespace GraphSharp.Algorithms.Highlight
{
    public class DijkstraHighlightAlgorithm<TVertex, TEdge, TGraph> : HighlightAlgorithmBase<TVertex, TEdge, TGraph, IHighlightParameters>
        where TVertex : class
        where TEdge : IEdge<TVertex>
        where TGraph : class, IBidirectionalGraph<TVertex, TEdge>
    {
        public DijkstraHighlightAlgorithm(
            IHighlightController<TVertex, TEdge, TGraph> controller,
            IHighlightParameters parameters)
            : base(controller, parameters)
        {
        }

        private void ClearSemiHighlights()
        {
            foreach (var vertex in Controller.SemiHighlightedVertices)
                Controller.RemoveSemiHighlightFromVertex(vertex);

            foreach (var edge in Controller.SemiHighlightedEdges)
                Controller.RemoveSemiHighlightFromEdge(edge);
        }

        private void ClearAllHighlights()
        {
            ClearSemiHighlights();

            foreach (var vertex in Controller.HighlightedVertices)
                Controller.RemoveHighlightFromVertex(vertex);

            foreach (var edge in Controller.HighlightedEdges)
                Controller.RemoveHighlightFromEdge(edge);
        }

        /// <summary>
        /// Resets the semi-highlights according to the actually
        /// highlighted vertices/edges.
        /// 
        /// This method should be called if the graph changed, 
        /// or the highlights should be resetted.
        /// </summary>
        public override void ResetHighlight()
        {
            ClearAllHighlights();
        }

        private List<TEdge> HighlightPath(TVertex vertex)
        {
            List<TEdge> highlightEdges = new List<TEdge>();

            Func<TEdge, double> edgeCost = e => 1; // constant cost
            // creating the algorithm instance
            var dijkstra =
                new DijkstraShortestPathAlgorithm<TVertex, TEdge>(Controller.Graph, edgeCost);

            // creating the observer
            var vis = new VertexPredecessorRecorderObserver<TVertex, TEdge>();

            TVertex root = null;
            foreach (TVertex v in Controller.Graph.Vertices)
            {
                if (v.ToString() == "0")
                {
                    root = v;
                    break;
                }
            }

            if (root == null) return highlightEdges;

            // compute and record shortest paths
            using (ObserverScope.Create(dijkstra, vis))
                dijkstra.Compute(root);

            // vis can create all the shortest path in the graph
            IEnumerable<TEdge> path;
            IEnumerable<TEdge> outEdges;
            vis.TryGetPath(vertex, out path);
            if (path == null) return highlightEdges;
            foreach (var e in path)
            {
                Controller.SemiHighlightEdge(e, "Edge");
                Controller.SemiHighlightVertex(e.Source, "Source");
                
                outEdges = Controller.Graph.OutEdges(e.Source);
                foreach (var oute in outEdges)
                {
                    if (oute.Target == e.Source)
                    {
                        Controller.SemiHighlightEdge(oute, "Edge");
                        highlightEdges.Add(oute);
                    }
                }
                
                highlightEdges.Add(e);
            }

            Controller.SemiHighlightVertex(vertex, "Target");
            
            outEdges = Controller.Graph.OutEdges(vertex);
            foreach (var e in outEdges)
            {
                if (e.Target == vertex)
                {
                    Controller.SemiHighlightEdge(e, "Edge");
                    highlightEdges.Add(e);
                }
            }
            
            return highlightEdges;
        }

        public override bool OnVertexHighlighting(TVertex vertex, ref List<TEdge> highlightEdges)
        {
            ClearAllHighlights();

            if (vertex == null || !Controller.Graph.ContainsVertex(vertex))
                return false;

            highlightEdges = HighlightPath(vertex);

            /*
            //semi-highlight the in-edges, and the neighbours on their other side
            foreach (var edge in Controller.Graph.InEdges(vertex))
            {
                Controller.SemiHighlightEdge(edge, "InEdge");
                if (edge.Source == vertex || Controller.IsHighlightedVertex(edge.Source))
                    continue;

                Controller.SemiHighlightVertex(edge.Source, "Source");
            }

            //semi-highlight the out-edges
            foreach (var edge in Controller.Graph.OutEdges(vertex))
            {
                Controller.SemiHighlightEdge(edge, "OutEdge");
                if (edge.Target == vertex || Controller.IsHighlightedVertex(edge.Target))
                    continue;

                Controller.SemiHighlightVertex(edge.Target, "Target");
            }
            Controller.HighlightVertex(vertex, "None");
            */

            return true;
        }

        public override bool OnVertexHighlightRemoving(TVertex vertex)
        {
            ClearAllHighlights();
            return true;
        }

        public override bool OnEdgeHighlighting(TEdge edge, ref List<TEdge> highlightEdges)
        {
            ClearAllHighlights();

            highlightEdges = HighlightPath(edge.Target);

            /*
            //highlight the source and the target
            if (Equals(edge, default(TEdge)) || !Controller.Graph.ContainsEdge(edge))
                return false;

            Controller.HighlightEdge(edge, null);
            Controller.SemiHighlightVertex(edge.Source, "Source");
            Controller.SemiHighlightVertex(edge.Target, "Target");
            */
            return true;
        }

        public override bool OnEdgeHighlightRemoving(TEdge edge)
        {
            ClearAllHighlights();
            return true;
        }
    }
}
