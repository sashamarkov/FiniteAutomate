using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using GraphSharp.Algorithms.Layout;
using GraphSharp.Algorithms.Layout.Simple.Tree;
using GraphSharp.Algorithms.Layout.Simple.Hierarchical;

namespace FiniteAutomate
{
    public class AlgorithmParametersModel : INotifyPropertyChanged
    {
        int horizontalGap = 10;
        public int HorizontalGap
        {
            get { return horizontalGap; }
            set { 
                horizontalGap = value;
                OnPropertyChanged("HorizontalGap");
            }
        }

        int verticalGap = 10;
        public int VerticalGap
        {
            get { return verticalGap; }
            set
            {
                verticalGap = value;
                OnPropertyChanged("VerticalGap");
            }
        }

        int layerGap = 10;
        public int LayerGap
        {
            get { return layerGap; }
            set
            {
                layerGap = value;
                OnPropertyChanged("LayerGap");
            }
        }

        int vertexGap = 10;
        public int VertexGap
        {
            get { return vertexGap; }
            set
            {
                vertexGap = value;
                OnPropertyChanged("VertexGap");
            }
        }

        LayoutDirection direction = LayoutDirection.LeftToRight;
        public LayoutDirection Direction
        {
            get { return direction; }
            set
            {
                direction = value;
                OnPropertyChanged("Direction");
            }
        }

        SpanningTreeGeneration spanningTreeGeneration = SpanningTreeGeneration.BFS;
        public SpanningTreeGeneration SpanningTreeGeneration
        {
            get { return spanningTreeGeneration; }
            set
            {
                spanningTreeGeneration = value;
                OnPropertyChanged("SpanningTreeGeneration");
            }
        }

        int width = 100;
        public int Width
        {
            get { return width; }
            set
            {
                width = value;
                OnPropertyChanged("Width");
            }
        }

        int height = 100;
        public int Height
        {
            get { return height; }
            set
            {
                height = value;
                OnPropertyChanged("Height");
            }
        }

        double k;
        public double K
        {
            get { return k; }
            set
            {
                k = value;
                OnPropertyChanged("K");
            }
        }

        double attractionMultiplier = 1.2;
        public double AttractionMultiplier
        {
            get { return attractionMultiplier; }
            set
            {
                attractionMultiplier = Math.Round(value,2);
                OnPropertyChanged("AttractionMultiplier");
            }
        }

        double repulsiveMultiplier = 0.6;
        public double RepulsiveMultiplier
        {
            get { return repulsiveMultiplier; }
            set
            {
                repulsiveMultiplier = Math.Round(value, 2);
                OnPropertyChanged("RepulsiveMultiplier");
            }
        }

        int iterationCount = 0;
        public int IterationCount
        {
            get { return iterationCount; }
            set
            {
                iterationCount = value;
                OnPropertyChanged("IterationCount");
            }
        }

        int maxEpoch = 2000;
        public int MaxEpoch
        {
            get { return maxEpoch; }
            set
            {
                maxEpoch = value;
                OnPropertyChanged("MaxEpoch");
            }
        }

        int radiusConstantTime = 5;
        public int RadiusConstantTime
        {
            get { return radiusConstantTime; }
            set
            {
                radiusConstantTime = value;
                OnPropertyChanged("RadiusConstantTime");
            }
        }

        int radius = 0;
        public int Radius
        {
            get { return radius; }
            set
            {
                radius = value;
                OnPropertyChanged("Radius");
            }
        }

        int minRadius = 1;
        public int MinRadius
        {
            get { return minRadius; }
            set
            {
                minRadius = value;
                OnPropertyChanged("MinRadius");
            }
        }

        double adaption = 0.9;
        public double Adaption
        {
            get { return adaption; }
            set
            {
                adaption = value;
                OnPropertyChanged("Adaption");
            }
        }

        double initialAdaption = 0.9;
        public double InitialAdaption
        {
            get { return initialAdaption; }
            set
            {
                initialAdaption = value;
                OnPropertyChanged("InitialAdaption");
            }
        }

        double minAdaption = 0;
        public double MinAdaption
        {
            get { return minAdaption; }
            set
            {
                minAdaption = value;
                OnPropertyChanged("MinAdaption");
            }
        }

        double coolingFactor = 1;
        public double CoolingFactor
        {
            get { return coolingFactor; }
            set
            {
                coolingFactor = value;
                OnPropertyChanged("CoolingFactor");
            }
        }

        double attractionExponent = 1;
        public double AttractionExponent
        {
            get { return attractionExponent; }
            set
            {
                attractionExponent = value;
                OnPropertyChanged("AttractionExponent");
            }
        }

        double repulsiveExponent = 0;
        public double RepulsiveExponent
        {
            get { return repulsiveExponent; }
            set
            {
                repulsiveExponent = value;
                OnPropertyChanged("RepulsiveExponent");
            }
        }

        double gravitationMultiplier = 0.1;
        public double GravitationMultiplier
        {
            get { return gravitationMultiplier; }
            set
            {
                gravitationMultiplier = value;
                OnPropertyChanged("GravitationMultiplier");
            }
        }

        int layerDistance = 15;
        public int LayerDistance
        {
            get { return layerDistance; }
            set
            {
                layerDistance = value;
                OnPropertyChanged("LayerDistance");
            }
        }

        int vertexDistance = 15;
        public int VertexDistance
        {
            get { return vertexDistance; }
            set
            {
                vertexDistance = value;
                OnPropertyChanged("VertexDistance");
            }
        }

        int positionMode = -1;
        public int PositionMode
        {
            get { return positionMode; }
            set
            {
                positionMode = value;
                OnPropertyChanged("PositionMode");
            }
        }

        bool minimizeEdgeLength = true;
        public bool MinimizeEdgeLength
        {
            get { return minimizeEdgeLength; }
            set
            {
                minimizeEdgeLength = value;
                OnPropertyChanged("MinimizeEdgeLength");
            }
        }

        SugiyamaEdgeRoutings edgeRouting = SugiyamaEdgeRoutings.Traditional;
        public SugiyamaEdgeRoutings EdgeRouting
        {
            get { return edgeRouting; }
            set
            {
                edgeRouting = value;
                OnPropertyChanged("EdgeRouting");
            }
        }

        bool optimizeWidth = false;
        public bool OptimizeWidth
        {
            get { return optimizeWidth; }
            set
            {
                optimizeWidth = value;
                OnPropertyChanged("OptimizeWidth");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
