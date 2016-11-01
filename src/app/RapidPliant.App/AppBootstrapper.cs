using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.App
{
    public static class AppBoostrapper
    {
        public static void Run<TApplication>()
            where TApplication : RapidPliantAppliction, new()
        {
            var app = new TApplication();
            app.Run();
        }
    }
}
