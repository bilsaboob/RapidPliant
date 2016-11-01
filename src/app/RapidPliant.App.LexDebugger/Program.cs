using System;

namespace RapidPliant.App.LexDebugger
{
    public static class Program
    {
        [STAThread()]
        public static void Main()
        {
            AppBoostrapper.Run<LexDebuggerApplication>();
        }
    }
}
