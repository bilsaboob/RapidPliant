using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pliant.Grammars;
using Pliant.Runtime;
using Pliant.Tokens;
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
            ActiveLexemes = new ObservableCollection<LexemeViewModel>();
            CompletedLexemes = new ObservableCollection<LexemeViewModel>();
            DiscaredLexemes = new ObservableCollection<LexemeViewModel>();
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

        public ObservableCollection<LexemeViewModel> ActiveLexemes { get { return get(() => ActiveLexemes); } set { set(() => ActiveLexemes, value); } }

        public ObservableCollection<LexemeViewModel> CompletedLexemes { get { return get(() => CompletedLexemes); } set { set(() => CompletedLexemes, value); } }

        public ObservableCollection<LexemeViewModel> DiscaredLexemes { get { return get(() => DiscaredLexemes); } set { set(() => DiscaredLexemes, value); } }

        public InputViewModel Input { get { return get(() => Input); } set { set(() => Input, value); } }
        
        public bool IsStarted { get { return get(() => IsStarted); } set { set(() => IsStarted, value); } }

        public bool CanLexNext { get { return get(() => CanLexNext); } set { set(() => CanLexNext, value); } }

        public bool CanPulseNext { get { return get(() => CanPulseNext); } set { set(() => CanPulseNext, value); } }

        public bool ReachedEndOfInput { get { return get(() => ReachedEndOfInput); } set { set(() => ReachedEndOfInput, value); } }

        public bool LastReadFailed { get { return get(() => LastReadFailed); } set { set(() => LastReadFailed, value); } }

        public bool PulsedNewSet { get { return get(() => PulsedNewSet); } set { set(() => PulsedNewSet, value); } }

        public string ParseInput
        {
            get { return get(() => ParseInput); }
            set
            {
                Input.LoadForInput(value);
                RefreshInput();
                set(() => ParseInput, value);
            }
        }
        
        public int InputPosition { get { return get(() => InputPosition); } set { set(() => InputPosition, value); } }

        public int InputLineNo { get { return get(() => InputLineNo); } set { set(() => InputLineNo, value); } }

        public int InputColNo { get { return get(() => InputColNo); } set { set(() => InputColNo, value); } }
        #endregion

        #region ViewModel Actions
        public void StartOrRestartParsing()
        {
            ActiveLexemes.Clear();
            DiscaredLexemes.Clear();
            CompletedLexemes.Clear();
            
            TargetParseEngine = new ParseEngine(_grammar);
            DebugParseEngine = new DebugParseEngine(TargetParseEngine);
            
            TargetParseRunner = new ParseRunner(DebugParseEngine, ParseInput);
            DebugParseRunner = new DebugParseRunner(TargetParseRunner);
            
            //Loadthe parse engine
            ParseEngine.LoadParseEngine(DebugParseEngine);
            
            //Refresh chart and init the lexemes
            RefreshChart();
            RefreshLexemes(new List<ILexeme>(), DebugParseRunner.ActiveLexemes.ToList());

            Reset();
        }

        public void PulseNext()
        {
            while (true)
            {
                if(!CanLexNext || !CanPulseNext) 
                    break;

                Lex(false);

                if (ParseEngine.HasPulsedForPulsePass)
                {
                    ParseEngine.StartNewPulsePass();
                    break;
                }
            }
        }

        public void LexNext()
        {
            Lex();
        }

        private void Lex(bool startNewPulsePass = true)
        {
            RefreshCanLexPulse();

            var preReadLexemes = DebugParseRunner.ActiveLexemes.ToList();

            //Do the read
            var readResult = Read();
            LastReadFailed = !readResult;
            CanLexNext = !LastReadFailed;
            CanPulseNext = !LastReadFailed;
            
            RefreshInput(DebugParseRunner.Position);

            var postReadLexems = DebugParseRunner.ActiveLexemes.ToList();

            RefreshLexemes(preReadLexemes, postReadLexems);

            RefreshChart(startNewPulsePass);
        }

        private void RefreshLexemes(List<ILexeme> preReadLexemes, List<ILexeme> postReadLexemes)
        {
            //The new active lexemes are the ones post the read
            var newActiveLexemes = postReadLexemes.ToList();

            //Check the lexemes that were removed, either a spelling was captured or simply didn't match anymore
            var removedLexemes = preReadLexemes.Except(postReadLexemes).ToList();

            //Check for any new lexemes
            var newLexemes = postReadLexemes.Except(preReadLexemes).ToList();
            
            ActiveLexemes.Clear();
            DiscaredLexemes.Clear();
            CompletedLexemes.Clear();

            foreach (var lexeme in newActiveLexemes)
            {
                var lexemeVm = new LexemeViewModel().LoadForLexeme(lexeme);

                var isIgnore = _grammar.Ignores.Contains(lexeme.LexerRule);
                if (isIgnore)
                {
                    lexemeVm.DisplayLabel = lexemeVm.DisplayLabel.TrimEnd() + " (ignore)";
                }

                if (newLexemes.Contains(lexeme))
                {
                    lexemeVm.DisplayLabel = lexemeVm.DisplayLabel.TrimEnd() + " **new**";
                }
                
                ActiveLexemes.Add(lexemeVm);
            }

            foreach (var lexeme in removedLexemes)
            {
                var lexemeVm = new LexemeViewModel().LoadForLexeme(lexeme);

                var isIgnore = _grammar.Ignores.Contains(lexeme.LexerRule);
                if (isIgnore)
                {
                    lexemeVm.DisplayLabel = lexemeVm.DisplayLabel.TrimEnd() + " (ignore)";
                }

                if (lexeme.IsAccepted())
                {
                    CompletedLexemes.Add(lexemeVm);
                }
                else
                {
                    DiscaredLexemes.Add(lexemeVm);
                }
            }
        }
        #endregion

        #region Internal helpers
        protected void Reset()
        {
            Input.Reset();
            RefreshInput();

            //Load the chart
            EarleyChart.LoadFromChart(TargetParseEngine.Chart);
            
            RefreshCanLexPulse();
            
            //Start a new pulse pass
            ParseEngine.StartNewPulsePass();

            IsStarted = true;
        }

        protected virtual void RefreshCanLexPulse()
        {
            if (DebugParseRunner == null)
            {
                CanLexNext = false;
                CanPulseNext = false;
            }
            else if (DebugParseRunner.EndOfStream())
            {
                CanLexNext = false;
                CanPulseNext = false;
                ReachedEndOfInput = true;
            }
            else
            {
                CanLexNext = true;
                CanPulseNext = true;
                ReachedEndOfInput = false;
            }
        }
        
        protected bool Read()
        {
            return DebugParseRunner.Read();
        }

        protected void RefreshChart(bool startNewPulsePass = true)
        {
            EarleyChart.RefreshFromChart();

            if (RefreshLastSetForPulse() && startNewPulsePass)
            {
                //Start the next pulse pass
                ParseEngine.StartNewPulsePass();
            }
        }
        
        protected bool RefreshLastSetForPulse()
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

                return true;
            }

            return false;
        }

        private void RefreshInput(int moveToPosition = 0)
        {
            if (moveToPosition > 0)
                Input.MoveNext(moveToPosition);
            
            InputPosition = Input.Position;
            InputLineNo = Input.LineNo;
            InputColNo = Input.ColNo;
        }
        #endregion
    }
}
