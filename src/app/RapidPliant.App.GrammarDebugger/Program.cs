using System;

namespace RapidPliant.App.GrammarDebugger
{
    public static class Program
    {
        [STAThread()]
        public static void Main()
        {
            AppBoostrapper.Run<GrammarDebuggerApplication>();
        }
    }
}
