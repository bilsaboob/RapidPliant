using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pliant.Grammars;
using Pliant.RegularExpressions;
using Pliant.Runtime;
using RapidPliant.App.Services;
using RapidPliant.App.ViewModels.Earley;
using RapidPliant.App.ViewModels.Grammar;
using RapidPliant.Mvx;

namespace RapidPliant.App.EarleyDebugger.ViewModels
{
    public class AppViewModel : RapidViewModel
    {
        protected IGrammar _grammar;
        protected ParseEngine _parseEngine;
        protected ParseRunner _parseRunner;

        protected IDebuggerGrammarService _grammarService;

        public AppViewModel()
        {
            Grammars = new ObservableCollection<GrammarViewModel>();

            _grammarService = new DebuggerGrammarService();
        }

        protected override void LoadData()
        {
            //Get the available grammars!
            var grammars = _grammarService.GetAvailableGrammars().Select(grammarInfo => GrammarViewModel.CreateFromGrammarInfo(grammarInfo));
            Grammars = new ObservableCollection<GrammarViewModel>(grammars);

            //Set the first grammar per default
            Grammar = Grammars.FirstOrDefault();
        }

        public GrammarViewModel Grammar
        {
            get { return get(() => Grammar); }
            set
            {
                set(() => Grammar, value);
            }
        }
        public ParseEngineViewModel ParseEngine { get { return get(() => ParseEngine); } set { set(() => ParseEngine, value); } }
        public DebuggerParseRunnerViewModel ParseRunner { get { return get(() => ParseRunner); } set { set(() => ParseRunner, value); } }

        public ObservableCollection<GrammarViewModel> Grammars { get { return get(() => Grammars); } set { set(() => Grammars, value); } }

        public bool IsStarted { get { return get(() => IsStarted); } set { set(() => IsStarted, value); } }
        public EarleyChartViewModel EarleyChart { get { return get(() => EarleyChart); } set { set(() => EarleyChart, value); } }

        public string ParseInput
        {
            get { return get(() => ParseInput); }
            set
            {
                set(() => ParseInput, value);
            }
        }

        public string SeletedGrammarName
        {
            get { return get(() => SeletedGrammarName); }
            set
            {
                _grammar = _grammarService.GetGrammarByName(value);

                set(() => SeletedGrammarName, value);
            }
        }

        public void StartOrRestartParsing()
        {
            if (IsStarted)
            {
                RestartParsing();
            }
            else
            {
                StartParsing();
                IsStarted = true;
            }
        }

        private void StartParsing()
        {
            if (ParseInput == null)
                ParseInput = "test";

            RestartParsing();
        }

        public void RestartParsing()
        {
            //Reload the grammar view model
            _grammar = _grammarService.GetGrammarByType(Grammar.GrammarType);
            Grammar.LoadGrammar(_grammar);

            //Initialize the parse engine and runner
            _parseEngine = new ParseEngine(_grammar, new ParseEngineOptions(optimizeRightRecursion: true));
            ParseEngine.EarleyChart = EarleyChart;
            ParseEngine.LoadParseEngine(_parseEngine);

            _parseRunner = new ParseRunner(_parseEngine, ParseInput);
            ParseRunner.EarleyChart = EarleyChart;
            ParseRunner.ParseEngine = ParseEngine;
            ParseRunner.LoadParseRunner(_parseRunner);

            EarleyChart.LoadFromChart(_parseEngine.Chart);
        }

        public void ScanNext()
        {
            ParseRunner.ScanNext();
        }

        public void LexNext()
        {
            ParseRunner.LexNext();
        }
    }
}
