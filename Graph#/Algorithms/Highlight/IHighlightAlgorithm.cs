using QuickGraph;
using System.Collections.Generic;

namespace GraphSharp.Algorithms.Highlight
{
	public interface IHighlightAlgorithm<TVertex, TEdge, TGraph>
		where TVertex : class
		where TEdge : IEdge<TVertex>
		where TGraph : class, IBidirectionalGraph<TVertex, TEdge>
	{
		IHighlightParameters Parameters { get; }

		/// <summary>
		/// Reset the whole highlight.
		/// </summary>
		void ResetHighlight();

        bool OnVertexHighlighting(TVertex vertex, ref List<TEdge> highlightEdges);
		bool OnVertexHighlightRemoving( TVertex vertex );
        bool OnEdgeHighlighting(TEdge edge, ref List<TEdge> highlightEdges);
		bool OnEdgeHighlightRemoving( TEdge edge );

		bool IsParametersSettable(IHighlightParameters parameters);
		bool TrySetParameters(IHighlightParameters parameters);
	}
}