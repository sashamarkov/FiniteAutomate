using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using GraphSharp.Controls;

namespace FiniteAutomate
{
    public class SelectAlgorithmModel : INotifyPropertyChanged
    {
        public SelectAlgorithmModel(GraphLayout _graphLayout)
        {
            graphLayout = _graphLayout;
        }

        public GraphLayout graphLayout { get; set; }

        string layoutAlgorithmType = "Tree";
        public string LayoutAlgorithmType
        {
            get { return layoutAlgorithmType; }
            set
            {
                layoutAlgorithmType = value;
                OnPropertyChanged("LayoutAlgorithmType");
            }
        }

        AlgorithmConstraints edgeRoutingConstraint = AlgorithmConstraints.Must;
        public AlgorithmConstraints EdgeRoutingConstraint
        {
            get { return edgeRoutingConstraint; }
            set
            {
                edgeRoutingConstraint = value;
                OnPropertyChanged("EdgeRoutingConstraint");
            }
        }

        AlgorithmConstraints overlapRemovalConstraint = AlgorithmConstraints.Must;
        public AlgorithmConstraints OverlapRemovalConstraint
        {
            get { return overlapRemovalConstraint; }
            set
            {
                overlapRemovalConstraint = value;
                OnPropertyChanged("OverlapRemovalConstraint");
            }
        }

        string overlapRemovalAlgorithmType = "FSA";
        public string OverlapRemovalAlgorithmType
        {
            get { return overlapRemovalAlgorithmType; }
            set
            {
                overlapRemovalAlgorithmType = value;
                OnPropertyChanged("OverlapRemovalAlgorithmType");
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
