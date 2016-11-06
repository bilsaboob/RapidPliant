using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pliant.Forest;
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

            ParseEngine = ParseRunner.ParseEngine;
            EarleyChart = ParseRunner.EarleyChart;

            onChange(() => ParseEngine.LastPulsedToken, RefreshParseResults);
        }

        private void RefreshParseResults()
        {
            var engine = ParseEngine.ParseEngine;
            if(engine == null)
                return;

            IInternalForestNode parseRoot = null;
            if (engine.IsAccepted())
            {
                parseRoot = engine.GetParseForestRootNode();
            }
            
            ParseResult.LoadForParseForest(parseRoot);
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
            get { return get(() => EarleyChart); }
            set { set(() => EarleyChart, value); }
        }

        public ParseResultViewModel ParseResult
        {
            get { return get(() => ParseResult); }
            set { set(() => ParseResult, value); }
        }

        public string ParseInput
        {
            get { return get(() => ParseInput); }
            set
            {
                if (ParseRunner != null)
                {
                    ParseRunner.ParseInput = value;
                }
                set(() => ParseInput, value);
            }
        }
    }
}
