using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pliant.Grammars;
using Pliant.RegularExpressions;
using Pliant.Runtime;
using RapidPliant.App.EarleyDebugger.Parsing;
using RapidPliant.App.Services;
using RapidPliant.App.ViewModels.Earley;
using RapidPliant.App.ViewModels.Grammar;
using RapidPliant.Mvx;

namespace RapidPliant.App.EarleyDebugger.ViewModels
{
    public class AppViewModel : RapidViewModel
    {
        public AppViewModel()
        {
        }

        protected override void LoadData()
        {
            //Reset the parse input
            ParseInput = "";
        }

        public ParseRunnerViewModel ParseRunner
        {
            get { return get(() => ParseRunner); }
            set { set(() => ParseRunner, value); }
        }

        public ParseEngineViewModel ParseEngine
        {
            get { return get(() => ParseEngine); }
            set { set(() => ParseEngine, value); }
        }

        public EarleyChartViewModel EarleyChart
        {
            get { return get(() => ParseEngine.EarleyChart); }
            set { set(() => ParseEngine.EarleyChart, value); }
        }

        public string ParseInput
        {
            get { return get(() => ParseRunner.ParseInput); }
            set { set(() => ParseRunner.ParseInput, value); }
        }
    }
}
