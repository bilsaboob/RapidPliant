using System.Windows.Controls;
using RapidPliant.App.GrammarDebugger.Views;

namespace RapidPliant.App.GrammarDebugger
{
    public class GrammarDebuggerApplication : RapidPliantAppliction
    {
        protected override Control CreateMainContent()
        {
            return new AppView();
        }
    }
}