using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.WpfGraphControl;
using RapidPliant.App.ViewModels;
using RapidPliant.Mvx;
using System.Windows.Media;
using RapidPliant.Mvx.Utils;

namespace RapidPliant.App.Controls
{
    public partial class MsaglGraphView : IRapidView<MsaglGraphViewModel>
    {
        public static DependencyProperty GraphPropery = DependencyProperty.Register("Graph", typeof(Graph), typeof(MsaglGraphView), new FrameworkPropertyMetadata(OnGraphPropertyChanged));
        private static void OnGraphPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var graphView = d as MsaglGraphView;
            if (graphView != null)
            {
                graphView.UpdateGraph((Graph)e.NewValue);
            }
        }

        private GraphViewer GraphViewer { get; set; }

        public MsaglGraphView()
        {
            InitializeComponent();

            GraphViewer = new GraphViewer();
            GraphViewer.ZoomOnResizeEnabled = false;
            
            GraphViewer.BindToPanel(GraphViewerPanel);
        }
        
        public Graph Graph
        {
            get { return GetValue(GraphPropery) as Graph; }
            set { SetValue(GraphPropery, value); }
        }

        private void UpdateGraph(Graph graph)
        {
            GraphViewer.Graph = graph;
            GraphViewer.SetInitialTransform(1);
        }

        public Canvas GraphCanvas
        {
            get { return GraphViewer.GraphCanvas; }
        }

        protected double CurrentXOffset
        {
            get { return ((MatrixTransform)GraphCanvas.RenderTransform).Matrix.OffsetX; }
        }

        protected double CurrentYOffset
        {
            get { return ((MatrixTransform)GraphCanvas.RenderTransform).Matrix.OffsetY; }
        }

        private double GetFitFactor(Size rect)
        {
            var graph = GraphViewer.Graph;
            return graph == null ? 1 : Math.Min(rect.Width / graph.Width, rect.Height / graph.Height);
        }

        void SetTransform(double scale, double dx, double dy)
        {
            GraphCanvas.RenderTransform = new MatrixTransform(scale, 0, 0, -scale, dx, dy);
        }
    }
}
