using System.Windows;
using System.Windows.Controls;
using RapidPliant.App.EarleyDebugger.Views;

namespace RapidPliant.App.EarleyDebugger
{
    public class EarleyDebuggerApplication : RapidPliantAppliction
    {
        protected override Window CreateMainWindow()
        {
            var window = base.CreateMainWindow();
            window.Width = 1400;
            window.Height = 600;
            window.Title = "RapidPliant - Earley Debugger";
            return window;
        }

        protected override Control CreateMainContent()
        {
            return new AppView();
        }
    }
}