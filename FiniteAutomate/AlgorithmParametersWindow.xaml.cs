using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GraphSharp.Controls;
using GraphSharp.Algorithms.OverlapRemoval;
using GraphSharp.Algorithms.Layout.Simple.Tree;
using GraphSharp.Algorithms.Layout.Simple.FDP;
using GraphSharp.Algorithms.Layout.Simple.Hierarchical;

namespace FiniteAutomate
{
    /// <summary>
    /// Interaction logic for AlgoritmParametersWindow.xaml
    /// </summary>
    public partial class AlgorithmParametersWindow : Window 
    {
        GraphLayout graphLayout;
        string LayoutAlgorithmType;
        AlgorithmParametersModel model;

        public AlgorithmParametersWindow(GraphLayout _graphLayout, string _LayoutAlgorithmType)
        {
            graphLayout = _graphLayout;
            LayoutAlgorithmType = _LayoutAlgorithmType;
            model = new AlgorithmParametersModel();
            this.DataContext = model;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (LayoutAlgorithmType == "Tree") SimpleTreeLayoutParametersGroup.Visibility = Visibility.Visible;
            else SimpleTreeLayoutParametersGroup.Visibility = Visibility.Collapsed;

            if (LayoutAlgorithmType == "BoundedFR") BoundedFRLayoutParametersGroup.Visibility = Visibility.Visible;
            else BoundedFRLayoutParametersGroup.Visibility = Visibility.Collapsed;

            if (LayoutAlgorithmType == "ISOM") ISOMLayoutParametersGroup.Visibility = Visibility.Visible;
            else ISOMLayoutParametersGroup.Visibility = Visibility.Collapsed;

            if (LayoutAlgorithmType == "LinLog") LinLogLayoutParametersGroup.Visibility = Visibility.Visible;
            else LinLogLayoutParametersGroup.Visibility = Visibility.Collapsed;

            if (LayoutAlgorithmType == "EfficientSugiyama") EfficientSugiyamaLayoutParametersGroup.Visibility = Visibility.Visible;
            else EfficientSugiyamaLayoutParametersGroup.Visibility = Visibility.Collapsed;

            model.HorizontalGap = (int)graphLayout.OverlapRemovalParameters.HorizontalGap;
            model.VerticalGap = (int)graphLayout.OverlapRemovalParameters.VerticalGap;
            if (LayoutAlgorithmType == "Tree")
            {
                if ((graphLayout.LayoutParameters != null) && (graphLayout.LayoutParameters is SimpleTreeLayoutParameters))
                {
                    model.LayerGap = (int)(graphLayout.LayoutParameters as SimpleTreeLayoutParameters).LayerGap;
                    model.VertexGap = (int)(graphLayout.LayoutParameters as SimpleTreeLayoutParameters).VertexGap;
                    model.Direction = (graphLayout.LayoutParameters as SimpleTreeLayoutParameters).Direction;
                    model.SpanningTreeGeneration = (graphLayout.LayoutParameters as SimpleTreeLayoutParameters).SpanningTreeGeneration;
                }
                else
                {
                    var lp = new SimpleTreeLayoutParameters();
                    model.LayerGap = (int)lp.LayerGap;
                    model.VertexGap = (int)lp.VertexGap;
                    model.Direction = lp.Direction;
                    model.SpanningTreeGeneration = lp.SpanningTreeGeneration;

                    model.HorizontalGap = 45;
                }
            }
            if (LayoutAlgorithmType == "BoundedFR")
            {
                if ((graphLayout.LayoutParameters != null) && (graphLayout.LayoutParameters is BoundedFRLayoutParameters))
                {
                    model.Width = (int)(graphLayout.LayoutParameters as BoundedFRLayoutParameters).Width;
                    model.Height = (int)(graphLayout.LayoutParameters as BoundedFRLayoutParameters).Height;
                    model.K = (graphLayout.LayoutParameters as BoundedFRLayoutParameters).K;
                    model.AttractionMultiplier = (graphLayout.LayoutParameters as BoundedFRLayoutParameters).AttractionMultiplier;
                    model.RepulsiveMultiplier = (graphLayout.LayoutParameters as BoundedFRLayoutParameters).RepulsiveMultiplier;
                    model.IterationCount = (graphLayout.LayoutParameters as BoundedFRLayoutParameters).IterationLimit;
                }
                else
                {
                    var lp = new BoundedFRLayoutParameters();
                    model.Width = (int)lp.Width;
                    model.Height = (int)lp.Height;
                    model.K = lp.K;
                    model.AttractionMultiplier = lp.AttractionMultiplier;
                    model.RepulsiveMultiplier = lp.RepulsiveMultiplier;
                    model.IterationCount = lp.IterationLimit;
                }
            }
            if (LayoutAlgorithmType == "ISOM")
            {
                if ((graphLayout.LayoutParameters != null) && (graphLayout.LayoutParameters is ISOMLayoutParameters))
                {
                    model.Width = (int)(graphLayout.LayoutParameters as ISOMLayoutParameters).Width;
                    model.Height = (int)(graphLayout.LayoutParameters as ISOMLayoutParameters).Height;
                    model.MaxEpoch = (graphLayout.LayoutParameters as ISOMLayoutParameters).MaxEpoch;
                    model.RadiusConstantTime = (graphLayout.LayoutParameters as ISOMLayoutParameters).RadiusConstantTime;
                    model.Radius = (graphLayout.LayoutParameters as ISOMLayoutParameters).InitialRadius;
                    model.MinRadius = (graphLayout.LayoutParameters as ISOMLayoutParameters).MinRadius;
                    model.MinAdaption = (graphLayout.LayoutParameters as ISOMLayoutParameters).MinAdaption;
                    model.InitialAdaption = (graphLayout.LayoutParameters as ISOMLayoutParameters).InitialAdaption;
                    model.CoolingFactor = (graphLayout.LayoutParameters as ISOMLayoutParameters).CoolingFactor;
                }
                else
                {
                    var lp = new ISOMLayoutParameters();
                    model.Width = (int)lp.Width;
                    model.Height = (int)lp.Height;
                    model.MaxEpoch = lp.MaxEpoch;
                    model.RadiusConstantTime = lp.RadiusConstantTime;
                    model.Radius = lp.InitialRadius;
                    model.MinRadius = lp.MinRadius;
                    model.MinAdaption = lp.MinAdaption;
                    model.InitialAdaption = lp.InitialAdaption;
                    model.CoolingFactor = lp.CoolingFactor;
                }
            }
            if (LayoutAlgorithmType == "LinLog")
            {
                if ((graphLayout.LayoutParameters != null) && (graphLayout.LayoutParameters is LinLogLayoutParameters))
                {
                    model.AttractionExponent = (graphLayout.LayoutParameters as LinLogLayoutParameters).AttractionExponent;
                    model.RepulsiveExponent = (graphLayout.LayoutParameters as LinLogLayoutParameters).RepulsiveExponent;
                    model.GravitationMultiplier = (graphLayout.LayoutParameters as LinLogLayoutParameters).GravitationMultiplier;
                    model.IterationCount = (graphLayout.LayoutParameters as LinLogLayoutParameters).IterationCount;
                }
                else
                {
                    var lp = new LinLogLayoutParameters();
                    model.AttractionExponent = lp.AttractionExponent;
                    model.RepulsiveExponent = lp.RepulsiveExponent;
                    model.GravitationMultiplier = lp.GravitationMultiplier;
                    model.IterationCount = lp.IterationCount;
                }
            }
            if (LayoutAlgorithmType == "EfficientSugiyama")
            {
                if ((graphLayout.LayoutParameters != null) && (graphLayout.LayoutParameters is EfficientSugiyamaLayoutParameters))
                {
                    model.LayerDistance = (int)(graphLayout.LayoutParameters as EfficientSugiyamaLayoutParameters).LayerDistance;
                    model.VertexDistance = (int)(graphLayout.LayoutParameters as EfficientSugiyamaLayoutParameters).VertexDistance;
                    model.PositionMode = (graphLayout.LayoutParameters as EfficientSugiyamaLayoutParameters).PositionMode;
                    model.MinimizeEdgeLength = (graphLayout.LayoutParameters as EfficientSugiyamaLayoutParameters).MinimizeEdgeLength;
                    model.EdgeRouting = (graphLayout.LayoutParameters as EfficientSugiyamaLayoutParameters).EdgeRouting;
                    model.OptimizeWidth = (graphLayout.LayoutParameters as EfficientSugiyamaLayoutParameters).OptimizeWidth;
                }
                else
                {
                    var lp = new EfficientSugiyamaLayoutParameters();
                    model.LayerDistance = (int)lp.LayerDistance;
                    model.VertexDistance = (int)lp.VertexDistance;
                    model.PositionMode = lp.PositionMode;
                    model.MinimizeEdgeLength = lp.MinimizeEdgeLength;
                    model.EdgeRouting = lp.EdgeRouting;
                    model.OptimizeWidth = lp.OptimizeWidth;
                }
            }
        }

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            var orp = new OverlapRemovalParameters();
            orp.HorizontalGap = model.HorizontalGap;
            orp.VerticalGap = model.VerticalGap;
            graphLayout.OverlapRemovalParameters = orp;

            if (LayoutAlgorithmType == "Tree")
            {
                var lp = new SimpleTreeLayoutParameters();
                lp.LayerGap = model.LayerGap;
                lp.VertexGap = model.VertexGap;
                lp.Direction = model.Direction;
                lp.SpanningTreeGeneration = model.SpanningTreeGeneration;
                graphLayout.LayoutParameters = lp;
            }
            if (LayoutAlgorithmType == "BoundedFR")
            {
                var lp = new BoundedFRLayoutParameters();
                lp.Width = model.Width;
                lp.Height = model.Height;
                lp.AttractionMultiplier = model.AttractionMultiplier;
                lp.RepulsiveMultiplier = model.RepulsiveMultiplier;
                lp.IterationLimit = model.IterationCount;
                graphLayout.LayoutParameters = lp;
            }
            if (LayoutAlgorithmType == "ISOM")
            {
                var lp = new ISOMLayoutParameters();
                lp.Width = model.Width;
                lp.Height = model.Height;
                lp.MaxEpoch = model.MaxEpoch;
                lp.RadiusConstantTime = model.RadiusConstantTime;
                lp.InitialRadius = model.Radius;
                lp.MinRadius = model.MinRadius;
                lp.MinAdaption = model.MinAdaption;
                lp.InitialAdaption = model.InitialAdaption;
                lp.CoolingFactor = model.CoolingFactor;
                graphLayout.LayoutParameters = lp;
            }
            if (LayoutAlgorithmType == "LinLog")
            {
                var lp = new LinLogLayoutParameters();
                lp.AttractionExponent = model.AttractionExponent;
                lp.RepulsiveExponent = model.RepulsiveExponent;
                lp.GravitationMultiplier = model.GravitationMultiplier;
                lp.IterationCount = model.IterationCount;
                graphLayout.LayoutParameters = lp;
            }
            if (LayoutAlgorithmType == "EfficientSugiyama")
            {
                var lp = new EfficientSugiyamaLayoutParameters();
                lp.LayerDistance = model.LayerDistance;
                lp.VertexDistance = model.VertexDistance;
                lp.PositionMode = model.PositionMode;
                lp.MinimizeEdgeLength = model.MinimizeEdgeLength;
                lp.EdgeRouting = model.EdgeRouting;
                lp.OptimizeWidth = model.OptimizeWidth;
                graphLayout.LayoutParameters = lp;
            }

            this.Close();
        }
    }
}
