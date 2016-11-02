using System.Windows;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.WpfGraphControl;
using RapidPliant.App.ViewModels;
using RapidPliant.Mvx;

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
        }
    }
}
