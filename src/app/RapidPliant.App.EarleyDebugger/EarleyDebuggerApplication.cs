using System.Windows.Controls;
using RapidPliant.App.EarleyDebugger.Views;

namespace RapidPliant.App.EarleyDebugger
{
    public class EarleyDebuggerApplication : RapidPliantAppliction
    {
        protected override Control CreateMainContent()
        {
            return new AppView();
        }
    }
}