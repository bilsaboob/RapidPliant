using RapidPliant.App.GrammarDebugger.ViewModels;
using RapidPliant.Mvx;

namespace RapidPliant.App.GrammarDebugger.Views
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
