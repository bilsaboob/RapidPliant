using System.Windows.Controls;
using RapidPliant.App.LexDebugger.Views;

namespace RapidPliant.App.LexDebugger
{
    public class LexDebuggerApplication : RapidPliantAppliction
    {
        protected override Control CreateMainContent()
        {
            return new AppView();
        }
    }
}