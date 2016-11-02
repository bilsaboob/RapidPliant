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
        public ParseRunnerViewModel ParseRunner { get { return get(() => ParseRunner); } set { set(() => ParseRunner, value); } }

        public ObservableCollection<GrammarViewModel> Grammars { get { return get(() => Grammars); } set { set(() => Grammars, value); } }

        public string SeletedGrammarName
        {
            get { return get(() => SeletedGrammarName); }
            set
            {
                _grammar = _grammarService.GetGrammarByName(value);

                set(() => SeletedGrammarName, value);
            }
        }

        public string ParseInput { get { return get(() => ParseInput); } set { set(() => ParseInput, value); } }

        public void StartParsing()
        {
            if (ParseInput == null)
                ParseInput = "";

            ResetParsing();
        }

        public void ResetParsing()
        {
            //Reload the grammar view model
            Grammar = new GrammarViewModel();
            Grammar.LoadGrammar(_grammar);

            //Initialize the parse engine and runner
            _parseEngine = new ParseEngine(_grammar, new ParseEngineOptions(optimizeRightRecursion: true));
            ParseEngine.LoadParseEngine(_parseEngine);

            _parseRunner = new ParseRunner(_parseEngine, ParseInput);
            ParseRunner.LoadParseRunner(_parseRunner);
            ParseRunner.ParseEngine = ParseEngine;
        }
    }
}
