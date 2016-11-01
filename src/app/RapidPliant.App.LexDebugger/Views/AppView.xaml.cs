using RapidPliant.App.LexDebugger.ViewModels;
using RapidPliant.Mvx;

namespace RapidPliant.App.LexDebugger.Views
{
    /// <summary>
    /// Interaction logic for AppView.xaml
    /// </summary>
    public partial class AppView : IRapidView<AppViewModel>
    {
        public AppView()
        {
            InitializeComponent();
        }
    }
}
