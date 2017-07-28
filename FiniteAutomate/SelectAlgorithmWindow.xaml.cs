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

namespace FiniteAutomate
{
    /// <summary>
    /// Interaction logic for SelectAlgorithmWindow.xaml
    /// </summary>
    public partial class SelectAlgorithmWindow : Window
    {
        SelectAlgorithmModel model;
        MainWindow mainWindow;
        GraphLayout graphLayout;

        public SelectAlgorithmWindow(GraphLayout _graphLayout, MainWindow _mainWindow)
        {
            graphLayout = _graphLayout;
            mainWindow = _mainWindow;
            model = new SelectAlgorithmModel(graphLayout);
            this.DataContext = model;
            InitializeComponent();
        }

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.LayoutAlgorithmType = model.LayoutAlgorithmType;
            mainWindow.EdgeRoutingConstraint = model.EdgeRoutingConstraint;
            mainWindow.OverlapRemovalConstraint = model.OverlapRemovalConstraint;
            mainWindow.OverlapRemovalAlgorithmType = model.OverlapRemovalAlgorithmType;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            model.LayoutAlgorithmType = mainWindow.LayoutAlgorithmType;
            model.EdgeRoutingConstraint = mainWindow.EdgeRoutingConstraint;
            model.OverlapRemovalConstraint = mainWindow.OverlapRemovalConstraint;
            model.OverlapRemovalAlgorithmType = mainWindow.OverlapRemovalAlgorithmType;
        }
    }
}
