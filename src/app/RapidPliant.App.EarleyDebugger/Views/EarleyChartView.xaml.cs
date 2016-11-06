using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RapidPliant.App.EarleyDebugger.ViewModels;
using RapidPliant.App.ViewModels.Earley;
using RapidPliant.Mvx;

namespace RapidPliant.App.EarleyDebugger.Views
{
    /// <summary>
    /// Interaction logic for AppView.xaml
    /// </summary>
    public partial class EarleyChartView : IRapidView<EarleyChartViewModel>
    {
        public EarleyChartView()
        {
            InitializeComponent();
        }
    }
}
