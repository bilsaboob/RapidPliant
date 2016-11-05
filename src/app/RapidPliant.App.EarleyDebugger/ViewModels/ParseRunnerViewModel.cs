using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pliant.Grammars;
using Pliant.Runtime;
using RapidPliant.App.EarleyDebugger.Parsing;
using RapidPliant.App.EarleyDebugger.ViewModels;
using RapidPliant.App.Services;
using RapidPliant.App.ViewModels.Earley;
using RapidPliant.Mvx;

namespace RapidPliant.App.ViewModels.Grammar
{
    public class ParseRunnerViewModel : RapidViewModel
    {
        protected IGrammar _grammar;
        
        protected IDebuggerGrammarService GrammarService { get; private set; }

        protected ParseEngine TargetParseEngine { get; private set; }
        protected DebugParseEngine DebugParseEngine { get; private set; }

        protected ParseRunner TargetParseRunner { get; private set; }
        protected DebugParseRunner DebugParseRunner { get; private set; }
        
        public ParseRunnerViewModel()
        {
            GrammarService = new DebuggerGrammarService();

            Grammars = new ObservableCollection<GrammarViewModel>();
        }

        #region Load
        protected override void LoadData()
        {
            //Get the available grammars!
            var grammars = GrammarService.GetAvailableGrammars().Select(grammarInfo => GrammarViewModel.CreateFromGrammarInfo(grammarInfo));
            Grammars = new ObservableCollection<GrammarViewModel>(grammars);

            //Set the first grammar per default
            Grammar = Grammars.FirstOrDefault();
        }
        #endregion

        #region ViewModel members
        public ParseEngineViewModel ParseEngine { get { return get(() => ParseEngine); } set { set(() => ParseEngine, value); } }

        public EarleyChartViewModel EarleyChart { get { return get(() => EarleyChart); } set { set(() => EarleyChart, value); } }
        
        public ObservableCollection<GrammarViewModel> Grammars { get { return get(() => Grammars); } set { set(() => Grammars, value); } }

        public GrammarViewModel Grammar
        {
            get { return get(() => Grammar); }
            set
            {
                if (value != null && value.GrammarType != null)
                {
                    _grammar = GrammarService.GetGrammarByType(value.GrammarType);
                }
                
                set(() => Grammar, value);
            }
        }

        public bool IsStarted { get { return get(() => IsStarted); } set { set(() => IsStarted, value); } }

        public bool CanLexNext { get { return get(() => CanLexNext); } set { set(() => CanLexNext, value); } }

        public bool ReachedEndOfInput { get { return get(() => ReachedEndOfInput); } set { set(() => ReachedEndOfInput, value); } }

        public bool LastReadFailed { get { return get(() => LastReadFailed); } set { set(() => LastReadFailed, value); } }
        
        public string ParseInput { get { return get(() => ParseInput); } set { set(() => ParseInput, value); } }
        #endregion

        #region ViewModel Actions
        public void StartOrRestartParsing()
        {
            TargetParseEngine = new ParseEngine(_grammar);
            DebugParseEngine = new DebugParseEngine(TargetParseEngine);

            TargetParseRunner = new ParseRunner(DebugParseEngine, ParseInput);
            DebugParseRunner = new DebugParseRunner(TargetParseRunner);
            
            //Loadthe parse engine
            ParseEngine.LoadParseEngine(DebugParseEngine);

            Reset();
        }

        public void LexNext()
        {
            RefreshCanLex();

            BeginRead();

            var readResult = Read();
            LastReadFailed = !readResult;
            CanLexNext = !LastReadFailed;

            EndRead();

            RefreshChart();
        }
        #endregion

        #region Internal helpers
        protected void Reset()
        {
            //Load the chart
            EarleyChart.LoadFromChart(TargetParseEngine.Chart);
            
            RefreshCanLex();
            
            //Start a new pulse pass
            ParseEngine.StartNewPulsePass();

            IsStarted = true;
        }

        protected virtual void RefreshCanLex()
        {
            if (DebugParseRunner == null)
            {
                CanLexNext = false;
            }
            else if (DebugParseRunner.EndOfStream())
            {
                CanLexNext = false;
                ReachedEndOfInput = true;
            }
            else
            {
                CanLexNext = true;
                ReachedEndOfInput = false;
            }
        }

        protected void BeginRead()
        {
        }

        protected bool Read()
        {
            return DebugParseRunner.Read();
        }

        protected void EndRead()
        {
        }

        protected void RefreshChart()
        {
            EarleyChart.RefreshFromChart();
            RefreshLastSetForPulse();
        }
        
        protected void RefreshLastSetForPulse()
        {
            ParseEngine.RefreshForPulsePass();
            if (ParseEngine.HasPulsedForPulsePass)
            {
                var lastSet = EarleyChart.GetLastEarleySet();
                if (lastSet != null)
                {
                    //Try updating the set with the pulsed token if any
                    lastSet.PulsedToken = ParseEngine.LastPulsedToken;
                    lastSet.PulsedTokenSuccess = ParseEngine.LastPulsedTokenSuccess;
                }

                //Start the next pulse pass
                ParseEngine.StartNewPulsePass();
            }
        }
        #endregion
    }
}
